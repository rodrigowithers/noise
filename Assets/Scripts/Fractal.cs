using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

public class Fractal : MonoBehaviour
{
    private struct FractalPart
    {
        public float3 Direction;
        public float3 WorldPosition;

        public quaternion Rotation;
        public quaternion WorldRotation;

        public float SpinAngle;
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    private struct UpdateFractalLevelJob : IJobFor
    {
        public float SpinAngleDelta;
        public float Scale;

        [ReadOnly] public NativeArray<FractalPart> Parents;
        [WriteOnly] public NativeArray<float3x4> Matrices;

        public NativeArray<FractalPart> Parts;

        public void Execute(int index)
        {
            var parent = Parents[index / 5];
            var part = Parts[index];

            part.SpinAngle += SpinAngleDelta;

            part.WorldRotation = mul(parent.WorldRotation, mul(part.Rotation, quaternion.RotateY(part.SpinAngle)));
            part.WorldPosition = parent.WorldPosition + mul(parent.WorldRotation, 1.5f * Scale * part.Direction);

            Parts[index] = part;

            float3x3 r = float3x3(part.WorldRotation) * Scale;
            Matrices[index] = float3x4(r.c0, r.c1, r.c2, part.WorldPosition);
        }
    }

    private static readonly int 
        _matricesId = Shader.PropertyToID("_Matrices"),
        _mainColorId = Shader.PropertyToID("_MainColor");

    [SerializeField, Range(2, 8)] private int _depth = 4;

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;

    [SerializeField] private Gradient _color;
    
    private static float3[] _directions =
    {
        up(),
        left(),
        right(),
        forward(),
        back(),
    };

    private static quaternion[] _rotations =
    {
        quaternion.identity,
        quaternion.RotateZ(0.5f * PI),
        quaternion.RotateZ(-0.5f * PI),
        quaternion.RotateX(0.5f * PI),
        quaternion.RotateX(-0.5f * PI)
    };

    private NativeArray<FractalPart>[] _parts;
    private NativeArray<float3x4>[] _matrices;

    private ComputeBuffer[] _matricesBuffer;

    private static MaterialPropertyBlock _propertyBlock;

    [ContextMenu("Screenshot")]
    public void TakeScreenshot()
    {
        string folderPath = "Screenshots/";
 
        if (!System.IO.Directory.Exists(folderPath))
            System.IO.Directory.CreateDirectory(folderPath);
 
        var screenshotName = 
            "Screenshot_" + 
            System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + 
            ".jpg";
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName));
        Debug.Log(folderPath + screenshotName);
    }
    
    private FractalPart CreatePart(int childIndex) => new FractalPart
    {
        Direction = _directions[childIndex],
        Rotation = _rotations[childIndex],
    };

    private void OnValidate()
    {
        if (_parts == null || !enabled) return;


        OnDisable();
        OnEnable();
    }

    private void OnDisable()
    {
        for (var i = 0; i < _matricesBuffer.Length; i++)
        {
            _matricesBuffer[i].Release();
            _parts[i].Dispose();
            _matrices[i].Dispose();
        }

        _parts = null;
        _matrices = null;
        _matricesBuffer = null;
    }

    private void OnEnable()
    {
        _parts = new NativeArray<FractalPart>[_depth];
        _matrices = new NativeArray<float3x4>[_depth];
        _matricesBuffer = new ComputeBuffer[_depth];

        _propertyBlock ??= new MaterialPropertyBlock();

        int stride = 12 * 4;

        for (int i = 0, length = 1; i < _parts.Length; i++, length *= 5)
        {
            _parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            _matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            _matricesBuffer[i] = new ComputeBuffer(length, stride);
        }

        _parts[0][0] = CreatePart(0);
        for (int li = 1; li < _parts.Length; li++)
        {
            var levelParts = _parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                for (int c = 0; c < 5; c++)
                {
                    levelParts[fpi + c] = CreatePart(c);
                }
            }
        }
    }

    private void Update()
    {
        float angleDelta = 0.01f * PI * Time.deltaTime * 0;

        var rootPart = _parts[0][0];
        rootPart.SpinAngle += angleDelta;
        rootPart.WorldRotation = mul(rootPart.Rotation, quaternion.RotateY(rootPart.SpinAngle));
        _parts[0][0] = rootPart;

        float3x3 r = float3x3(rootPart.WorldRotation);
        _matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.WorldPosition);

        float scale = 1f;
        JobHandle jobHandle = default;

        for (int li = 1; li < _parts.Length; li++)
        {
            scale *= 0.5f;

            jobHandle = new UpdateFractalLevelJob
            {
                SpinAngleDelta = angleDelta,
                Scale = scale,
                Parents = _parts[li - 1],
                Matrices = _matrices[li],
                Parts = _parts[li]
            }.ScheduleParallel(_parts[li].Length, 5, jobHandle);
        }

        jobHandle.Complete();

        var bounds = new Bounds(Vector3.zero, Vector3.one * 3);

        for (var i = 0; i < _matricesBuffer.Length; i++)
        {
            var buffer = _matricesBuffer[i];
            buffer.SetData(_matrices[i]);
            _propertyBlock.SetBuffer(_matricesId, buffer);
            _propertyBlock.SetColor(_mainColorId, _color.Evaluate(i / (_matricesBuffer.Length - 1f)));
            Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds, buffer.count, _propertyBlock);
        }
    }
}
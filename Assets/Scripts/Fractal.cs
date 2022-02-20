using UnityEngine;
using Unity.Mathematics;

public class Fractal : MonoBehaviour
{
    private struct FractalPart
    {
        public Vector3 Direction;
        public Vector3 WorldPosition;

        public Quaternion Rotation;
        public Quaternion WorldRotation;

        public float SpinAngle;
    }

    private static readonly int _matricesId = Shader.PropertyToID("_Matrices");

    [SerializeField, Range(1, 8)] private int _depth = 4;

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;

    private static Vector3[] _directions =
    {
        Vector3.up,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back,
    };

    private static Quaternion[] _rotations =
    {
        quaternion.identity,
        Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(0f, 0f, -90f),
        Quaternion.Euler(90f, 0f, 0f),
        Quaternion.Euler(-90f, 0f, 0f)
    };

    private FractalPart[][] _parts;
    private Matrix4x4[][] _matrices;

    private ComputeBuffer[] _matricesBuffer;

    private static MaterialPropertyBlock _propertyBlock;

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
        }

        _parts = null;
        _matrices = null;
        _matricesBuffer = null;
    }

    private void OnEnable()
    {
        _parts = new FractalPart[_depth][];
        _matrices = new Matrix4x4[_depth][];
        _matricesBuffer = new ComputeBuffer[_depth];

        _propertyBlock ??= new MaterialPropertyBlock();

        int stride = 16 * 4;

        for (int i = 0, length = 1; i < _parts.Length; i++, length *= 5)
        {
            _parts[i] = new FractalPart[length];
            _matrices[i] = new Matrix4x4[length];
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
        float angleDelta = 20f * Time.deltaTime;

        var rootPart = _parts[0][0];
        rootPart.SpinAngle += angleDelta;
        rootPart.WorldRotation = rootPart.Rotation * Quaternion.Euler(0, rootPart.SpinAngle, 0);
        _parts[0][0] = rootPart;

        _matrices[0][0] = Matrix4x4.TRS(rootPart.WorldPosition, rootPart.WorldRotation, Vector3.one);

        float scale = 1f;

        for (int li = 1; li < _parts.Length; li++)
        {
            scale *= 0.5f;

            var parentParts = _parts[li - 1];
            var levelParts = _parts[li];
            var levelMatrices = _matrices[li];

            for (int fpi = 0; fpi < levelParts.Length; fpi++)
            {
                var parent = parentParts[fpi / 5];
                var part = levelParts[fpi];

                part.SpinAngle += angleDelta;

                part.WorldRotation = parent.WorldRotation * (part.Rotation * Quaternion.Euler(0, part.SpinAngle, 0));
                part.WorldPosition = parent.WorldPosition + parent.WorldRotation * (1.5f * scale * part.Direction);

                levelParts[fpi] = part;
                
                levelMatrices[fpi] = Matrix4x4.TRS(part.WorldPosition, part.WorldRotation, scale * Vector3.one);
            }
        }

        var bounds = new Bounds(Vector3.zero, Vector3.one * 3);

        for (var i = 0; i < _matricesBuffer.Length; i++)
        {
            var buffer = _matricesBuffer[i];
            buffer.SetData(_matrices[i]);
            _propertyBlock.SetBuffer(_matricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds, buffer.count, _propertyBlock);
        }
    }
}
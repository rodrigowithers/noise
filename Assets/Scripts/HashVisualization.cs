using System;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class HashVisualization : MonoBehaviour
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor
    {
        [WriteOnly] public NativeArray<uint> Hashes;

        public int Resolution;
        public float InverseResolution;

        public SmallXXHash Hash;

        public void Execute(int index)
        {
            int v = (int)floor(InverseResolution * index + 0.00001f);
            int u = index - Resolution * v - Resolution / 2;
            v -= Resolution / 2;

            Hashes[index] = Hash.Eat(u).Eat(v);
        }
    }

    private static int
        _hashesId = Shader.PropertyToID("_Hashes"),
        _configId = Shader.PropertyToID("_Config");

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField] private int _seed;

    [Space] [SerializeField, Range(1, 512)]
    private int _resolution = 16;

    [SerializeField, Range(-2, 2)] private float _verticalOffset = 1;

    private NativeArray<uint> _hashes;
    private ComputeBuffer _hashesBuffer;
    private MaterialPropertyBlock _propertyBlock;

    private void OnEnable()
    {
        int lenght = _resolution * _resolution;
        _hashes = new NativeArray<uint>(lenght, Allocator.Persistent);
        _hashesBuffer = new ComputeBuffer(lenght, 4);

        new HashJob
        {
            Hash = SmallXXHash.Seed(_seed),
            Hashes = _hashes,
            Resolution = _resolution,
            InverseResolution = 1f / _resolution
        }.ScheduleParallel(_hashes.Length, _resolution, default).Complete();

        _hashesBuffer.SetData(_hashes);

        _propertyBlock ??= new MaterialPropertyBlock();
        _propertyBlock.SetBuffer(_hashesId, _hashesBuffer);
        _propertyBlock.SetVector(_configId, 
            new Vector4(_resolution, 1f / _resolution, _verticalOffset / _resolution));
    }

    private void OnDisable()
    {
        _hashes.Dispose();

        _hashesBuffer.Release();
        _hashesBuffer = null;
    }

    private void OnValidate()
    {
        if (_hashesBuffer == null || !enabled) return;

        OnDisable();
        OnEnable();
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material,
            new Bounds(Vector3.zero, Vector3.one), _hashes.Length, _propertyBlock);
    }
}
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
        [WriteOnly] public NativeArray<uint> hashes;

        public void Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }

    private static int
        _hashesId = Shader.PropertyToID("_Hashes"),
        _configId = Shader.PropertyToID("_Config");

    [SerializeField] private Mesh _instancedMesh;
    [SerializeField] private Material _material;
    [SerializeField, Range(1, 512)] private int _resolution = 16;

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
            hashes = _hashes
        }.ScheduleParallel(_hashes.Length, _resolution, default).Complete();
        
        _hashesBuffer.SetData(_hashes);

        _propertyBlock ??= new MaterialPropertyBlock();
        _propertyBlock.SetBuffer(_hashesId, _hashesBuffer);
        _propertyBlock.SetVector(_configId, new Vector4(_resolution, 1f/_resolution));
    }

    private void OnDisable()
    {
        _hashes.Dispose();
        _hashesBuffer.Release();
        _hashesBuffer = null;
    }

    private void OnValidate()
    {
        if (_hashesBuffer != null && enabled)
        {
            OnDisable();
            OnEnable();
        }   
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedProcedural(
            _instancedMesh, 0, _material, 
            new Bounds(Vector3.zero, Vector3.one),
            _hashes.Length, _propertyBlock
            );
    }
}
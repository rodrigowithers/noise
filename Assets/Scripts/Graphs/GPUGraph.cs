﻿using UnityEngine;

namespace Graphs
{
    public class GPUGraph : MonoBehaviour
    {
        private static readonly int 
            _positionsId = Shader.PropertyToID("_Positions"),
            _resolutionId = Shader.PropertyToID("_Resolution"),
            _stepId = Shader.PropertyToID("_Step"),
            _timeId = Shader.PropertyToID("_Time");

        [SerializeField, Range(4, 256)] private int _resolution = 10;

        [SerializeField] private ComputeShader _compute;
        [SerializeField] private Material _material;
        [SerializeField] private Mesh _mesh;
        
        private ComputeBuffer _positionBuffer;

        private void UpdateFunctionOnGPU()
        {
            float step = 2f / _resolution;
            int groups = Mathf.CeilToInt(_resolution / 8f);

            _compute.SetInt(_resolutionId, _resolution);
            _compute.SetFloat(_stepId, step);
            _compute.SetFloat(_timeId, Time.time);

            _compute.SetBuffer(0, _positionsId, _positionBuffer);
            _compute.Dispatch(0, groups, groups, 1);

            var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / _resolution));
            Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds, _positionBuffer.count);
        }

        private void OnEnable()
        {
            _positionBuffer = new ComputeBuffer(_resolution * _resolution, 3 * 4);
        }

        private void OnDisable()
        {
            _positionBuffer.Release();
            _positionBuffer = null;
        }

        private void Update()
        {
            // _time += Time.deltaTime * 1f;
            // var step = 2f / _resolution;
            //
            // float v = 0.5f * step - 1f;
            // for (int i = 0, x = 0, z = 0; i < _resolution; i++, x++)
            // {
            //     if (x == _resolution)
            //     {
            //         x = 0;
            //
            //         z++;
            //         v = (z + 0.5f) * step - 1f;
            //     }
            //
            //     float u = (x + 0.5f) * step - 1f;
            // }

            UpdateFunctionOnGPU();
        }
    }
}
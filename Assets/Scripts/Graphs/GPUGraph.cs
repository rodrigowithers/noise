using UnityEngine;

namespace Graphs
{
    public class GPUGraph : MonoBehaviour
    {
        private static readonly int 
            _positionsId = Shader.PropertyToID("_Positions"),
            _resolutionId = Shader.PropertyToID("_Resolution"),
            _stepId = Shader.PropertyToID("_Step"),
            _timeId = Shader.PropertyToID("_Time");

        const int _maxResolution = 512;
        [SerializeField, Range(4, _maxResolution)] private int _resolution = 512;

        [SerializeField] private ComputeShader _compute;
        [SerializeField] private Material _material;
        [SerializeField] private Mesh _mesh;

        [SerializeField] private FunctionLibrary.FunctionName _currentGraphFunction;
        
        private ComputeBuffer _positionsBuffer;

        private void UpdateFunctionOnGPU()
        {
            float step = 2f / _resolution;
            int groups = Mathf.CeilToInt(_resolution / 8f);
            var kernelIndex = (int) _currentGraphFunction;

            _compute.SetInt(_resolutionId, _resolution);
            _compute.SetFloat(_stepId, step);
            _compute.SetFloat(_timeId, Time.time);

            _material.SetBuffer(_positionsId, _positionsBuffer);
            _material.SetFloat(_stepId, step);

            _compute.SetBuffer(kernelIndex, _positionsId, _positionsBuffer);
            _compute.Dispatch(kernelIndex, groups, groups, 1);

            var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / _resolution));
            Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds, _resolution * _resolution);
        }

        private void OnEnable()
        {
            _positionsBuffer = new ComputeBuffer(_maxResolution * _maxResolution, 3 * 4);
        }

        private void OnDisable()
        {
            _positionsBuffer.Release();
            _positionsBuffer = null;
        }

        private void Update()
        {
            UpdateFunctionOnGPU();
        }
    }
}
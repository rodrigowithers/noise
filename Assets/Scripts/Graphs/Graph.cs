using UnityEngine;

namespace Graphs
{
    public class Graph : MonoBehaviour
    {
        [SerializeField] private Transform _point;

        [SerializeField, Range(16, 256)] private int _resolution = 10;
    
        [SerializeField, Range(0f, 1f)] private float _morph = 0.5f;

        [SerializeField] private FunctionLibrary.FunctionName _currentGraphFunction;

        private float _time;

        private Transform[] _points;

        private FunctionLibrary.MathFunction _graphFunction;

        private void Awake()
        {
            _points = new Transform[_resolution * _resolution];
            var step = 2f / _resolution;

            var scale = Vector3.one * step;

            for (int i = 0; i < _points.Length; i++)
            {
                Transform point = Instantiate(_point, transform, false);
                point.localScale = scale;
                _points[i] = point;
            }
        }

        private void Update()
        {
            _time += Time.deltaTime * 1f;
            _graphFunction = FunctionLibrary.GetFunction(_currentGraphFunction);
            var step = 2f / _resolution;

            float v = 0.5f * step - 1f;
            for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++)
            {
                if (x == _resolution)
                {
                    x = 0;

                    z++;
                    v = (z + 0.5f) * step - 1f;
                }

                float u = (x + 0.5f) * step - 1f;

                _points[i].localPosition = FunctionLibrary.Morph(u, v, _time,
                    FunctionLibrary.GetFunction(FunctionLibrary.FunctionName.Torus),
                    FunctionLibrary.GetFunction(FunctionLibrary.FunctionName.Sphere),
                    _morph);

                // _points[i].localPosition = _graphFunction(u, v, _time);
            }
        }
    }
}
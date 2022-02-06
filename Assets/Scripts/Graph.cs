using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] private Transform _point;

    [SerializeField, Range(10, 100)] private int _resolution = 10;

    [SerializeField] private FunctionLibrary.FunctionName _currentGraphFunction;

    private Transform[] _points;

    private FunctionLibrary.MathFunction _graphFunction;

    private void Awake()
    {
        _points = new Transform[_resolution * _resolution];
        var step = 2f / _resolution;

        Vector3 position = Vector3.zero;
        var scale = Vector3.one * step;

        for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++)
        {
            if (x == _resolution)
            {
                x = 0;
                z++;
            }
            
            Transform point = Instantiate(_point, transform, false);

            position.x = (x + 0.5f) * step - 1f;
            position.z = (z + 0.5f) * step - 1f;

            point.localPosition = position;
            point.localScale = scale;

            _points[i] = point;
        }
    }

    private float _t;
    
    private void Update()
    {
        var time = Time.time;
        _t += Time.deltaTime * 1f;
        
        _graphFunction = FunctionLibrary.GetFunction(_currentGraphFunction);

        for (int i = 0; i < _points.Length; i++)
        {
            Transform point = _points[i];
            Vector3 position = point.localPosition;

            position.y = _graphFunction.Invoke(position.x, position.z, _t);
            point.localPosition = position;
        }
    }
}
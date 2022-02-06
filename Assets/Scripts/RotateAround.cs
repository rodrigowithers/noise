using UnityEngine;

public class RotateAround : MonoBehaviour
{
    [SerializeField] private float _speed = 10;
    [SerializeField] private Transform _target;
    
    private float _angle;

    private void Update()
    {
        _angle += Time.deltaTime * _speed;
    }

    private void LateUpdate()
    {
        var rad = Mathf.Deg2Rad * _angle;
        transform.position = new Vector3(
            2 * Mathf.Cos(rad),
            2,
            2 * Mathf.Sin(rad)
        );
        
        transform.LookAt(_target);
    }
}
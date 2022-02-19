using System;
using UnityEngine;
using Unity.Mathematics;

public class Fractal : MonoBehaviour
{
    private struct FractalPart
    {
        public Vector3 Direction;
        public Quaternion Rotation;

        public Transform Transform;
    }

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

    private FractalPart CreatePart(int levelIndex, int childIndex, float scale)
    {
        var part = new GameObject($"Fractal Part L {levelIndex} C {childIndex}");
        part.transform.SetParent(transform, false);
        part.transform.localScale = Vector3.one * scale;

        part.AddComponent<MeshFilter>().mesh = _mesh;
        part.AddComponent<MeshRenderer>().material = _material;

        return new FractalPart
        {
            Direction = _directions[childIndex],
            Rotation = _rotations[childIndex],
            Transform = part.transform
        };
    }

    private void Awake()
    {
        _parts = new FractalPart[_depth][];
        float scale = 1;

        for (int i = 0, length = 1; i < _parts.Length; i++, length *= 5)
        {
            _parts[i] = new FractalPart[length];
        }

        _parts[0][0] = CreatePart(0, 0, scale);
        for (int li = 1; li < _parts.Length; li++)
        {
            scale *= 0.5f;

            var levelParts = _parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                for (int c = 0; c < 5; c++)
                {
                    levelParts[fpi + c] = CreatePart(li, c, scale);
                }
            }
        }
    }

    private void Update()
    {
        for (int li = 1; li < _parts.Length; li++)
        {
            var parentParts = _parts[li - 1];
            var levelParts = _parts[li];

            for (int fpi = 0; fpi < levelParts.Length; fpi++)
            {
                Transform parentTransform = parentParts[fpi / 5].Transform;
                var part = levelParts[fpi];
                var parentRotation = parentTransform.localRotation;

                part.Transform.localRotation =
                    parentRotation * part.Rotation;

                part.Transform.localPosition =
                    parentTransform.localPosition +
                    parentRotation * (1.5f * part.Transform.localScale.x * part.Direction);
            }
        }
    }
}
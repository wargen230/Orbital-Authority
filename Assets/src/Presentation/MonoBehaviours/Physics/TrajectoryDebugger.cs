using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryDebugger : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool recordOnStart = true;
    [SerializeField] private float lineWidth = 0.2f;
    [SerializeField] private int maxPoints = 2000;
    [SerializeField] private float minDistanceBetweenPoints = 0.5f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.white;

    private LineRenderer lineRenderer;
    private readonly List<Vector3> points = new();
    private Vector3 lastPoint;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.material = lineMaterial != null
            ? lineMaterial
            : new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        if (target == null)
        {
            target = transform;
        }
    }

    private void Start()
    {
        if (recordOnStart)
        {
            AddPoint(target.position);
        }
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        if (points.Count == 0 || Vector3.Distance(lastPoint, target.position) >= minDistanceBetweenPoints)
        {
            AddPoint(target.position);
        }
    }

    public void Clear()
    {
        points.Clear();
        lineRenderer.positionCount = 0;
    }

    private void AddPoint(Vector3 point)
    {
        points.Add(point);
        lastPoint = point;

        if (points.Count > maxPoints)
        {
            points.RemoveAt(0);
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}

using System.Collections.Generic;
using UnityEngine;

public class TrajectoryDebugger : MonoBehaviour
{
    [SerializeField] private List<Transform> targets;
    [SerializeField] private bool recordOnStart = true;
    [SerializeField] private float lineWidth = 0.2f;
    [SerializeField] private int maxPoints = 2000;
    [SerializeField] private float minDistanceBetweenPoints = 0.5f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.white;

    private class TrajectoryData
    {
        public LineRenderer lineRenderer;
        public List<Vector3> points = new();
        public Vector3 lastPoint;
    }

    private Dictionary<Transform, TrajectoryData> trajectories = new();

    private void Awake()
    {
        if (targets == null || targets.Count == 0)
        {
            targets = new List<Transform> { transform };
        }

        foreach (var target in targets)
        {
            CreateTrajectoryForTarget(target);
        }
    }

    private void Start()
    {
        if (recordOnStart)
        {
            foreach (var kvp in trajectories)
            {
                AddPoint(kvp.Key, kvp.Key.position);
            }
        }
    }

    private void Update()
    {
        foreach (var target in targets)
        {
            if (target == null) continue;
            if (!trajectories.ContainsKey(target)) continue;

            var data = trajectories[target];
            if (data.points.Count == 0 || 
                Vector3.Distance(data.lastPoint, target.position) >= minDistanceBetweenPoints)
            {
                AddPoint(target, target.position);
            }
        }
    }

    private void CreateTrajectoryForTarget(Transform target)
    {
        GameObject lineObj = new GameObject($"Trajectory_{target.name}");
        lineObj.transform.SetParent(transform);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        
        lr.useWorldSpace = true;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = 0;
        lr.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        trajectories[target] = new TrajectoryData
        {
            lineRenderer = lr,
            points = new List<Vector3>(),
            lastPoint = target.position
        };
    }

    private void AddPoint(Transform target, Vector3 point)
    {
        var data = trajectories[target];
        data.points.Add(point);
        data.lastPoint = point;

        if (data.points.Count > maxPoints)
            data.points.RemoveAt(0);

        data.lineRenderer.positionCount = data.points.Count;
        data.lineRenderer.SetPositions(data.points.ToArray());
    }

    public void Clear(Transform target = null)
    {
        if (target != null && trajectories.ContainsKey(target))
        {
            var data = trajectories[target];
            data.points.Clear();
            data.lineRenderer.positionCount = 0;
        }
        else
        {
            foreach (var data in trajectories.Values)
            {
                data.points.Clear();
                data.lineRenderer.positionCount = 0;
            }
        }
    }
}
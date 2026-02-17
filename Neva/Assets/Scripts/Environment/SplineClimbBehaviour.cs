using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(EdgeCollider2D))]
public class SplineClimbBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineContainer splineContainer;

    [Header("Sampling")]
    [Range(2, 200)]
    [SerializeField] private int resolution = 50;

    private EdgeCollider2D edgeCollider;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        lineRenderer = GetComponent<LineRenderer>();

        UpdateCollider();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!edgeCollider)
            edgeCollider = GetComponent<EdgeCollider2D>();
        if (!lineRenderer)
            lineRenderer = GetComponent<LineRenderer>();

        UpdateCollider();
        UpdateVissuals();
    }
#endif

    public void UpdateVissuals()
    {
        if (lineRenderer == null || splineContainer.Spline == null)
            return;

        List<Vector3> points = new List<Vector3>();
        lineRenderer.positionCount = resolution + 1;

        // Sample along spline length
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;

            Vector3 worldPoint = splineContainer.EvaluatePosition(t);
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

            points.Add(new Vector2(localPoint.x, localPoint.y));
        }

        lineRenderer.SetPositions(points.ToArray());
    }

    public void UpdateCollider()
    {
        if (splineContainer == null || splineContainer.Spline == null)
            return;

        List<Vector2> points = new List<Vector2>();

        // Sample along spline length
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;

            Vector3 worldPoint = splineContainer.EvaluatePosition(t);
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

            points.Add(new Vector2(localPoint.x, localPoint.y));
        }

        edgeCollider.points = points.ToArray();
    }
}
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryRenderer : MonoBehaviour
{
    LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void ShowTrajectory(Vector3 origin, Vector3 speed)
    {
        Vector3[] points = new Vector3[100];
        lineRenderer.positionCount = points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            float time = i * 0.1f;
            points[i] = origin + speed * time + Physics.gravity * time * time / 2f;
            if (points[i].y < 0) // Assuming the ground is at y = 0
            {
                lineRenderer.positionCount = i + 1;
                break;
            }
        }

        lineRenderer.SetPositions(points);
    }

    public void HideTrajectory()
    {
        lineRenderer.positionCount = 0;
    }
}

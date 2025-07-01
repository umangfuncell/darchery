using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CircleMeshWithHorizontalDivisions : MonoBehaviour
{
    [System.Serializable]
    public class SegmentData
    {
        public float angle; // Internal angle of the segment
        public Color color; // Color of the segment
        public int subdivide = 5; // Number of horizontal divisions for this segment
        public float startRadius = 0.3f; // Start radius for the ring
        public float endRadius = 1f; // End radius for the ring
    }

    public float radius = 1f; // Radius of the circle
    public float bullseyeRadius = 0.3f; // Radius of the bullseye in the center (smaller than the circle's radius)
    public List<SegmentData> segmentDataList = new List<SegmentData>
    {
        new SegmentData { angle = 60f, color = Color.red, subdivide = 3, startRadius = 0.3f, endRadius = 0.6f },
        new SegmentData { angle = 120f, color = Color.green, subdivide = 4, startRadius = 0.6f, endRadius = 1f },
        new SegmentData { angle = 90f, color = Color.blue, subdivide = 5, startRadius = 0.5f, endRadius = 1f },
        new SegmentData { angle = 90f, color = Color.yellow, subdivide = 6, startRadius = 0.4f, endRadius = 0.7f }
    }; // List of segment data
    public int smoothness = 20; // Number of additional points along the curve of each segment
    public bool flipNormals = false; // Toggle for flipping the direction of normals
    public bool useURPLitShader = false; // Toggle to use URP Lit Shader

    private GameObject segmentsParent; // Parent object for segments

    private void Start()
    {
        UpdateCircle(); // Initialize the circle
    }

    /// <summary>
    /// Updates the circle dynamically, including all segments and the bullseye.
    /// </summary>
    public void UpdateCircle()
    {
        DivideCircleIntoSegments(); // Re-generate segments
    }

    /// <summary>
    /// Divides the circle into segments based on the segment data list.
    /// </summary>
    private void DivideCircleIntoSegments()
    {
        // Ensure the parent object exists
        if (segmentsParent == null)
        {
            segmentsParent = new GameObject("Segments");
            segmentsParent.transform.parent = this.transform;
        }

        // Clear old segments by disabling or reusing them
        for (int i = 0; i < segmentsParent.transform.childCount; i++)
        {
            segmentsParent.transform.GetChild(i).gameObject.SetActive(false);
        }

        // Generate bullseye
        GenerateBullseye();

        float currentAngle = 0f; // Start at 0 degrees
        for (int i = 0; i < segmentDataList.Count; i++)
        {
            SegmentData segmentData = segmentDataList[i];
            float nextAngle = currentAngle + segmentData.angle;

            GameObject segment;
            if (i < segmentsParent.transform.childCount)
            {
                // Reuse existing child object
                segment = segmentsParent.transform.GetChild(i).gameObject;
                segment.SetActive(true);
            }
            else
            {
                // Create a new GameObject for the segment
                segment = new GameObject($"Segment {i + 1}");
                segment.transform.parent = segmentsParent.transform;

                // Add MeshFilter and MeshRenderer
                segment.AddComponent<MeshFilter>();
                segment.AddComponent<MeshRenderer>();
            }

            // Generate and assign the segment mesh with rings
            MeshFilter meshFilter = segment.GetComponent<MeshFilter>();
            meshFilter.mesh = GenerateSegmentMesh(currentAngle, nextAngle, segmentData);

            // Assign the selected shader and color to the segment
            MeshRenderer meshRenderer = segment.GetComponent<MeshRenderer>();
            Material material = CreateMaterial();
            material.color = segmentData.color; // Assign the color from the segment data
            meshRenderer.material = material;

            // Add MeshCollider
            MeshCollider meshCollider = segment.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = segment.AddComponent<MeshCollider>();
            }
            meshCollider.sharedMesh = meshFilter.mesh;

            // Update the current angle for the next segment
            currentAngle = nextAngle;
        }
    }

    /// <summary>
    /// Generates a mesh for a single segment of the circle with horizontal divisions and rings.
    /// </summary>
    /// <param name="startAngle">Starting angle of the segment in degrees.</param>
    /// <param name="endAngle">Ending angle of the segment in degrees.</param>
    /// <param name="segmentData">The data for the current segment, including subdivision and ring radii.</param>
    /// <returns>A Mesh object for the segment.</returns>
    private Mesh GenerateSegmentMesh(float startAngle, float endAngle, SegmentData segmentData)
    {
        Mesh mesh = new Mesh();

        int totalVertices = (smoothness + 1) * (segmentData.subdivide + 1); // Total vertices for the segment
        Vector3[] vertices = new Vector3[totalVertices];

        float radiusStep = (segmentData.endRadius - segmentData.startRadius) / segmentData.subdivide; // Distance between rings
        float angleStep = (endAngle - startAngle) / smoothness; // Angle step per segment

        int vertexIndex = 0;

        // Generate vertices for each horizontal division and ring
        for (int division = 0; division <= segmentData.subdivide; division++)
        {
            float currentRadius = segmentData.startRadius + radiusStep * division;
            for (int i = 0; i <= smoothness; i++)
            {
                float currentAngle = Mathf.Deg2Rad * (startAngle + i * angleStep);
                vertices[vertexIndex] = new Vector3(
                    Mathf.Cos(currentAngle) * currentRadius,
                    Mathf.Sin(currentAngle) * currentRadius,
                    0f
                );
                vertexIndex++;
            }
        }

        // Generate triangles
        int[] triangles = new int[smoothness * segmentData.subdivide * 6];
        int triangleIndex = 0;

        for (int division = 0; division < segmentData.subdivide; division++)
        {
            for (int i = 0; i < smoothness; i++)
            {
                int topLeft = division * (smoothness + 1) + i;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + (smoothness + 1);
                int bottomRight = bottomLeft + 1;

                if (flipNormals)
                {
                    // Reverse winding order for inward-facing normals
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomRight;

                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomRight;
                    triangles[triangleIndex++] = topRight;
                }
                else
                {
                    // Default winding order for outward-facing normals
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomRight;

                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomRight;
                    triangles[triangleIndex++] = bottomLeft;
                }
            }
        }

        // Assign vertices and triangles to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Auto-generate normals
        mesh.RecalculateNormals();

        return mesh;
    }

    /// <summary>
    /// Creates a material using the selected shader (Standard or URP Lit).
    /// </summary>
    /// <returns>A new Material object.</returns>
    private Material CreateMaterial()
    {
        Material material;

        if (useURPLitShader)
        {
            // Use URP Lit Shader
            Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLitShader != null)
            {
                material = new Material(urpLitShader);
                material.SetColor("_BaseColor", Color.white); // Set default base color
            }
            else
            {
                Debug.LogWarning("URP Lit Shader not found. Falling back to Standard Shader.");
                material = new Material(Shader.Find("Standard"));
            }
        }
        else
        {
            // Use Standard Shader
            material = new Material(Shader.Find("Standard"));
        }

        return material;
    }

    /// <summary>
    /// Generates a bullseye at the center of the circle, respecting the `flipNormals` variable.
    /// </summary>
    private void GenerateBullseye()
    {
        GameObject bullseye = GameObject.Find("Bullseye");
        if (bullseye == null)
        {
            bullseye = new GameObject("Bullseye");
            bullseye.transform.parent = this.transform;
        }

        MeshFilter meshFilter = bullseye.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = bullseye.AddComponent<MeshFilter>();
        }

        MeshRenderer meshRenderer = bullseye.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = bullseye.AddComponent<MeshRenderer>();
        }

        // Generate the bullseye mesh (a smaller circle)
        Mesh mesh = new Mesh();

        int smoothness = 20; // Number of vertices for the bullseye
        Vector3[] vertices = new Vector3[smoothness + 1];
        int[] triangles = new int[smoothness * 3];

        vertices[0] = Vector3.zero; // Center of the bullseye
        for (int i = 0; i < smoothness; i++)
        {
            float angle = i * Mathf.PI * 2f / smoothness;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * bullseyeRadius, Mathf.Sin(angle) * bullseyeRadius, 0f);
        }

        for (int i = 0; i < smoothness - 1; i++)
        {
            if (!flipNormals)
            {
                // Reverse winding order for inward-facing normals
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = i + 1;
            }
            else
            {
                // Default winding order for outward-facing normals
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        // Close the circle
        if (!flipNormals)
        {
            triangles[(smoothness - 1) * 3] = 0;
            triangles[(smoothness - 1) * 3 + 1] = 1;
            triangles[(smoothness - 1) * 3 + 2] = smoothness;
        }
        else
        {
            triangles[(smoothness - 1) * 3] = 0;
            triangles[(smoothness - 1) * 3 + 1] = smoothness;
            triangles[(smoothness - 1) * 3 + 2] = 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        Material material = CreateMaterial();
        material.color = Color.white; // Bullseye color
        meshRenderer.material = material;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateCircle();
        }
    }
}

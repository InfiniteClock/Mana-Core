using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.LowLevelPhysics;
using static UnityEngine.Rendering.HableCurve;

public class DetectionDisplay : MonoBehaviour
{
    [Range(0f, 180f)]
    public float cutoffAngle;
    public float height;
    public int subdivisions = 24;
    public float baseRadius;

    public Material detectionMaterial;

    private void Start()
    {
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = detectionMaterial;
        mf.mesh = CreateConeMesh();
    }
    private void OnValidate()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = CreateConeMesh();
    }
    private Mesh CreateConeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "RadialCone";

        // gets the radius of the cone from the angle of cutoff and the radius of the sphere
        baseRadius = height * Mathf.Sin((cutoffAngle / 2f) * Mathf.Deg2Rad);
        float edgeHeight = Mathf.Sqrt((height * height) + (baseRadius * baseRadius));
        // Vertices
        Vector3[] vertices = new Vector3[subdivisions + 2];

        // Apex of cone at the origin;
        vertices[0] = Vector3.zero;
        // Outer edge points
        for (int i = 0; i < subdivisions; i++)
        {
            float angle = (Mathf.PI * 2f * i) / subdivisions;
            float x = Mathf.Cos(angle) * edgeHeight;
            float z = Mathf.Sin(angle) * edgeHeight;
            vertices[i + 1] = new Vector3(x, -height, z);
        }
        // Center of the base
        vertices[subdivisions + 1] = new Vector3(0, -height, 0);

        // Triangles
        int[] triangles = new int[subdivisions * 3 + subdivisions * 3];
        int triIndex = 0;

        // Side triangles
        for (int i = 0; i < subdivisions; i++)
        {
            int next = (i + 1) % subdivisions;
            triangles[triIndex++] = 0;              // The apex point
            triangles[triIndex++] = i + 1;          // The current point along edge
            triangles[triIndex++] = next + 1;       // The next point along edge
        }

        // Base triangles
        for (int i = 0; i < subdivisions; i++)
        {
            int next = (i + 1) % subdivisions;
            triangles[triIndex++] = subdivisions + 1; // Base center
            triangles[triIndex++] = i + 1;          // The current point along edge
            triangles[triIndex++] = next + 1;       // The next point along edge
        }

        // UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        // Apex of cone at the origin
        uvs[0] = new Vector2(0.5f, 1f); 
        // Outer edge points
        for(int i = 0; i < subdivisions; i++)
        {
            float angle = (Mathf.PI * 2f * i) / subdivisions;
            uvs[i + 1] = new Vector2((Mathf.Cos(angle) + 1f) / 2f, (Mathf.Sin(angle) + 1f) / 2f);
        }
        // Center of the base 
        uvs[subdivisions + 1] = new Vector2(0.5f, 0.5f);

        // Assign data to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
    private Mesh CreatePartialSphere()
    {
        // Create a new mesh, and lists for its vertices and triangles
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Setup the vertices first

        // Get the cutoffangle in radians
        float cutOffRad = Mathf.Deg2Rad * cutoffAngle;

        // Get latitude points within the resolution
        for (int lat = 0; lat <= subdivisions; lat++)
        {
            float theta = Mathf.PI * lat / subdivisions;

            // Stop at the cutoff point
            if (theta > cutOffRad) break; 

            // Get longitude points within the resolution
            for (int lon = 0; lon <= subdivisions; lon++)
            {
                float phi = Mathf.PI * lon / subdivisions;

                // Gets the x, y, z of the vertice at specified cross angle of theta and phi
                // By setting z to only use Theta, the cutoff is applied to the z axis
                float x = height * Mathf.Sin(theta) * Mathf.Cos(phi);
                float y = height * Mathf.Sin(theta) * Mathf.Sin(phi);
                float z = height * Mathf.Cos(theta);
                
                vertices.Add(new Vector3(x, y, z));
            }
        }

        // Setup the triangles

        int vertsPerRow = subdivisions + 1;
        for (int lat = 0; lat < vertices.Count / vertsPerRow - 1; lat++)
        {
            for (int lon = 0; lon < vertsPerRow - 1; lon++)
            {
                int current = lat * vertsPerRow + lon;
                int next = current + vertsPerRow;

                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + 1);

                triangles.Add(current + 1);
                triangles.Add(next);
                triangles.Add(next + 1);
            }
        }

        // Set up the mesh with the vertices and triangles, recalculate the normals, then return the new mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

}

using System.Collections.Generic;
using UnityEngine;

public class GizmosDrawer : MonoBehaviour
{
    [SerializeField] private CharacterController m_Controller;
    [SerializeField] private Color m_Color;
    [SerializeField] private int m_SegmentCount = 12;
    [SerializeField] private int m_RingCount = 6;

    private static float m_CapsuleRadius;
    private static float m_CapsuleHeight;
    private static Mesh m_CapsuleMesh;

    private void OnDrawGizmos()
    {
        if (m_Controller == null)
            return;

        float radius = m_Controller.radius;
        float height = m_Controller.height;
        Vector3 center = m_Controller.center;

        if (m_CapsuleMesh == null || HasCapsulePropertyChanged(radius, height))
            m_CapsuleMesh = CreateCapsule(radius, height, m_SegmentCount, m_RingCount);

        Gizmos.color = m_Color;
        Gizmos.DrawMesh(m_CapsuleMesh, m_Controller.transform.position + center);
    }

    [ContextMenu("Update Capsule Gizmos Mesh")]
    public void UpdateCapsuleMesh()
    {
        if (m_Controller == null)
            return;

        m_CapsuleMesh = CreateCapsule(m_Controller.radius, m_Controller.height, m_SegmentCount, m_RingCount);
    }

    private static bool HasCapsulePropertyChanged(float radius, float height)
    {
        bool hasChanged = !Mathf.Approximately(radius, m_CapsuleRadius) || !Mathf.Approximately(height, m_CapsuleHeight);
        m_CapsuleRadius = radius;
        m_CapsuleHeight = height;
        return hasChanged;
    }

    private static Mesh CreateCapsule(float radius, float height, int segmentCount = 12, int ringCount = 6)
    {
        segmentCount = Mathf.Max(2, segmentCount);
        ringCount = Mathf.Max(2, ringCount);

        Mesh mesh = new Mesh();
        mesh.name = "Capsule";

        List<Vector3> positions = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        float deltaRingAngle = 90.0f / ringCount;
        float deltaSegmentAngle = 360.0f / segmentCount;

        ///////////////////////////////////////////////
        // TOP SPHERE /////////////////////////////////
        ///////////////////////////////////////////////

        Vector3 topSphereCenter = (height * 0.5f - radius) * Vector3.up;

        // Calculate vertex positions and normals
        for (int ringIndex = 0; ringIndex < ringCount; ringIndex++)
        {
            float ringAngle = ringIndex * deltaRingAngle * Mathf.Deg2Rad;

            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                float segmentAngle = segmentIndex * deltaSegmentAngle * Mathf.Deg2Rad;
                Vector3 position = new Vector3(Mathf.Sin(segmentAngle) * Mathf.Cos(ringAngle), Mathf.Sin(ringAngle), Mathf.Cos(segmentAngle) * Mathf.Cos(ringAngle));
                Vector3 normal = position.normalized;

                positions.Add(topSphereCenter + position * radius);
                normals.Add(normal);
            }
        }

        positions.Add(topSphereCenter + Vector3.up * radius);
        normals.Add(Vector3.up);

        // Triangulate vertices
        for (int ringIndex = 0; ringIndex < ringCount - 1; ringIndex++)
        {
            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                int current = segmentCount * ringIndex + segmentIndex;
                int top = segmentCount * (ringIndex + 1) + segmentIndex;
                int left = (current + 1) % segmentCount + (segmentCount * ringIndex);
                int topLeft = (top + 1) % segmentCount + (segmentCount * (ringIndex + 1));

                triangles.Add(current);
                triangles.Add(left);
                triangles.Add(topLeft);

                triangles.Add(topLeft);
                triangles.Add(top);
                triangles.Add(current);
            }
        }

        for (int ringIndex = ringCount - 1; ringIndex < ringCount; ringIndex++)
        {
            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                int current = segmentCount * ringIndex + segmentIndex;
                int top = positions.Count - 1;
                int left = (current + 1) % segmentCount + (segmentCount * ringIndex);

                triangles.Add(current);
                triangles.Add(left);
                triangles.Add(top);
            }
        }

        ////////////////////////////////////////////////
        // BOTTOM SPHERE ///////////////////////////////
        ////////////////////////////////////////////////

        Vector3 bottomSphereCenter = (height * 0.5f - radius) * Vector3.down;

        // Calculate vertex positions and normals
        for (int ringIndex = 0; ringIndex < ringCount; ringIndex++)
        {
            float ringAngle = ringIndex * deltaRingAngle * Mathf.Deg2Rad;

            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                float segmentAngle = segmentIndex * deltaSegmentAngle * Mathf.Deg2Rad;
                Vector3 position = new Vector3(Mathf.Sin(segmentAngle) * Mathf.Cos(ringAngle), -Mathf.Sin(ringAngle), Mathf.Cos(segmentAngle) * Mathf.Cos(ringAngle));
                Vector3 normal = position.normalized;

                positions.Add(bottomSphereCenter + position * radius);
                normals.Add(normal);
            }
        }

        positions.Add(bottomSphereCenter + Vector3.down * radius);
        normals.Add(Vector3.down);

        // Triangulate vertices
        for (int ringIndex = 0; ringIndex < ringCount - 1; ringIndex++)
        {
            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                int offset = ringCount * segmentCount + 1;
                int current = segmentCount * ringIndex + segmentIndex;
                int bottom = segmentCount * (ringIndex + 1) + segmentIndex;
                int left = (current + 1) % segmentCount + (segmentCount * ringIndex);
                int bottomLeft = (bottom + 1) % segmentCount + (segmentCount * (ringIndex + 1));

                triangles.Add(left + offset);
                triangles.Add(current + offset);
                triangles.Add(bottom + offset);

                triangles.Add(bottom + offset);
                triangles.Add(bottomLeft + offset);
                triangles.Add(left + offset);
            }
        }

        for (int ringIndex = ringCount - 1; ringIndex < ringCount; ringIndex++)
        {
            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                int offset = ringCount * segmentCount + 1;
                int current = segmentCount * ringIndex + segmentIndex;
                int bottom = positions.Count - 1;
                int left = (current + 1) % segmentCount + (segmentCount * ringIndex);

                triangles.Add(left + offset);
                triangles.Add(current + offset);
                triangles.Add(bottom);
            }
        }

        ////////////////////////////////////////////////
        // CYLINDER ////////////////////////////////////
        ////////////////////////////////////////////////

        // Triangulate vertices
        for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
        {
            int offset = ringCount * segmentCount + 1;
            int current = segmentIndex;
            int left = (current + 1) % segmentCount;
            int bottom = offset + segmentIndex;
            int bottomLeft = left + offset;

            triangles.Add(left);
            triangles.Add(current);
            triangles.Add(bottom);

            triangles.Add(bottom);
            triangles.Add(bottomLeft);
            triangles.Add(left);
        }


        mesh.vertices = positions.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();

        return mesh;
    }

}

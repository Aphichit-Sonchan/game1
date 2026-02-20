using UnityEngine;
using System.Collections;

/// <summary>
/// แสดงโซนอันตรายด้วยเอฟเฟกต์กระพริบ (ไม่มี Collider)
/// </summary>
public class DangerZoneVisualizer : MonoBehaviour
{
    [Header("Visualization")]
    public Color dangerColor = Color.red;
    public Color normalColor = Color.white;
    public float blinkSpeed = 2f;

    [Header("Danger Zone Display")]
    [Tooltip("แสดงโซนอันตรายตลอดเวลา")]
    public bool alwaysShow = true;

    [Tooltip("รัศมีด้านใน")]
    public float innerRadius = 2f;

    [Tooltip("รัศมีด้านนอก")]
    public float outerRadius = 6f;

    [Tooltip("ความสูงจากพื้น")]
    public float heightOffset = 0.05f;

    [Tooltip("ความกว้างของโซนอันตราย (องศา)")]
    public float dangerZoneWidth = 45f;

    private bool isBlinking = false;
    private GameObject[] dangerZones;
    private Material dangerMaterial;

    void Start()
    {
        if (alwaysShow)
        {
            CreateDangerZones();
        }
    }

    /// <summary>
    /// สร้างโซนอันตราย 4 โซน (ไม่มี Collider!)
    /// </summary>
    void CreateDangerZones()
    {
        dangerZones = new GameObject[4];

        // สร้าง Material แดงโปร่งใส
        dangerMaterial = new Material(Shader.Find("Standard"));
        dangerMaterial.color = dangerColor;
        dangerMaterial.SetFloat("_Mode", 3); // Transparent mode
        dangerMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        dangerMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        dangerMaterial.SetInt("_ZWrite", 0);
        dangerMaterial.DisableKeyword("_ALPHATEST_ON");
        dangerMaterial.EnableKeyword("_ALPHABLEND_ON");
        dangerMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        dangerMaterial.renderQueue = 3000;

        // สร้างโซนที่มุม 0°, 90°, 180°, 270°
        float[] angles = { 0f, 90f, 180f, 270f };

        for (int i = 0; i < 4; i++)
        {
            dangerZones[i] = CreateDangerZone(angles[i], $"DangerZone_{angles[i]}");
            dangerZones[i].transform.SetParent(transform);
            dangerZones[i].SetActive(alwaysShow);
        }

        Debug.Log("[DangerZoneVisualizer] Created 4 danger zones (NO COLLIDERS)");
    }

    /// <summary>
    /// สร้างโซนอันตราย 1 โซน (ไม่มี Collider!)
    /// </summary>
    GameObject CreateDangerZone(float centerAngle, string name)
    {
        GameObject zone = new GameObject(name);

        MeshFilter meshFilter = zone.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = zone.AddComponent<MeshRenderer>();

        meshFilter.mesh = CreateDangerZoneMesh(centerAngle);
        meshRenderer.material = dangerMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        // ⭐ ไม่มี Collider เลย - แค่เป็น Visual เท่านั้น!

        // ตำแหน่งสูงจากพื้นนิดหน่อย
        zone.transform.localPosition = new Vector3(0, heightOffset, 0);
        zone.transform.localRotation = Quaternion.identity;

        return zone;
    }

    /// <summary>
    /// สร้าง Mesh รูปพัดวงกลม สำหรับโซนอันตราย
    /// </summary>
    Mesh CreateDangerZoneMesh(float centerAngle)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"DangerZone_{centerAngle}";

        // คำนวณมุมเริ่มต้นและสิ้นสุด
        float halfWidth = dangerZoneWidth / 2f;
        float startAngle = centerAngle - halfWidth;
        float endAngle = centerAngle + halfWidth;

        // จำนวน segments
        int segments = 20;
        int vertexCount = (segments + 1) * 2;

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        int[] triangles = new int[segments * 6];

        // สร้าง vertices
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;

            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // Vertex ด้านใน
            vertices[i * 2] = new Vector3(cos * innerRadius, 0, sin * innerRadius);
            uv[i * 2] = new Vector2(t, 0);

            // Vertex ด้านนอก
            vertices[i * 2 + 1] = new Vector3(cos * outerRadius, 0, sin * outerRadius);
            uv[i * 2 + 1] = new Vector2(t, 1);
        }

        // สร้าง triangles
        for (int i = 0; i < segments; i++)
        {
            int baseIndex = i * 6;
            int vertIndex = i * 2;

            // Triangle 1
            triangles[baseIndex] = vertIndex;
            triangles[baseIndex + 1] = vertIndex + 2;
            triangles[baseIndex + 2] = vertIndex + 1;

            // Triangle 2
            triangles[baseIndex + 3] = vertIndex + 1;
            triangles[baseIndex + 4] = vertIndex + 2;
            triangles[baseIndex + 5] = vertIndex + 3;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// เริ่มกระพริบโซนอันตราย
    /// </summary>
    public void StartBlinking()
    {
        isBlinking = true;
        ShowDangerZones();
        Debug.Log("[DangerZoneVisualizer] Started blinking");
    }

    /// <summary>
    /// หยุดกระพริบ
    /// </summary>
    public void StopBlinking()
    {
        isBlinking = false;

        // คืนสีเดิม
        if (dangerMaterial != null)
        {
            dangerMaterial.color = dangerColor;
        }

        Debug.Log("[DangerZoneVisualizer] Stopped blinking");
    }

    /// <summary>
    /// แสดงโซนอันตราย
    /// </summary>
    public void ShowDangerZones()
    {
        if (dangerZones == null) return;

        foreach (var zone in dangerZones)
        {
            if (zone != null)
                zone.SetActive(true);
        }
    }

    /// <summary>
    /// ซ่อนโซนอันตราย
    /// </summary>
    public void HideDangerZones()
    {
        if (dangerZones == null) return;

        foreach (var zone in dangerZones)
        {
            if (zone != null)
                zone.SetActive(false);
        }
    }

    void Update()
    {
        if (isBlinking && dangerMaterial != null)
        {
            // กระพริบสีแดง
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 0.5f) + 0.2f;
            Color color = dangerColor;
            color.a = alpha;
            dangerMaterial.color = color;
        }
    }

    void OnDestroy()
    {
        // ทำลาย Material
        if (dangerMaterial != null)
        {
            Destroy(dangerMaterial);
        }

        // ทำลาย Danger Zones
        if (dangerZones != null)
        {
            foreach (var zone in dangerZones)
            {
                if (zone != null)
                    Destroy(zone);
            }
        }
    }

    void OnDrawGizmos()
    {
        // แสดง Gizmo ใน Scene View
        if (isBlinking)
        {
            Gizmos.color = dangerColor;
        }
        else
        {
            Gizmos.color = normalColor;
        }
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
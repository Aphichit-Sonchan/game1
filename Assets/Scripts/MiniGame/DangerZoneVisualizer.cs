using UnityEngine;

/// <summary>
/// แสดงโซนอันตรายด้วยเอฟเฟกต์กระพริบ
/// </summary>
public class DangerZoneVisualizer : MonoBehaviour
{
    [Header("Visualization")]
    public Color dangerColor = Color.red;
    public Color normalColor = Color.white;
    public float blinkSpeed = 2f;

    private SpinWheelSectionManager sectionManager;
    private bool isBlinking = false;
    private Renderer[] targetRenderers;

    void Start()
    {
        sectionManager = GetComponent<SpinWheelSectionManager>();
    }

    /// <summary>
    /// เริ่มกระพริบโซนอันตราย
    /// </summary>
    public void StartBlinking()
    {
        isBlinking = true;
        Debug.Log("[DangerZoneVisualizer] Started blinking");
    }

    /// <summary>
    /// หยุดกระพริบ
    /// </summary>
    public void StopBlinking()
    {
        isBlinking = false;
        Debug.Log("[DangerZoneVisualizer] Stopped blinking");
    }

    void Update()
    {
        if (isBlinking)
        {
            // กระพริบโซนอันตราย (ถ้ามี)
            // ใช้ PingPong เพื่อให้กระพริบไปมา
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);

            // อัพเดทสีของโซนอันตราย (ถ้ามี SectionManager)
            // โค้ดนี้เป็นตัวอย่าง - ปรับตามโครงสร้างจริง
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
using UnityEngine;

/// <summary>
/// จัดการส่วนของ Spin Wheel - สุ่ม 1 ส่วนเป็นสีแดง
/// </summary>
public class SpinWheelSectionManager : MonoBehaviour
{
    [Header("🎨 Section Colors")]
    public Color normalColor = Color.white;
    public Color dangerColor = Color.red;

    [Header("📦 Section Objects")]
    [Tooltip("4 ส่วนของวงล้อ ตามลำดับ: [0]=0°-90°, [1]=90°-180°, [2]=180°-270°, [3]=270°-360°")]
    public GameObject[] sections = new GameObject[4];

    private Renderer[] sectionRenderers = new Renderer[4];
    private int dangerSectionIndex = -1;

    void Start()
    {
        // เก็บ Renderer ของแต่ละส่วน
        for (int i = 0; i < 4; i++)
        {
            if (sections[i] != null)
            {
                sectionRenderers[i] = sections[i].GetComponent<Renderer>();

                // ตรวจสอบว่ามี Renderer หรือไม่
                if (sectionRenderers[i] == null)
                {
                    Debug.LogError($"[SectionManager] Section {i} doesn't have a Renderer!");
                }
            }
            else
            {
                Debug.LogWarning($"[SectionManager] Section {i} not assigned!");
            }
        }
    }

    /// <summary>
    /// สุ่มเลือก 1 ส่วนให้เป็นสีแดง
    /// </summary>
    public void SelectRandomDangerSection()
    {
        // รีเซ็ตทุกส่วนเป็นสีปกติก่อน
        ResetAllSections();

        // สุ่มเลือก 1 ส่วน (0-3)
        dangerSectionIndex = Random.Range(0, 4);

        // เปลี่ยนส่วนนั้นเป็นสีแดง
        if (sectionRenderers[dangerSectionIndex] != null)
        {
            sectionRenderers[dangerSectionIndex].material.color = dangerColor;
            Debug.Log($"[SectionManager] Danger zone: Section {dangerSectionIndex} ({dangerSectionIndex * 90}° - {(dangerSectionIndex + 1) * 90}°)");
        }
    }

    /// <summary>
    /// รีเซ็ตทุกส่วนเป็นสีปกติ
    /// </summary>
    public void ResetAllSections()
    {
        for (int i = 0; i < 4; i++)
        {
            if (sectionRenderers[i] != null)
            {
                sectionRenderers[i].material.color = normalColor;
            }
        }
        dangerSectionIndex = -1;
    }

    /// <summary>
    /// ตรวจสอบว่าผู้เล่นอยู่ในโซนอันตรายหรือไม่
    /// </summary>
    public bool IsPlayerInDangerZone(PlayerController player, float platformRotation)
    {
        if (dangerSectionIndex == -1)
        {
            Debug.LogWarning("[SectionManager] No danger section selected!");
            return false;
        }

        if (!player.isAlive) return false;

        // ดึงมุมของผู้เล่น
        float playerAngle = player.GetCurrentAngle();

        // คำนวณมุมรวมหลังจากแพลตฟอร์มหมุน
        float totalAngle = (playerAngle + platformRotation) % 360f;
        if (totalAngle < 0) totalAngle += 360f;

        // แปลงมุมเป็นส่วน (0-3)
        // Section 0 = 0°-90°
        // Section 1 = 90°-180°
        // Section 2 = 180°-270°
        // Section 3 = 270°-360°
        int playerSection = Mathf.FloorToInt(totalAngle / 90f);
        if (playerSection >= 4) playerSection = 3; // ป้องกัน edge case

        bool inDanger = (playerSection == dangerSectionIndex);

        Debug.Log($"[SectionManager] {player.playerName}: " +
                  $"angle={totalAngle:F1}°, section={playerSection}, " +
                  $"danger={dangerSectionIndex}, inDanger={inDanger}");

        return inDanger;
    }
}
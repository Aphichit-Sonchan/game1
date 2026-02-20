using UnityEngine;

public class SpinWheelSectionManager : MonoBehaviour
{
    [Header("🎨 Materials")]
    public Material normalMaterial;
    public Material dangerMaterial;

    [Header("📦 Section Objects")]
    [Tooltip("4 ส่วนของวงล้อ ตามลำดับ: [0]=0°-90°, [1]=90°-180°, [2]=180°-270°, [3]=270°-360°")]
    public GameObject[] sections = new GameObject[4];

    private Renderer[] sectionRenderers = new Renderer[4];
    private int dangerSectionIndex = -1;

    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            if (sections[i] != null)
            {
                sectionRenderers[i] = sections[i].GetComponent<Renderer>();

                if (sectionRenderers[i] == null)
                {
                    Debug.LogError($"[SectionManager] Section {i} ({sections[i].name}) doesn't have a Renderer!");
                }
            }
            else
            {
                Debug.LogWarning($"[SectionManager] Section {i} not assigned!");
            }
        }

        if (normalMaterial == null)
            Debug.LogError("[SectionManager] Normal Material not assigned!");

        if (dangerMaterial == null)
            Debug.LogError("[SectionManager] Danger Material not assigned!");

        ResetAllSections();
    }

    public void SelectRandomDangerSection()
    {
        ResetAllSections();
        dangerSectionIndex = Random.Range(0, 4);

        if (sectionRenderers[dangerSectionIndex] != null && dangerMaterial != null)
        {
            sectionRenderers[dangerSectionIndex].material = dangerMaterial;

            Debug.Log($"[SectionManager] ========== DANGER ZONE SELECTED ==========");
            Debug.Log($"[SectionManager] Section Index: {dangerSectionIndex}");
            Debug.Log($"[SectionManager] Section Name: {sections[dangerSectionIndex].name}");
            Debug.Log($"[SectionManager] Angle Range: {dangerSectionIndex * 90}° - {(dangerSectionIndex + 1) * 90}°");
            Debug.Log($"[SectionManager] ============================================");
        }
    }

    public void ResetAllSections()
    {
        if (normalMaterial == null) return;

        for (int i = 0; i < 4; i++)
        {
            if (sectionRenderers[i] != null)
            {
                sectionRenderers[i].material = normalMaterial;
            }
        }
        dangerSectionIndex = -1;
    }

    public bool IsPlayerInDangerZone(PlayerController player, float platformRotation)
    {
        if (dangerSectionIndex == -1)
        {
            Debug.LogWarning("[SectionManager] No danger section selected!");
            return false;
        }

        if (!player.isAlive)
            return false;

        // ดึงมุมของผู้เล่นเทียบกับ RotatingBase (local angle)
        float playerLocalAngle = player.GetCurrentAngle();

        // คำนวณมุมของผู้เล่นในโลก (world angle)
        // เนื่องจากผู้เล่นหมุนตาม RotatingBase
        // มุมจริง = มุม local + มุมที่ RotatingBase หมุนไป
        float playerWorldAngle = playerLocalAngle + platformRotation;

        // Normalize เป็น 0-360
        playerWorldAngle = ((playerWorldAngle % 360f) + 360f) % 360f;

        // แบ่งเป็น 4 ส่วน
        // Section 0 = 0° - 90°
        // Section 1 = 90° - 180°
        // Section 2 = 180° - 270°
        // Section 3 = 270° - 360°
        int playerSection = Mathf.FloorToInt(playerWorldAngle / 90f);

        // Handle edge case
        if (playerSection >= 4) playerSection = 0;

        // ตรวจสอบว่าอยู่ในโซนอันตรายหรือไม่
        bool inDanger = (playerSection == dangerSectionIndex);

        Debug.Log($"[SectionManager] Player: {player.playerName}");
        Debug.Log($"  Local Angle: {playerLocalAngle:F1}°");
        Debug.Log($"  Platform Rotation: {platformRotation:F1}°");
        Debug.Log($"  World Angle: {playerWorldAngle:F1}°");
        Debug.Log($"  Player Section: {playerSection}");
        Debug.Log($"  Danger Section: {dangerSectionIndex}");
        Debug.Log($"  In Danger? {(inDanger ? "❌ YES" : "✅ NO")}");

        return inDanger;
    }
}
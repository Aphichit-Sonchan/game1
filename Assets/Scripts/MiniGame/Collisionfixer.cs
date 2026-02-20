using UnityEngine;

/// <summary>
/// แก้ปัญหา Collider ให้อัตโนมัติ - ลบ Collider จาก Visual Objects
/// วิธีใช้: แนบไว้ที่ Platform GameObject
/// </summary>
public class CollisionFixer : MonoBehaviour
{
    [Header("Auto Fix Settings")]
    [Tooltip("ลบ Collider จาก Overlay อัตโนมัติตอน Start")]
    public bool autoFixOnStart = true;

    [Tooltip("ชื่อที่ต้องการหา (ใช้ Contains)")]
    public string[] visualObjectNames = new string[]
    {
        "DangerOverlay",
        "DangerZone_",
        "Overlay",
        "Visual"
    };

    void Start()
    {
        if (autoFixOnStart)
        {
            FixAllCollisions();
        }
    }

    /// <summary>
    /// แก้ปัญหา Collision ทั้งหมดในฉาก
    /// </summary>
    [ContextMenu("Fix All Collisions Now")]
    public void FixAllCollisions()
    {
        int fixedCount = 0;

        // หาทุก GameObject ในฉาก
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // ตรวจสอบว่าชื่อตรงกับที่ต้องการหรือไม่
            bool isVisualObject = false;

            foreach (string name in visualObjectNames)
            {
                if (obj.name.Contains(name))
                {
                    isVisualObject = true;
                    break;
                }
            }

            if (isVisualObject)
            {
                // ลบ Collider ทั้งหมดออก
                Collider[] colliders = obj.GetComponents<Collider>();

                foreach (Collider col in colliders)
                {
                    Debug.Log($"[CollisionFixer] Removing {col.GetType().Name} from {obj.name}");

                    if (Application.isPlaying)
                    {
                        Destroy(col);
                    }
                    else
                    {
                        DestroyImmediate(col);
                    }

                    fixedCount++;
                }
            }
        }

        Debug.Log($"[CollisionFixer] ✅ Fixed {fixedCount} colliders!");
    }

    /// <summary>
    /// ตั้งค่า Layer ให้ Visual Objects
    /// </summary>
    [ContextMenu("Set Visual Objects to VisualOnly Layer")]
    public void SetVisualLayer()
    {
        // ลองหา Layer "VisualOnly"
        int visualLayer = LayerMask.NameToLayer("VisualOnly");

        if (visualLayer == -1)
        {
            Debug.LogWarning("[CollisionFixer] ⚠️ Layer 'VisualOnly' not found! Please create it first.");
            Debug.LogWarning("Go to: Edit → Project Settings → Tags and Layers");
            return;
        }

        int changedCount = 0;
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            bool isVisualObject = false;

            foreach (string name in visualObjectNames)
            {
                if (obj.name.Contains(name))
                {
                    isVisualObject = true;
                    break;
                }
            }

            if (isVisualObject)
            {
                obj.layer = visualLayer;
                Debug.Log($"[CollisionFixer] Set {obj.name} to VisualOnly layer");
                changedCount++;
            }
        }

        Debug.Log($"[CollisionFixer] ✅ Changed {changedCount} objects to VisualOnly layer!");
    }
}
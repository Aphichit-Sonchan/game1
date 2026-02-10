using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script สำหรับจัดวางผู้เล่นอัตโนมัติในรูปแบบวงกลม
/// ใช้ใน Unity Editor เท่านั้น
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject playerPrefab;
    public Transform platformTransform;
    public float spawnRadius = 3.5f;
    public float spawnHeight = 0.5f;
    
    [Header("Player Settings")]
    public string[] playerNames = { "แดง", "น้ำเงิน", "เขียว", "เหลือง", "ม่วง", "ส้ม" };
    public Color[] playerColors = {
        new Color(0.906f, 0.298f, 0.235f), // แดง #e74c3c
        new Color(0.204f, 0.596f, 0.859f), // น้ำเงิน #3498db
        new Color(0.180f, 0.800f, 0.443f), // เขียว #2ecc71
        new Color(0.945f, 0.769f, 0.059f), // เหลือง #f1c40f
        new Color(0.608f, 0.349f, 0.714f), // ม่วง #9b59b6
        new Color(0.902f, 0.494f, 0.133f)  // ส้ม #e67e22
    };

#if UNITY_EDITOR
    [ContextMenu("Spawn All Players")]
    public void SpawnAllPlayers()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab is not assigned!");
            return;
        }

        if (platformTransform == null)
        {
            platformTransform = transform;
        }

        // ลบผู้เล่นเก่าออก (ถ้ามี)
        ClearExistingPlayers();

        // สร้างผู้เล่นใหม่
        for (int i = 0; i < 6; i++)
        {
            SpawnPlayer(i);
        }

        Debug.Log("Spawned 6 players successfully!");
    }

    [ContextMenu("Clear All Players")]
    public void ClearExistingPlayers()
    {
        // หาผู้เล่นทั้งหมดที่เป็น child ของ platform
        PlayerController[] existingPlayers = platformTransform.GetComponentsInChildren<PlayerController>();
        
        foreach (var player in existingPlayers)
        {
            DestroyImmediate(player.gameObject);
        }

        Debug.Log("Cleared all existing players.");
    }

    private void SpawnPlayer(int index)
    {
        // คำนวณมุมสำหรับผู้เล่นแต่ละคน (แบ่งเป็น 6 ส่วนเท่าๆ กัน)
        float angle = index * 60f;
        float radians = angle * Mathf.Deg2Rad;

        // คำนวณตำแหน่ง
        Vector3 localPosition = new Vector3(
            Mathf.Cos(radians) * spawnRadius,
            spawnHeight,
            Mathf.Sin(radians) * spawnRadius
        );

        // สร้าง player
        GameObject playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, platformTransform);
        playerObj.transform.localPosition = localPosition;
        playerObj.name = $"Player_{index + 1}_{playerNames[index]}";

        // ตั้งค่า PlayerController
        PlayerController controller = playerObj.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.playerName = $"ผู้เล่น {playerNames[index]}";
            controller.playerId = index;
            controller.playerColor = playerColors[index];
            controller.distanceFromCenter = spawnRadius;

            // ตั้งค่าตำแหน่งเริ่มต้น
            controller.SetInitialPosition(angle, spawnRadius, platformTransform.position);

            // ตั้งค่าสี Material (ถ้ามี Renderer)
            Renderer renderer = playerObj.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = playerColors[index];
            }
        }

        Debug.Log($"Spawned {playerNames[index]} at angle {angle}° (Position: {localPosition})");
    }

    // วาดเส้นช่วยใน Scene View
    private void OnDrawGizmos()
    {
        if (platformTransform == null) return;

        Gizmos.color = Color.yellow;
        
        // วาดวงกลมแสดงตำแหน่งที่จะ spawn
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f;
            float radians = angle * Mathf.Deg2Rad;

            Vector3 position = platformTransform.position + new Vector3(
                Mathf.Cos(radians) * spawnRadius,
                spawnHeight,
                Mathf.Sin(radians) * spawnRadius
            );

            Gizmos.DrawWireSphere(position, 0.3f);
            
            // วาดเส้นจากศูนย์กลางไปยังตำแหน่ง spawn
            Gizmos.DrawLine(
                platformTransform.position + Vector3.up * spawnHeight, 
                position
            );
        }

        // วาดวงกลม spawn radius
        Gizmos.color = Color.cyan;
        DrawCircle(platformTransform.position + Vector3.up * spawnHeight, spawnRadius, 36);
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0) * radius, 0, Mathf.Sin(0) * radius);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif
}

#if UNITY_EDITOR
/// <summary>
/// Custom Editor สำหรับ PlayerSpawner เพื่อให้ใช้งานง่ายขึ้น
/// </summary>
[CustomEditor(typeof(PlayerSpawner))]
public class PlayerSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerSpawner spawner = (PlayerSpawner)target;

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("🎮 Spawn All Players", GUILayout.Height(40)))
        {
            spawner.SpawnAllPlayers();
        }

        if (GUILayout.Button("🗑️ Clear All Players", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(
                "Clear Players",
                "Are you sure you want to delete all existing players?",
                "Yes, Clear",
                "Cancel"))
            {
                spawner.ClearExistingPlayers();
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "1. Assign Player Prefab และ Platform Transform\n" +
            "2. กดปุ่ม 'Spawn All Players' เพื่อสร้างผู้เล่นทั้ง 6 คน\n" +
            "3. ผู้เล่นจะถูกจัดวางเป็นวงกลมรอบแพลตฟอร์มโดยอัตโนมัติ",
            MessageType.Info
        );
    }
}
#endif

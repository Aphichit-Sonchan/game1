using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Info")]
    public string playerName = "Player";
    public Color playerColor = Color.white;
    public int playerId = 0;

    [Header("Control Settings")]
    public bool isLocalPlayer = false;

    [Header("Platform Reference")]
    public GameObject platform;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float distanceFromCenter = 3.5f;

    [Header("Input Settings")]
    public bool useKeyboardMovement = true;
    public float walkSpeed = 3f;
    public float maxDistanceFromCenter = 5f;
    public float minDistanceFromCenter = 2f;

    [Header("Visual Settings")]
    public GameObject playerModel;
    public Renderer playerRenderer;
    public GameObject selectionRing;

    [Header("Fall Settings")]
    public float fallDuration = 0.8f;
    public float fallDistance = 5f;

    public bool isAlive { get; private set; } = true;

    private float currentAngle = 0f;
    private bool canMove = false;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Setup Rigidbody อัตโนมัติถ้ายังไม่ได้ใส่
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // ห้ามตัวล้ม

        originalPosition = transform.position;

        if (playerRenderer != null)
        {
            playerRenderer.material.color = playerColor;
        }

        if (selectionRing != null)
        {
            selectionRing.SetActive(false);
        }

        Vector3 center = GetPlatformCenter();
        Vector3 offset = transform.position - center;
        currentAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
    }

    void Update()
    {
        // ถ้าถูกสั่งห้ามเดิน หรือตาย ห้ามรับ Input
        if (!canMove || !isAlive) return;

        // WASD Movement
        if (useKeyboardMovement && isLocalPlayer && !isMoving)
        {
            HandleKeyboardMovement();
        }

        // Smooth movement (ถ้าใช้ระบบเดินแบบคลิก หรือ Lerp)
        if (isMoving)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    void HandleKeyboardMovement()
    {
        Vector3 center = GetPlatformCenter();
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;

        if (horizontal != 0f || vertical != 0f)
        {
            Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;
            Vector3 newPosition = transform.position + moveDirection * walkSpeed * Time.deltaTime;

            // Limit distance logic
            Vector3 offset = newPosition - center;
            offset.y = 0;
            float distance = offset.magnitude;

            if (distance > maxDistanceFromCenter)
            {
                offset = offset.normalized * maxDistanceFromCenter;
            }
            else if (distance < minDistanceFromCenter)
            {
                offset = offset.normalized * minDistanceFromCenter;
            }

            newPosition = center + offset;
            newPosition.y = transform.position.y;

            transform.position = newPosition; // Move directly

            // อัปเดตมุมปัจจุบัน
            Vector3 finalOffset = transform.position - center;
            currentAngle = Mathf.Atan2(finalOffset.z, finalOffset.x) * Mathf.Rad2Deg;
            distanceFromCenter = finalOffset.magnitude;
        }
    }

    // ✅ ฟังก์ชันสั่งเปิด/ปิดการขยับ และ Physics (สำคัญมากสำหรับกันตก)
    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;

        if (selectionRing != null)
        {
            selectionRing.SetActive(canMove && isLocalPlayer);
        }

        if (rb != null)
        {
            if (!canMove)
            {
                // 🧊 Freeze: หยุดทุกอย่าง ลอยค้างกลางอากาศ (ป้องกันร่วงตอนพื้นหาย)
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // ▶️ Unfreeze: กลับมาใช้ Physics ปกติ
                rb.isKinematic = false;
                rb.WakeUp();
            }
        }
    }

    public void Eliminate()
    {
        if (!isAlive) return;

        isAlive = false;
        canMove = false;

        if (selectionRing != null)
        {
            selectionRing.SetActive(false);
        }

        // ปลด Physics เพื่อให้ Animation ควบคุมการตกแทน
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        StartCoroutine(FallAnimation());
    }

    IEnumerator FallAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - new Vector3(0, fallDistance, 0);
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 0.1f;

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;

            // ตกแบบ Smooth
            transform.position = Vector3.Lerp(startPos, endPos, t * t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            if (playerRenderer != null)
            {
                Color color = playerRenderer.material.color;
                color.a = 1f - t;
                playerRenderer.material.color = color;
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }

    // ✅ ฟังก์ชันที่หายไป (เอากลับมาแล้ว)
    public void SetInitialPosition(float angle, float radius, Vector3 platformCenter)
    {
        currentAngle = angle;
        distanceFromCenter = radius;

        float radians = angle * Mathf.Deg2Rad;
        transform.position = platformCenter + new Vector3(
            Mathf.Cos(radians) * radius,
            transform.position.y, // รักษาระดับความสูงเดิม
            Mathf.Sin(radians) * radius
        );

        originalPosition = transform.position;
    }

    Vector3 GetPlatformCenter()
    {
        if (transform.parent != null)
        {
            return transform.parent.position;
        }
        else if (platform != null)
        {
            return platform.transform.position;
        }
        else
        {
            GameObject foundPlatform = GameObject.Find("Platform");
            if (foundPlatform != null)
            {
                platform = foundPlatform;
                return foundPlatform.transform.position;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    // ฟังก์ชันย้ายตำแหน่งสุ่ม (ถ้าจำเป็นต้องใช้)
    public void MoveToNewPosition()
    {
        float angleChange = Random.Range(-45f, 45f);
        currentAngle += angleChange;
        currentAngle = (currentAngle % 360f + 360f) % 360f;

        Vector3 center = GetPlatformCenter();
        float radians = currentAngle * Mathf.Deg2Rad;

        targetPosition = center + new Vector3(
            Mathf.Cos(radians) * distanceFromCenter,
            transform.position.y,
            Mathf.Sin(radians) * distanceFromCenter
        );

        isMoving = true;
    }
}
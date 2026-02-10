using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Info")]
    public string playerName = "Player";
    public Color playerColor = Color.white;
    public int playerId = 0;

    [Header("Control Settings")]
    [Tooltip("Player นี้เป็นผู้เล่นหลัก (ควบคุมด้วย WASD)")]
    public bool isLocalPlayer = false;

    [Header("Platform Reference")]
    [Tooltip("ลาก Platform GameObject มาใส่")]
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

    void Start()
    {
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

        // แสดงข้อความว่า Player นี้ควบคุมได้หรือไม่
        if (isLocalPlayer)
        {
            Debug.Log($"[{playerName}] ✅ Local Player - Use WASD to move!");
        }
    }

    void Update()
    {
        // WASD Movement - ใช้ได้เฉพาะ Local Player
        if (useKeyboardMovement && isAlive && !isMoving && isLocalPlayer)
        {
            HandleKeyboardMovement();
        }

        // Smooth movement
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

            transform.position = newPosition;

            Vector3 finalOffset = transform.position - center;
            currentAngle = Mathf.Atan2(finalOffset.z, finalOffset.x) * Mathf.Rad2Deg;
            distanceFromCenter = finalOffset.magnitude;
        }
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

    void OnMouseEnter()
    {
        if (canMove && isAlive && selectionRing != null)
        {
            selectionRing.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        if (selectionRing != null)
        {
            selectionRing.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        if (canMove && isAlive && !isMoving)
        {
            MoveToNewPosition();
        }
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;

        if (!canMove && selectionRing != null)
        {
            selectionRing.SetActive(false);
        }

        // แสดงข้อความเตือนสำหรับ Local Player
        if (canMove && isLocalPlayer)
        {
            Debug.Log($"[{playerName}] 🎮 Press WASD to move!");
        }
    }

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

    public bool IsOnSafePlatform(float platformRotation)
    {
        if (!isAlive) return true;

        float normalizedRotation = ((platformRotation % 360f) + 360f) % 360f;
        float playerAngleRelativeToPlatform = ((currentAngle + normalizedRotation) % 360f + 360f) % 360f;

        float dangerZoneSize = 22.5f;

        if (IsInDangerZone(playerAngleRelativeToPlatform, 0f, dangerZoneSize) ||
            IsInDangerZone(playerAngleRelativeToPlatform, 90f, dangerZoneSize) ||
            IsInDangerZone(playerAngleRelativeToPlatform, 180f, dangerZoneSize) ||
            IsInDangerZone(playerAngleRelativeToPlatform, 270f, dangerZoneSize))
        {
            return false;
        }

        return true;
    }

    private bool IsInDangerZone(float angle, float centerAngle, float zoneSize)
    {
        float minAngle = (centerAngle - zoneSize + 360f) % 360f;
        float maxAngle = (centerAngle + zoneSize) % 360f;

        if (minAngle > maxAngle)
        {
            return angle >= minAngle || angle <= maxAngle;
        }
        else
        {
            return angle >= minAngle && angle <= maxAngle;
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

        StartCoroutine(FallAnimation());
    }

    IEnumerator FallAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - new Vector3(0, fallDistance, 0);
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 0.3f;

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float easeT = t * t;

            transform.position = Vector3.Lerp(startPos, endPos, easeT);
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

    public void SetInitialPosition(float angle, float radius, Vector3 platformCenter)
    {
        currentAngle = angle;
        distanceFromCenter = radius;

        float radians = angle * Mathf.Deg2Rad;
        transform.position = platformCenter + new Vector3(
            Mathf.Cos(radians) * radius,
            transform.position.y,
            Mathf.Sin(radians) * radius
        );

        originalPosition = transform.position;
    }

    public float GetCurrentAngle()
    {
        return currentAngle;
    }
}
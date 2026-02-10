using System.Collections;
using UnityEngine;

/// <summary>
/// สคริปต์หมุนแพลตฟอร์มแบบง่าย สำหรับเกม Mario Party
/// วางบน GameObject ที่ต้องการหมุน แล้วเรียก RotateRandom()
/// </summary>
public class SimplePlatformRotator : MonoBehaviour
{
    [Header("🎮 ตั้งค่าการหมุน")]
    [Tooltip("ระยะเวลาในการหมุน (วินาที)")]
    public float rotationDuration = 1f;

    [Tooltip("มุมที่เป็นไปได้ในการหมุน")]
    public int[] possibleRotations = { 90, 180, 270, 360 };

    [Header("⚙️ ตัวเลือกเพิ่มเติม")]
    [Tooltip("แสดง Debug Message ในคอนโซล")]
    public bool showDebugMessages = true;

    [Tooltip("หมุนอัตโนมัติเมื่อเริ่มเกม")]
    public bool autoRotateOnStart = false;

    [Tooltip("เวลารอก่อนหมุนอัตโนมัติครั้งแรก (วินาที)")]
    public float autoRotateDelay = 3f;

    // ตัวแปรภายใน
    private float currentYRotation = 0f;
    private bool isCurrentlyRotating = false;

    void Start()
    {
        // บันทึกมุมเริ่มต้น
        currentYRotation = transform.eulerAngles.y;

        if (autoRotateOnStart)
        {
            StartCoroutine(AutoRotateSequence());
        }

        if (showDebugMessages)
        {
            Debug.Log($"[Platform] เริ่มต้นที่มุม {currentYRotation}°");
        }
    }

    /// <summary>
    /// หมุนแพลตฟอร์มด้วยมุมสุ่มจาก possibleRotations
    /// เรียกฟังก์ชันนี้จาก Button หรือ Script อื่น
    /// </summary>
    public void RotateRandom()
    {
        if (isCurrentlyRotating)
        {
            if (showDebugMessages)
                Debug.LogWarning("[Platform] กำลังหมุนอยู่! กรุณารอให้เสร็จก่อน");
            return;
        }

        // สุ่มเลือกมุม
        int randomAngle = possibleRotations[Random.Range(0, possibleRotations.Length)];
        StartCoroutine(PerformRotation(randomAngle));
    }

    /// <summary>
    /// หมุนแพลตฟอร์มด้วยมุมที่กำหนด
    /// </summary>
    /// <param name="angle">มุมที่ต้องการหมุน (เช่น 90, 180)</param>
    public void RotateByAngle(int angle)
    {
        if (isCurrentlyRotating)
        {
            if (showDebugMessages)
                Debug.LogWarning("[Platform] กำลังหมุนอยู่! กรุณารอให้เสร็จก่อน");
            return;
        }

        StartCoroutine(PerformRotation(angle));
    }

    /// <summary>
    /// Coroutine หลักสำหรับการหมุน
    /// </summary>
    IEnumerator PerformRotation(float rotationAngle)
    {
        isCurrentlyRotating = true;

        if (showDebugMessages)
            Debug.Log($"[Platform] เริ่มหมุน {rotationAngle}°");

        float startRotation = currentYRotation;
        float targetRotation = currentYRotation + rotationAngle;
        float elapsedTime = 0f;

        // วนลูปหมุนจนครบเวลา
        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;

            // คำนวณความก้าวหน้า (0 ถึง 1)
            float progress = elapsedTime / rotationDuration;

            // ใช้ Smooth Step เพื่อให้การหมุนดูนุ่มนวล
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // คำนวณมุมปัจจุบัน
            currentYRotation = Mathf.Lerp(startRotation, targetRotation, smoothProgress);

            // หมุน GameObject
            transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);

            yield return null; // รอ 1 frame
        }

        // ตั้งมุมสุดท้ายให้แน่นอน
        currentYRotation = targetRotation;

        // Normalize มุมให้อยู่ในช่วง 0-360
        while (currentYRotation >= 360f)
            currentYRotation -= 360f;

        transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);

        if (showDebugMessages)
            Debug.Log($"[Platform] หมุนเสร็จ! มุมปัจจุบัน: {currentYRotation}°");

        isCurrentlyRotating = false;
    }

    /// <summary>
    /// ระบบหมุนอัตโนมัติ
    /// </summary>
    IEnumerator AutoRotateSequence()
    {
        // รอตามเวลาที่กำหนด
        yield return new WaitForSeconds(autoRotateDelay);

        while (true)
        {
            RotateRandom();

            // รอให้หมุนเสร็จ
            yield return new WaitUntil(() => !isCurrentlyRotating);

            // พักก่อนหมุนครั้งถัดไป
            yield return new WaitForSeconds(autoRotateDelay);
        }
    }

    /// <summary>
    /// หยุดการหมุนอัตโนมัติ
    /// </summary>
    public void StopAutoRotation()
    {
        StopAllCoroutines();
        isCurrentlyRotating = false;

        if (showDebugMessages)
            Debug.Log("[Platform] หยุดการหมุนอัตโนมัติแล้ว");
    }

    /// <summary>
    /// รีเซ็ตมุมกลับไป 0 องศา
    /// </summary>
    public void ResetRotation()
    {
        StopAllCoroutines();
        isCurrentlyRotating = false;
        currentYRotation = 0f;
        transform.rotation = Quaternion.identity;

        if (showDebugMessages)
            Debug.Log("[Platform] รีเซ็ตการหมุนกลับไปที่ 0° แล้ว");
    }

    /// <summary>
    /// ตรวจสอบว่ากำลังหมุนอยู่หรือไม่
    /// </summary>
    public bool IsRotating()
    {
        return isCurrentlyRotating;
    }

    /// <summary>
    /// ดึงมุมปัจจุบัน
    /// </summary>
    public float GetCurrentRotation()
    {
        return currentYRotation;
    }

    // ========================================
    // ฟังก์ชันสำหรับเรียกจาก Button หรือ Inspector
    // ========================================

    public void Rotate90()
    {
        RotateByAngle(90);
    }

    public void Rotate180()
    {
        RotateByAngle(180);
    }

    public void Rotate270()
    {
        RotateByAngle(270);
    }

    public void Rotate360()
    {
        RotateByAngle(360);
    }
}
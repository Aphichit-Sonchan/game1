using System.Collections;
using UnityEngine;

/// <summary>
/// ระบบหมุนแพลตฟอร์มแบบ Spin Wheel
/// - เริ่มช้า -> เร็วขึ้น -> ช้าลง -> หยุดที่มุม 90, 180, 270, 360...
/// </summary>
public class SpinWheelPlatformRotator : MonoBehaviour
{
    [Header("🎡 Spin Settings")]
    [Tooltip("จำนวนรอบขั้นต่ำที่จะหมุน (เช่น 3 = หมุน 1080°)")]
    public int minimumSpins = 3;

    [Tooltip("จำนวนรอบสูงสุด (เช่น 5 = หมุน 1800°)")]
    public int maximumSpins = 5;

    [Tooltip("ระยะเวลาในการหมุนทั้งหมด (วินาที)")]
    public float spinDuration = 4f;

    [Header("⚙️ Speed Curve")]
    [Tooltip("เส้นโค้งความเร็ว (ช้า-เร็ว-ช้า)")]
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("🎯 Stop Positions")]
    [Tooltip("มุมที่เป็นไปได้ในการหยุด (ต้องหาร 90 ลงตัว)")]
    public int[] possibleStopAngles = { 90, 180, 270, 360 };

    [Header("🔊 Audio (Optional)")]
    public AudioClip spinSound;
    public AudioClip stopSound;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private float currentRotation = 0f;
    private bool isSpinning = false; // ตัวแปรสำคัญที่ GameManager รอตรวจสอบ
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (spinSound != null || stopSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // เริ่มต้นเซ็ตค่ามุมปัจจุบันตาม Transform จริง
        currentRotation = transform.eulerAngles.y;
    }

    /// <summary>
    /// หมุนแพลตฟอร์มแบบสุ่ม (วิธีหลักที่ GameManager เรียกใช้)
    /// </summary>
    public void SpinRandom()
    {
        if (isSpinning)
        {
            if (showDebugInfo) Debug.LogWarning("[SpinWheel] Already spinning!");
            return;
        }

        int spins = Random.Range(minimumSpins, maximumSpins + 1);
        int stopAngle = possibleStopAngles[Random.Range(0, possibleStopAngles.Length)];

        // คำนวณมุมทั้งหมดที่จะหมุน
        float totalRotation = (spins * 360f) + stopAngle;

        if (showDebugInfo)
            Debug.Log($"[SpinWheel] Spinning {spins} rounds + {stopAngle}° = {totalRotation}° total");

        StartCoroutine(SpinRoutine(totalRotation));
    }

    public void SpinToAngle(float targetAngle)
    {
        if (isSpinning) return;

        if (targetAngle % 90 != 0)
        {
            Debug.LogError($"[SpinWheel] Target angle {targetAngle} must be divisible by 90!");
            return;
        }

        StartCoroutine(SpinRoutine(targetAngle));
    }

    IEnumerator SpinRoutine(float totalRotation)
    {
        // 1. เริ่มสถานะหมุน
        isSpinning = true;

        float startRotation = currentRotation;
        float targetRotation = currentRotation + totalRotation;
        float elapsed = 0f;

        if (audioSource != null && spinSound != null)
        {
            audioSource.clip = spinSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // ใช้ try-finally เพื่อความปลอดภัย ถ้ามี Error สถานะจะยังถูก reset
        try
        {
            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / spinDuration;

                // ใช้ Animation Curve
                float curvedProgress = speedCurve.Evaluate(progress);

                // คำนวณและหมุน
                currentRotation = Mathf.Lerp(startRotation, targetRotation, curvedProgress);
                transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

                // ปรับเสียง Pitch
                if (audioSource != null && audioSource.isPlaying)
                {
                    float speed = GetSpeedFromCurve(progress);
                    audioSource.pitch = Mathf.Lerp(0.8f, 1.5f, speed);
                }

                yield return null;
            }
        }
        finally
        {
            // 2. จบการหมุน (ส่วนนี้จะทำงานเสมอเมื่อ Loop จบ)

            // Normalize มุมให้อยู่ในช่วง 0-360 และหาร 90 ลงตัว
            float finalAngle = targetRotation % 360f;
            if (finalAngle < 0) finalAngle += 360f;
            finalAngle = Mathf.Round(finalAngle / 90f) * 90f; // ปัดเศษให้ตรงล็อก 90 เป๊ะๆ

            currentRotation = finalAngle;
            transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

            // หยุดเสียง
            if (audioSource != null)
            {
                audioSource.loop = false;
                audioSource.Stop();
                audioSource.pitch = 1f;

                if (stopSound != null)
                    audioSource.PlayOneShot(stopSound);
            }

            if (showDebugInfo)
                Debug.Log($"[SpinWheel] Stopped at {currentRotation}°");

            // 3. แจ้ง GameManager ว่าหมุนเสร็จแล้ว
            isSpinning = false;
        }
    }

    float GetSpeedFromCurve(float t)
    {
        float delta = 0.01f;
        float v1 = speedCurve.Evaluate(Mathf.Clamp01(t));
        float v2 = speedCurve.Evaluate(Mathf.Clamp01(t + delta));
        return Mathf.Abs(v2 - v1) / delta;
    }

    /// <summary>
    /// ฟังก์ชันเช็คสถานะที่ GameManager เรียกใช้ใน WaitUntil
    /// </summary>
    public bool IsSpinning()
    {
        return isSpinning;
    }

    public float GetCurrentRotation()
    {
        return currentRotation;
    }

    public void ResetRotation()
    {
        StopAllCoroutines();
        isSpinning = false;
        currentRotation = 0f;
        transform.rotation = Quaternion.identity;
        if (audioSource != null) audioSource.Stop();
    }

    // ========================================
    // Debug & Manual Controls
    // ========================================
    public void Spin90() => SpinToAngle(90);
    public void Spin180() => SpinToAngle(180);
    public void Spin270() => SpinToAngle(270);
    public void Spin360() => SpinToAngle(360);

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;
        float radius = 5f;

        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Gizmos.DrawLine(center, center + direction * radius);
        }

        Gizmos.color = Color.red;
        float currentAngle = currentRotation * Mathf.Deg2Rad;
        Vector3 currentDir = new Vector3(Mathf.Cos(currentAngle), 0, Mathf.Sin(currentAngle));
        Gizmos.DrawLine(center, center + currentDir * (radius + 1f));
    }
}
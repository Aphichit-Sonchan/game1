using System.Collections;
using UnityEngine;

/// <summary>
/// ระบบหมุนแพลตฟอร์มแบบ Spin Wheel
/// - เริ่มช้า → เร็วขึ้น → ช้าลง → หยุดที่มุม 90, 180, 270, 360...
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
    private bool isSpinning = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (spinSound != null || stopSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// หมุนแพลตฟอร์มแบบสุ่ม (วิธีหลัก)
    /// </summary>
    public void SpinRandom()
    {
        if (isSpinning)
        {
            if (showDebugInfo)
                Debug.LogWarning("[SpinWheel] Already spinning!");
            return;
        }

        // สุ่มจำนวนรอบ
        int spins = Random.Range(minimumSpins, maximumSpins + 1);
        
        // สุ่มมุมหยุด (ต้องหาร 90 ลงตัว)
        int stopAngle = possibleStopAngles[Random.Range(0, possibleStopAngles.Length)];
        
        // คำนวณมุมทั้งหมดที่จะหมุน
        float totalRotation = (spins * 360f) + stopAngle;
        
        if (showDebugInfo)
            Debug.Log($"[SpinWheel] Spinning {spins} rounds + {stopAngle}° = {totalRotation}° total");
        
        StartCoroutine(SpinRoutine(totalRotation));
    }

    /// <summary>
    /// หมุนไปยังมุมที่กำหนด (สำหรับควบคุมเอง)
    /// </summary>
    /// <param name="targetAngle">มุมเป้าหมาย (ต้องหาร 90 ลงตัว)</param>
    public void SpinToAngle(float targetAngle)
    {
        if (isSpinning)
        {
            if (showDebugInfo)
                Debug.LogWarning("[SpinWheel] Already spinning!");
            return;
        }

        // ตรวจสอบว่ามุมหาร 90 ลงตัวหรือไม่
        if (targetAngle % 90 != 0)
        {
            Debug.LogError($"[SpinWheel] Target angle {targetAngle} must be divisible by 90!");
            return;
        }

        StartCoroutine(SpinRoutine(targetAngle));
    }

    /// <summary>
    /// Coroutine หลักสำหรับการหมุน
    /// </summary>
    IEnumerator SpinRoutine(float totalRotation)
    {
        isSpinning = true;
        
        float startRotation = currentRotation;
        float targetRotation = currentRotation + totalRotation;
        float elapsed = 0f;

        // เล่นเสียงหมุน
        if (audioSource != null && spinSound != null)
        {
            audioSource.clip = spinSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // วนลูปหมุน
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / spinDuration;
            
            // ใช้ Animation Curve เพื่อให้หมุนช้า-เร็ว-ช้า
            float curvedProgress = speedCurve.Evaluate(progress);
            
            // คำนวณมุมปัจจุบัน
            currentRotation = Mathf.Lerp(startRotation, targetRotation, curvedProgress);
            
            // หมุน GameObject
            transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
            
            // ปรับ pitch ของเสียงตามความเร็ว (เสียงสูงขึ้นเมื่อหมุนเร็ว)
            if (audioSource != null && audioSource.isPlaying)
            {
                // คำนวณความเร็วจาก derivative ของ curve
                float speed = GetSpeedFromCurve(progress);
                audioSource.pitch = Mathf.Lerp(0.8f, 1.5f, speed);
            }
            
            yield return null;
        }

        // ตั้งมุมสุดท้ายให้แน่นอน
        currentRotation = targetRotation;
        
        // Normalize มุมให้อยู่ในช่วง 0-360
        float normalizedAngle = currentRotation % 360f;
        if (normalizedAngle < 0) normalizedAngle += 360f;
        
        // ปัดมุมให้หาร 90 ลงตัวเสมอ (ป้องกันข้อผิดพลาดจาก floating point)
        normalizedAngle = Mathf.Round(normalizedAngle / 90f) * 90f;
        
        currentRotation = normalizedAngle;
        transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

        // หยุดเสียงหมุนและเล่นเสียงหยุด
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
            
            if (stopSound != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(stopSound);
            }
        }

        if (showDebugInfo)
            Debug.Log($"[SpinWheel] Stopped at {currentRotation}°");

        isSpinning = false;
    }

    /// <summary>
    /// คำนวณความเร็วจาก Animation Curve
    /// </summary>
    float GetSpeedFromCurve(float t)
    {
        float delta = 0.01f;
        float v1 = speedCurve.Evaluate(Mathf.Clamp01(t));
        float v2 = speedCurve.Evaluate(Mathf.Clamp01(t + delta));
        return Mathf.Abs(v2 - v1) / delta;
    }

    /// <summary>
    /// ตรวจสอบว่ากำลังหมุนอยู่หรือไม่
    /// </summary>
    public bool IsSpinning()
    {
        return isSpinning;
    }

    /// <summary>
    /// ดึงมุมปัจจุบัน
    /// </summary>
    public float GetCurrentRotation()
    {
        return currentRotation;
    }

    /// <summary>
    /// รีเซ็ตการหมุน
    /// </summary>
    public void ResetRotation()
    {
        StopAllCoroutines();
        isSpinning = false;
        currentRotation = 0f;
        transform.rotation = Quaternion.identity;
        
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        if (showDebugInfo)
            Debug.Log("[SpinWheel] Reset to 0°");
    }

    // ========================================
    // ฟังก์ชันสำหรับเรียกจาก Button หรือ Inspector
    // ========================================

    public void Spin90() => SpinToAngle(90);
    public void Spin180() => SpinToAngle(180);
    public void Spin270() => SpinToAngle(270);
    public void Spin360() => SpinToAngle(360);
    public void Spin450() => SpinToAngle(450);
    public void Spin540() => SpinToAngle(540);
    public void Spin720() => SpinToAngle(720);
    public void Spin1080() => SpinToAngle(1080);

    // วาดเส้นช่วยแสดงมุม 90 องศา
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;
        float radius = 5f;

        // วาดเส้นแสดงมุม 0, 90, 180, 270
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Gizmos.DrawLine(center, center + direction * radius);
        }

        // แสดงทิศทางปัจจุบัน
        Gizmos.color = Color.red;
        float currentAngle = currentRotation * Mathf.Deg2Rad;
        Vector3 currentDir = new Vector3(Mathf.Cos(currentAngle), 0, Mathf.Sin(currentAngle));
        Gizmos.DrawLine(center, center + currentDir * (radius + 1f));
    }
}

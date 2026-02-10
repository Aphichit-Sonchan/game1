using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ระบบจัดการพื้นที่แพลตฟอร์มที่หายไป
/// หลังจากหมุนเสร็จ พื้นบางส่วนจะหาย ทำให้ผู้เล่นตก
/// </summary>
public class PlatformSectionManager : MonoBehaviour
{
    [Header("🎯 Platform Sections (4 ส่วน)")]
    [Tooltip("ลาก GameObject ของพื้นทั้ง 4 ส่วนมาใส่")]
    public List<GameObject> platformSections = new List<GameObject>();
    
    [Header("⏱️ Timing Settings")]
    [Tooltip("เวลาที่พื้นหายไป (วินาที)")]
    public float disappearDuration = 3f;
    
    [Tooltip("ความเร็วในการหายและกลับมา")]
    public float fadeSpeed = 2f;
    
    [Header("🎨 Visual Effects")]
    [Tooltip("เปิดใช้งานเอฟเฟกต์การหาย")]
    public bool useFadeEffect = true;
    
    [Tooltip("สีเตือนก่อนพื้นหาย")]
    public Color warningColor = Color.red;
    
    [Tooltip("เวลาแสดงสัญญาณเตือน (วินาที)")]
    public float warningTime = 0.5f;
    
    [Header("🔊 Audio (Optional)")]
    public AudioClip disappearSound;
    public AudioClip reappearSound;
    public AudioClip warningSound;
    
    private AudioSource audioSource;
    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
    private bool isSectionDisappearing = false;

    void Start()
    {
        // เตรียม AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (disappearSound != null || reappearSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // เก็บ Material เดิมไว้
        foreach (var section in platformSections)
        {
            if (section != null)
            {
                Renderer[] renderers = section.GetComponentsInChildren<Renderer>();
                List<Material> mats = new List<Material>();
                
                foreach (var renderer in renderers)
                {
                    mats.AddRange(renderer.materials);
                }
                
                originalMaterials[section] = mats.ToArray();
            }
        }
    }

    /// <summary>
    /// ทำให้พื้นบางส่วนหายไป (เรียกหลังจากหมุนเสร็จ)
    /// </summary>
    public void MakeRandomSectionDisappear()
    {
        if (isSectionDisappearing || platformSections.Count == 0)
            return;
        
        // สุ่มเลือกส่วนที่จะหาย
        int randomIndex = Random.Range(0, platformSections.Count);
        GameObject targetSection = platformSections[randomIndex];
        
        if (targetSection != null)
        {
            StartCoroutine(DisappearAndReappearSequence(targetSection));
        }
    }

    /// <summary>
    /// ทำให้หลายส่วนหายพร้อมกัน
    /// </summary>
    /// <param name="numberOfSections">จำนวนส่วนที่จะหาย</param>
    public void MakeMultipleSectionsDisappear(int numberOfSections)
    {
        if (isSectionDisappearing || platformSections.Count == 0)
            return;
        
        // สุ่มเลือกหลายส่วน
        List<GameObject> selectedSections = new List<GameObject>();
        List<int> availableIndices = new List<int>();
        
        for (int i = 0; i < platformSections.Count; i++)
        {
            availableIndices.Add(i);
        }
        
        int count = Mathf.Min(numberOfSections, platformSections.Count);
        
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            int sectionIndex = availableIndices[randomIndex];
            selectedSections.Add(platformSections[sectionIndex]);
            availableIndices.RemoveAt(randomIndex);
        }
        
        StartCoroutine(DisappearMultipleSections(selectedSections));
    }

    /// <summary>
    /// ลำดับการหายและกลับมาของพื้น 1 ส่วน
    /// </summary>
    IEnumerator DisappearAndReappearSequence(GameObject section)
    {
        isSectionDisappearing = true;
        
        Debug.Log($"[Platform] พื้นส่วน {section.name} กำลังจะหาย!");
        
        // 1. แสดงสัญญาณเตือน
        yield return StartCoroutine(ShowWarning(section));
        
        // 2. ทำให้พื้นหาย
        yield return StartCoroutine(DisappearSection(section));
        
        // 3. รอตามเวลาที่กำหนด
        Debug.Log($"[Platform] รอ {disappearDuration} วินาที...");
        yield return new WaitForSeconds(disappearDuration);
        
        // 4. ทำให้พื้นกลับมา
        yield return StartCoroutine(ReappearSection(section));
        
        Debug.Log($"[Platform] พื้นส่วน {section.name} กลับมาแล้ว!");
        
        isSectionDisappearing = false;
    }

    /// <summary>
    /// ลำดับการหายและกลับมาของพื้นหลายส่วน
    /// </summary>
    IEnumerator DisappearMultipleSections(List<GameObject> sections)
    {
        isSectionDisappearing = true;
        
        // 1. แสดงสัญญาณเตือนทุกส่วน
        List<Coroutine> warningCoroutines = new List<Coroutine>();
        foreach (var section in sections)
        {
            warningCoroutines.Add(StartCoroutine(ShowWarning(section)));
        }
        
        // รอให้เตือนเสร็จ
        yield return new WaitForSeconds(warningTime);
        
        // 2. ทำให้ทุกส่วนหายพร้อมกัน
        foreach (var section in sections)
        {
            StartCoroutine(DisappearSection(section));
        }
        
        yield return new WaitForSeconds(1f / fadeSpeed);
        
        // 3. รอตามเวลาที่กำหนด
        yield return new WaitForSeconds(disappearDuration);
        
        // 4. ทำให้ทุกส่วนกลับมาพร้อมกัน
        foreach (var section in sections)
        {
            StartCoroutine(ReappearSection(section));
        }
        
        yield return new WaitForSeconds(1f / fadeSpeed);
        
        isSectionDisappearing = false;
    }

    /// <summary>
    /// แสดงสัญญาณเตือนก่อนพื้นหาย
    /// </summary>
    IEnumerator ShowWarning(GameObject section)
    {
        if (!useFadeEffect)
            yield break;
        
        // เล่นเสียงเตือน
        if (audioSource != null && warningSound != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
        
        Renderer[] renderers = section.GetComponentsInChildren<Renderer>();
        float elapsed = 0f;
        
        // กระพริบสีเตือน
        while (elapsed < warningTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 10f, 1f);
            
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        mat.color = Color.Lerp(Color.white, warningColor, t);
                    }
                }
            }
            
            yield return null;
        }
    }

    /// <summary>
    /// ทำให้พื้นหาย
    /// </summary>
    IEnumerator DisappearSection(GameObject section)
    {
        // เล่นเสียงหาย
        if (audioSource != null && disappearSound != null)
        {
            audioSource.PlayOneShot(disappearSound);
        }
        
        Renderer[] renderers = section.GetComponentsInChildren<Renderer>();
        Collider[] colliders = section.GetComponentsInChildren<Collider>();
        
        if (useFadeEffect)
        {
            // ค่อยๆ จางหาย
            float elapsed = 0f;
            float duration = 1f / fadeSpeed;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration);
                
                foreach (var renderer in renderers)
                {
                    foreach (var mat in renderer.materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = alpha;
                            mat.color = color;
                        }
                    }
                }
                
                yield return null;
            }
        }
        
        // ปิด Renderer และ Collider
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        
        Debug.Log($"[Platform] {section.name} หายไปแล้ว!");
    }

    /// <summary>
    /// ทำให้พื้นกลับมา
    /// </summary>
    IEnumerator ReappearSection(GameObject section)
    {
        // เล่นเสียงกลับมา
        if (audioSource != null && reappearSound != null)
        {
            audioSource.PlayOneShot(reappearSound);
        }
        
        Renderer[] renderers = section.GetComponentsInChildren<Renderer>();
        Collider[] colliders = section.GetComponentsInChildren<Collider>();
        
        // เปิด Renderer และ Collider
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }
        
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }
        
        if (useFadeEffect)
        {
            // ค่อยๆ ปรากฏ
            float elapsed = 0f;
            float duration = 1f / fadeSpeed;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = elapsed / duration;
                
                foreach (var renderer in renderers)
                {
                    foreach (var mat in renderer.materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = alpha;
                            mat.color = color;
                        }
                    }
                }
                
                yield return null;
            }
        }
        
        // คืนสีเดิม
        RestoreOriginalColors(section);
        
        Debug.Log($"[Platform] {section.name} กลับมาแล้ว!");
    }

    /// <summary>
    /// คืนสีเดิมให้กับพื้น
    /// </summary>
    void RestoreOriginalColors(GameObject section)
    {
        if (!originalMaterials.ContainsKey(section))
            return;
        
        Renderer[] renderers = section.GetComponentsInChildren<Renderer>();
        Material[] originals = originalMaterials[section];
        int index = 0;
        
        foreach (var renderer in renderers)
        {
            for (int i = 0; i < renderer.materials.Length && index < originals.Length; i++, index++)
            {
                renderer.materials[i].color = originals[index].color;
            }
        }
    }

    /// <summary>
    /// ตรวจสอบว่ากำลังทำให้พื้นหายอยู่หรือไม่
    /// </summary>
    public bool IsDisappearing()
    {
        return isSectionDisappearing;
    }
}

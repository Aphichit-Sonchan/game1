using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSectionManager : MonoBehaviour
{
    [Header("🎯 Platform Sections (4 ส่วน)")]
    public List<GameObject> platformSections = new List<GameObject>();

    [Header("⏱️ Timing Settings")]
    public float disappearDuration = 3f;
    public float fadeSpeed = 2f;

    [Header("🎨 Visual Effects")]
    public bool useFadeEffect = true;
    public Color warningColor = Color.red;

    [Header("🎯 Danger Zones")]
    public bool showDangerZonesAlways = true;
    [Range(0f, 1f)]
    public float dangerZoneAlpha = 0.3f;

    [Header("🔊 Audio (Optional)")]
    public AudioClip disappearSound;
    public AudioClip reappearSound;
    public AudioClip warningSound;

    private AudioSource audioSource;
    private Dictionary<GameObject, List<Color>> originalColors = new Dictionary<GameObject, List<Color>>();
    private bool isSectionDisappearing = false;
    private List<GameObject> dangerZoneOverlays = new List<GameObject>();

    // ตัวแปรสำหรับระบบใหม่
    private GameObject currentTargetSection;
    private Coroutine warningCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (disappearSound != null || reappearSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // เก็บสีเดิมของพื้นไว้
        foreach (var section in platformSections)
        {
            if (section != null)
            {
                Renderer[] renderers = section.GetComponentsInChildren<Renderer>();
                List<Color> colors = new List<Color>();
                foreach (var renderer in renderers)
                {
                    foreach (var mat in renderer.materials)
                    {
                        string colorProp = mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                        colors.Add(mat.HasProperty(colorProp) ? mat.GetColor(colorProp) : Color.white);
                    }
                }
                originalColors[section] = colors;
            }
        }

        if (showDangerZonesAlways)
        {
            CreateDangerZoneOverlays();
        }
    }

    // --------------------------------------------------------------------------
    // ✅ ส่วนที่แก้ไข Logic ใหม่ (แยกการเตือน กับ การหาย ออกจากกัน)
    // --------------------------------------------------------------------------

    /// <summary>
    /// 1. เริ่มเฟสเตือน: สุ่มเลือกพื้นและสั่งให้กระพริบวนไปเรื่อยๆ (GameManager เรียกตอนเริ่มหมุน)
    /// </summary>
    public void StartWarningPhase()
    {
        if (isSectionDisappearing || platformSections.Count == 0) return;

        // สุ่มเลือกพื้นที่จะหาย
        int randomIndex = Random.Range(0, platformSections.Count);
        currentTargetSection = platformSections[randomIndex];

        // เริ่มกระพริบ (วนลูปไม่รู้จบ จนกว่าจะสั่งหยุด)
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(LoopWarningEffect(currentTargetSection));
    }

    /// <summary>
    /// 2. สั่งให้หาย: หยุดกระพริบแล้วเริ่มจางหายจริง (GameManager เรียกตอนล้อหยุดหมุน)
    /// </summary>
    public void TriggerDisappear()
    {
        if (currentTargetSection == null) return;

        // หยุดการกระพริบ
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);

        // เริ่มกระบวนการหายไป
        StartCoroutine(DisappearSequence(currentTargetSection));
    }

    // Coroutine กระพริบไฟเตือนแบบวนลูป (รอคำสั่งหยุด)
    IEnumerator LoopWarningEffect(GameObject section)
    {
        if (audioSource != null && warningSound != null) audioSource.PlayOneShot(warningSound);
        Renderer[] renderers = section.GetComponentsInChildren<Renderer>();

        float startTime = Time.time;

        while (true)
        {
            float elapsed = Time.time - startTime;
            // ยิ่งนาน ยิ่งกระพริบเร็ว (ลูกเล่นเสริม)
            float flashSpeed = Mathf.Lerp(3f, 20f, elapsed / 5.0f);
            float t = Mathf.PingPong(Time.time * flashSpeed, 1f);

            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.materials)
                {
                    string colorProp = mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                    if (mat.HasProperty(colorProp))
                    {
                        mat.SetColor(colorProp, Color.Lerp(Color.white, warningColor, t));
                    }
                }
            }
            yield return null;
        }
    }

    // ลำดับการหายไป -> รอ -> กลับมา
    IEnumerator DisappearSequence(GameObject section)
    {
        isSectionDisappearing = true;

        // สั่งให้จางหาย (Fade Out)
        yield return StartCoroutine(DisappearSection(section));

        // รอเวลาที่กำหนด (ช่วงที่ผู้เล่นจะร่วง)
        yield return new WaitForSeconds(disappearDuration);

        // สั่งให้กลับมา (Fade In)
        yield return StartCoroutine(ReappearSection(section));

        // รีเซ็ตค่า
        isSectionDisappearing = false;
        currentTargetSection = null;
    }

    // --------------------------------------------------------------------------
    // จบส่วนแก้ไข
    // --------------------------------------------------------------------------

    void CreateDangerZoneOverlays()
    {
        foreach (var section in platformSections)
        {
            if (section == null) continue;
            GameObject overlay = GameObject.CreatePrimitive(PrimitiveType.Cube);
            overlay.name = $"{section.name}_DangerOverlay";
            overlay.transform.SetParent(section.transform);
            overlay.transform.localPosition = new Vector3(0, 0.01f, 0);
            overlay.transform.localRotation = Quaternion.identity;
            overlay.transform.localScale = new Vector3(1, 0.01f, 1);
            Destroy(overlay.GetComponent<Collider>());

            Material redMaterial = new Material(Shader.Find("Standard"));
            Color redColor = Color.red;
            redColor.a = dangerZoneAlpha;
            redMaterial.color = redColor;

            // ตั้งค่า Material ให้เป็น Transparent
            redMaterial.SetFloat("_Mode", 3);
            redMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            redMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            redMaterial.SetInt("_ZWrite", 0);
            redMaterial.renderQueue = 3000;

            overlay.GetComponent<Renderer>().material = redMaterial;
            overlay.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dangerZoneOverlays.Add(overlay);
        }
    }

    IEnumerator DisappearSection(GameObject section)
    {
        if (audioSource != null && disappearSound != null) audioSource.PlayOneShot(disappearSound);
        Renderer[] renderers = section.GetComponentsInChildren<Renderer>();
        Collider[] colliders = section.GetComponentsInChildren<Collider>();

        if (useFadeEffect)
        {
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
                        string colorProp = mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                        if (mat.HasProperty(colorProp))
                        {
                            Color c = mat.GetColor(colorProp); c.a = alpha; mat.SetColor(colorProp, c);
                        }
                    }
                }
                yield return null;
            }
        }
        foreach (var r in renderers) r.enabled = false;
        foreach (var c in colliders) c.enabled = false;
    }

    IEnumerator ReappearSection(GameObject section)
    {
        if (audioSource != null && reappearSound != null) audioSource.PlayOneShot(reappearSound);
        Renderer[] renderers = section.GetComponentsInChildren<Renderer>();
        Collider[] colliders = section.GetComponentsInChildren<Collider>();
        foreach (var r in renderers) r.enabled = true;
        foreach (var c in colliders) c.enabled = true;

        if (useFadeEffect)
        {
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
                        string colorProp = mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                        if (mat.HasProperty(colorProp))
                        {
                            Color c = mat.GetColor(colorProp); c.a = alpha; mat.SetColor(colorProp, c);
                        }
                    }
                }
                yield return null;
            }
        }
        RestoreOriginalColors(section);
    }

    void RestoreOriginalColors(GameObject section)
    {
        if (!originalColors.ContainsKey(section)) return;
        Renderer[] renderers = section.GetComponentsInChildren<Renderer>();
        List<Color> colors = originalColors[section];
        int colorIndex = 0;
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                if (colorIndex < colors.Count)
                {
                    string prop = mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                    if (mat.HasProperty(prop)) mat.SetColor(prop, colors[colorIndex]);
                    colorIndex++;
                }
            }
        }
    }

    public bool IsDisappearing() => isSectionDisappearing;
}
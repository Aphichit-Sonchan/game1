using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Game Manager สำหรับ Mario Party (เวอร์ชันสมบูรณ์ แก้เดินไม่ได้ Round 1)
/// </summary>
public class MarioPartyGameManager : MonoBehaviour
{
    [Header("Component References")]
    public SpinWheelPlatformRotator platformSpinner;
    public PlatformSectionManager sectionManager;

    [Header("Players")]
    public PlayerController[] players;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI roundText;
    public Button startButton;
    public Button resetButton;
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;

    [Header("Game Settings")]
    [Tooltip("จำนวนส่วนของพื้นที่จะหาย (1-4)")]
    [Range(1, 4)]
    public int sectionsToDisappear = 1;

    [Header("Audio (Optional)")]
    public AudioClip roundStartSound;
    public AudioClip eliminationSound;

    private int currentRound = 1;
    private bool isGameRunning = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (resetButton != null) resetButton.onClick.AddListener(ResetGame);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (statusText != null) statusText.text = "Press Start to Play!";
        if (roundText != null) roundText.text = "Round 1";
    }

    public void StartGame()
    {
        if (isGameRunning) return;
        if (platformSpinner == null || sectionManager == null)
        {
            Debug.LogError("PlatformSpinner or SectionManager is missing!");
            return;
        }

        isGameRunning = true;
        if (startButton != null) startButton.interactable = false;

        currentRound = 1;
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (GetAlivePlayersCount() > 1)
        {
            yield return StartCoroutine(PlayRound());
            currentRound++;
            yield return new WaitForSeconds(1.5f);
        }
        EndGame();
    }

    IEnumerator PlayRound()
    {
        if (roundText != null)
            roundText.text = $"Round {currentRound}";

        // ✅ แก้ไข: ปลดล็อคให้ผู้เล่นเดินได้ทันทีที่เริ่มรอบ!
        // (เพื่อให้เดินไปหาที่ปลอดภัยระหว่างที่ล้อกำลังหมุน)
        SetPlayersCanMove(true);

        // Phase 1: หมุนแพลตฟอร์ม
        yield return StartCoroutine(SpinPhase());

        // Phase 2: หยุดผู้เล่น -> ตรวจสอบ -> พื้นกลับ
        yield return StartCoroutine(FreezeAndCheckPhase());

        Debug.Log($"[Game] Round {currentRound} complete!");
    }

    IEnumerator SpinPhase()
    {
        if (statusText != null)
            statusText.text = "Spinning & Warning...";

        if (audioSource != null && roundStartSound != null)
            audioSource.PlayOneShot(roundStartSound);

        if (sectionManager != null)
        {
            sectionManager.StartWarningPhase();
        }

        platformSpinner.SpinRandom();

        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => !platformSpinner.IsSpinning());

        if (statusText != null)
            statusText.text = "Spin Complete!";

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator FreezeAndCheckPhase()
    {
        // 1. หยุดผู้เล่นทุกคน (Freeze เพื่อเช็ค)
        SetPlayersCanMove(false);

        if (statusText != null)
            statusText.text = "FREEZE! Don't move!";

        Debug.Log("[GameManager] Players FROZEN!");

        // 2. พื้นหาย
        if (sectionManager != null)
        {
            sectionManager.TriggerDisappear();
        }

        if (statusText != null)
            statusText.text = "Floor disappearing...";

        yield return new WaitForSeconds(0.8f);

        // 3. ตรวจสอบผู้เล่นทีละคน
        if (statusText != null)
            statusText.text = "Checking players...";

        yield return StartCoroutine(CheckPlayersSequence());

        // 4. พื้นกลับมา
        if (statusText != null)
            statusText.text = "Floor returning...";

        if (sectionManager != null)
        {
            yield return new WaitUntil(() => !sectionManager.IsDisappearing());
        }

        if (statusText != null)
            statusText.text = "Floor returned!";

        yield return new WaitForSeconds(0.5f);

        // 5. ปลดล็อคให้เดินได้อีกครั้ง (เตรียมพร้อมรอบหน้า)
        SetPlayersCanMove(true);

        if (statusText != null)
            statusText.text = "You can move again!";

        Debug.Log("[GameManager] Players UNFROZEN!");

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator CheckPlayersSequence()
    {
        int eliminatedCount = 0;
        if (players == null) yield break;

        foreach (var player in players)
        {
            if (player != null && player.isAlive)
            {
                if (statusText != null)
                    statusText.text = $"Checking {player.playerName}...";

                yield return new WaitForSeconds(0.5f);

                bool isFalling = IsPlayerOnDisappearedSection(player);

                if (isFalling)
                {
                    if (statusText != null)
                        statusText.text = $"{player.playerName} is falling!";

                    player.Eliminate();
                    eliminatedCount++;

                    if (audioSource != null && eliminationSound != null)
                        audioSource.PlayOneShot(eliminationSound);

                    yield return new WaitForSeconds(1.5f);
                }
            }
        }

        if (eliminatedCount > 0)
        {
            if (statusText != null)
                statusText.text = $"{eliminatedCount} player(s) fell!";
        }
        else
        {
            if (statusText != null)
                statusText.text = "Everyone is safe!";
            yield return new WaitForSeconds(1.0f);
        }
    }

    bool IsPlayerOnDisappearedSection(PlayerController player)
    {
        Vector3 origin = player.transform.position + Vector3.up * 2.0f;
        Vector3 direction = Vector3.down;
        float maxDistance = 10.0f;
        float radius = 0.2f;

        RaycastHit[] hits = Physics.SphereCastAll(origin, radius, direction, maxDistance);

        bool foundSafeGround = false;

        foreach (var hit in hits)
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj == player.gameObject) continue;

            foundSafeGround = true;
            Debug.DrawLine(origin, hit.point, Color.green, 2.0f);
            break;
        }

        if (!foundSafeGround)
        {
            Debug.DrawRay(origin, direction * maxDistance, Color.red, 2.0f);
        }

        return !foundSafeGround;
    }

    void SetPlayersCanMove(bool canMove)
    {
        if (players == null) return;
        foreach (var player in players)
        {
            if (player != null && player.isAlive)
            {
                player.SetCanMove(canMove);
            }
        }
    }

    int GetAlivePlayersCount()
    {
        int count = 0;
        if (players == null) return count;
        foreach (var player in players)
        {
            if (player != null && player.isAlive) count++;
        }
        return count;
    }

    void EndGame()
    {
        isGameRunning = false;
        PlayerController winner = null;
        if (players != null)
        {
            foreach (var player in players)
            {
                if (player != null && player.isAlive)
                {
                    winner = player;
                    break;
                }
            }
        }

        if (winnerText != null)
        {
            if (winner != null) winnerText.text = $"Winner: {winner.playerName}!";
            else winnerText.text = "It's a Tie!";
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (statusText != null) statusText.text = "Game Over!";
    }

    public void ResetGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
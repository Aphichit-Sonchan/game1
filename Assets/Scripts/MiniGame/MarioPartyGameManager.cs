using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Game Manager สำหรับ Spin Wheel Game
/// ลำดับ: สุ่มโซนแดง → หมุนวงล้อ → ผู้เล่นในโซนแดงตก
/// </summary>
public class MarioPartyGameManager : MonoBehaviour
{
    [Header("🎮 Component References")]
    public SpinWheelPlatformRotator platformSpinner;
    public SpinWheelSectionManager sectionManager;

    [Header("👥 Players")]
    public PlayerController[] players;

    [Header("⏱️ UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI roundText;
    public Button startButton;
    public Button resetButton;
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;

    [Header("⚙️ Game Settings")]
    [Tooltip("เวลานับถอยหลังที่ผู้เล่นเคลื่อนที่ได้ (วินาที)")]
    public float movementTime = 5f;

    [Header("🎵 Audio (Optional)")]
    public AudioClip roundStartSound;
    public AudioClip countdownSound;
    public AudioClip eliminationSound;

    private int currentRound = 1;
    private bool isGameRunning = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        else
            Debug.LogError("[GameManager] Start Button not assigned!");

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetGame);
        else
            Debug.LogError("[GameManager] Reset Button not assigned!");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (statusText != null)
            statusText.text = "Press Start to Play!";

        if (roundText != null)
            roundText.text = "Round 1";

        if (platformSpinner == null)
            Debug.LogError("[GameManager] Platform Spinner not assigned!");

        if (sectionManager == null)
            Debug.LogError("[GameManager] Section Manager not assigned!");

        if (players == null || players.Length == 0)
            Debug.LogWarning("[GameManager] No Players assigned!");
    }

    public void StartGame()
    {
        if (isGameRunning) return;

        if (platformSpinner == null || sectionManager == null ||
            players == null || players.Length == 0)
        {
            Debug.LogError("[GameManager] Cannot start! Missing components");
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

        // Phase 1: สุ่มโซนอันตราย (1 ส่วนเป็นสีแดง)
        yield return StartCoroutine(SelectDangerZonePhase());

        // Phase 2: หมุนวงล้อ
        yield return StartCoroutine(SpinPhase());

        // Phase 3: ตรวจสอบและตัดผู้เล่น
        yield return StartCoroutine(CheckPlayersPhase());

        Debug.Log($"[Game] Round {currentRound} complete!");
    }

    /// <summary>
    /// Phase 1: สุ่มเลือกโซนอันตราย (สีแดง)
    /// </summary>
    IEnumerator SelectDangerZonePhase()
    {
        if (statusText != null)
            statusText.text = "🎲 Selecting danger zone...";

        // สุ่มเลือก 1 ส่วนเป็นสีแดง
        sectionManager.SelectRandomDangerSection();

        yield return new WaitForSeconds(2f);
    }

    /// <summary>
    /// Phase 2: หมุนวงล้อ
    /// </summary>
    IEnumerator SpinPhase()
    {
        if (statusText != null)
            statusText.text = "🎡 Spinning...";

        if (audioSource != null && roundStartSound != null)
            audioSource.PlayOneShot(roundStartSound);

        // เริ่มหมุน
        platformSpinner.SpinRandom();

        // รอจนกว่าจะหมุนเสร็จ
        yield return new WaitUntil(() => !platformSpinner.IsSpinning());

        if (statusText != null)
            statusText.text = "🎯 Wheel stopped!";

        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// Phase 3: ตรวจสอบผู้เล่นและตัดคนที่อยู่ในโซนแดง
    /// </summary>
    IEnumerator CheckPlayersPhase()
    {
        if (statusText != null)
            statusText.text = "⚠️ Checking positions...";

        yield return new WaitForSeconds(1f);

        // ตรวจสอบและตัดผู้เล่น
        int eliminatedCount = CheckAndEliminatePlayers();

        if (eliminatedCount > 0)
        {
            if (statusText != null)
                statusText.text = $"💀 {eliminatedCount} player(s) eliminated!";

            if (audioSource != null && eliminationSound != null)
                audioSource.PlayOneShot(eliminationSound);
        }
        else
        {
            if (statusText != null)
                statusText.text = "✅ Everyone is safe!";
        }

        yield return new WaitForSeconds(2f);
    }

    /// <summary>
    /// ตรวจสอบและตัดผู้เล่นที่อยู่ในโซนอันตราย
    /// </summary>
    int CheckAndEliminatePlayers()
    {
        int count = 0;
        if (players == null) return count;

        // ดึงมุมปัจจุบันของแพลตฟอร์ม
        float platformRotation = platformSpinner.GetCurrentRotation();

        Debug.Log($"[GameManager] Platform rotation: {platformRotation}°");

        foreach (var player in players)
        {
            if (player != null && player.isAlive)
            {
                // ตรวจสอบว่าอยู่ในโซนอันตรายหรือไม่
                if (sectionManager.IsPlayerInDangerZone(player, platformRotation))
                {
                    player.Eliminate();
                    count++;
                }
            }
        }

        return count;
    }

    void SetPlayersCanMove(bool canMove)
    {
        if (players == null) return;

        foreach (var player in players)
        {
            if (player != null && player.isAlive)
                player.SetCanMove(canMove);
        }
    }

    int GetAlivePlayersCount()
    {
        int count = 0;
        if (players == null) return count;

        foreach (var player in players)
        {
            if (player != null && player.isAlive)
                count++;
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
            if (winner != null)
                winnerText.text = $"🎉 Winner: {winner.playerName}! 🎉";
            else
                winnerText.text = "It's a Tie!";
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (statusText != null)
            statusText.text = "Game Over!";
    }

    public void ResetGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
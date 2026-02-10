using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlatformRotationManager : MonoBehaviour
{
    [Header("Platform Settings")]
    public Transform platform;
    public float rotationDuration = 2f;
    
    [Header("Player Settings")]
    public List<PlayerController> players = new List<PlayerController>();
    
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI rotationInfoText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    public Button startButton;
    public Button resetButton;
    
    [Header("Game Settings")]
    public float countdownTime = 5f;
    public float checkDelay = 1.5f;
    
    private bool isGameRunning = false;
    private int currentRound = 1;
    private float currentPlatformRotation = 0f;
    
    private int[] possibleRotations = { 90, 180, 270, 360 };

    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        resetButton.onClick.AddListener(ResetGame);
        gameOverPanel.SetActive(false);
        statusText.text = "กดเริ่มเกมเพื่อเล่น";
        rotationInfoText.text = "รอการหมุน...";
    }

    public void StartGame()
    {
        if (isGameRunning) return;
        
        isGameRunning = true;
        startButton.interactable = false;
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (GetAlivePlayers().Count > 1)
        {
            yield return StartCoroutine(PlayRound());
            currentRound++;
            yield return new WaitForSeconds(2f);
        }
        
        EndGame();
    }

    IEnumerator PlayRound()
    {
        // Phase 1: Rotation
        statusText.text = $"รอบที่ {currentRound} - กำลังหมุน...";
        
        foreach (var player in players)
        {
            if (player.isAlive)
            {
                player.SetCanMove(false);
            }
        }
        
        int rotationAmount = possibleRotations[Random.Range(0, possibleRotations.Length)];
        rotationInfoText.text = $"หมุน {rotationAmount}°";
        
        yield return StartCoroutine(RotatePlatform(rotationAmount));
        
        // Phase 2: Countdown and movement
        statusText.text = "เคลื่อนที่เลย!";
        
        foreach (var player in players)
        {
            if (player.isAlive)
            {
                player.SetCanMove(true);
            }
        }
        
        yield return StartCoroutine(Countdown());
        
        // Phase 3: Check eliminations
        
        foreach (var player in players)
        {
            player.SetCanMove(false);
        }
        
        statusText.text = "กำลังตรวจสอบ...";
        
        yield return StartCoroutine(CheckEliminations());
    }

    IEnumerator RotatePlatform(float rotationAmount)
    {
        float startRotation = currentPlatformRotation;
        float targetRotation = currentPlatformRotation + rotationAmount;
        float elapsed = 0f;
        
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;
            
            currentPlatformRotation = Mathf.Lerp(startRotation, targetRotation, t);
            platform.rotation = Quaternion.Euler(0, currentPlatformRotation, 0);
            
            yield return null;
        }
        
        currentPlatformRotation = targetRotation % 360f;
        platform.rotation = Quaternion.Euler(0, currentPlatformRotation, 0);
    }

    IEnumerator Countdown()
    {
        for (int i = (int)countdownTime; i > 0; i--)
        {
            timerText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        
        timerText.text = "0";
    }

    IEnumerator CheckEliminations()
    {
        int eliminatedCount = 0;
        
        foreach (var player in players)
        {
            if (player.isAlive && !player.IsOnSafePlatform(currentPlatformRotation))
            {
                player.Eliminate();
                eliminatedCount++;
            }
        }
        
        if (eliminatedCount > 0)
        {
            statusText.text = $"มี {eliminatedCount} คนตกลงไป!";
        }
        else
        {
            statusText.text = "ทุกคนปลอดภัย!";
        }
        
        yield return new WaitForSeconds(checkDelay);
    }

    List<PlayerController> GetAlivePlayers()
    {
        List<PlayerController> alivePlayers = new List<PlayerController>();
        
        foreach (var player in players)
        {
            if (player.isAlive)
            {
                alivePlayers.Add(player);
            }
        }
        
        return alivePlayers;
    }

    void EndGame()
    {
        List<PlayerController> winners = GetAlivePlayers();
        
        if (winners.Count == 1)
        {
            winnerText.text = $"ผู้ชนะคือ {winners[0].playerName}!";
        }
        else
        {
            winnerText.text = "เสมอกัน!";
        }
        
        gameOverPanel.SetActive(true);
    }

    public void ResetGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}

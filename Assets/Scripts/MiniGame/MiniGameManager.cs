
using System.Collections;
using UnityEngine;

namespace MiniGame
{
    public class MiniGameManager : MonoBehaviour
    {
        public static MiniGameManager Instance { get; private set; }
        public bool IsGameActive { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            StartCoroutine(StartGameRoutine());
        }

        private IEnumerator StartGameRoutine()
        {
            IsGameActive = false;
            yield return null; // Wait for initialization

            if (UIManager.Instance != null) UIManager.Instance.UpdateCountdown("3");
            yield return new WaitForSeconds(1f);

            if (UIManager.Instance != null) UIManager.Instance.UpdateCountdown("2");
            yield return new WaitForSeconds(1f);

            if (UIManager.Instance != null) UIManager.Instance.UpdateCountdown("1");
            yield return new WaitForSeconds(1f);

            if (UIManager.Instance != null) UIManager.Instance.UpdateCountdown("GO!");
            StartGame();

            yield return new WaitForSeconds(1f);
            if (UIManager.Instance != null) UIManager.Instance.UpdateCountdown("");
        }


        public void StartGame()
        {
            Debug.Log("MiniGame Started!");
            IsGameActive = true;
            StartCoroutine(GameLoop());
        }

        private IEnumerator GameLoop()
        {
            ArenaController arena = FindFirstObjectByType<ArenaController>();
            int totalRounds = 5;
            
            for (int round = 1; round <= totalRounds; round++)
            {
                if (!IsGameActive) break;
                
                // Show round number
                if (UIManager.Instance != null) UIManager.Instance.UpdateRound(round, totalRounds);
                
                // Show random hazard sectors (2 at a time)
                if (arena != null) arena.SetRandomHazards(2);
                
                // 5-second countdown on center timer
                for (int i = 5; i >= 0; i--)
                {
                    if (UIManager.Instance != null) UIManager.Instance.UpdateHazardTimer(i.ToString());
                    yield return new WaitForSeconds(1f);
                }
                
                // Clear timer display
                if (UIManager.Instance != null) UIManager.Instance.UpdateHazardTimer("");
                
                // Kill players on hazard sectors
                if (arena != null) arena.CheckAndKillPlayers();
                
                // Brief pause before next round
                yield return new WaitForSeconds(1f);
                
                // Reset sectors
                if (arena != null) arena.ResetAllSectors();
                
                yield return new WaitForSeconds(0.5f);
            }
            
            // Game complete
            EndGame();
            if (UIManager.Instance != null) UIManager.Instance.UpdateCountdown("FINISH!");
        }

        public void EndGame()
        {
            Debug.Log("MiniGame Ended!");
            IsGameActive = false;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        [SerializeField] private Text countdownText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (countdownText == null)
                countdownText = GetComponentInChildren<Text>();
        }

        [SerializeField] private Text hazardTimerText;

        public void UpdateHazardTimer(string text)
        {
            if (hazardTimerText != null)
                hazardTimerText.text = text;
        }

        [SerializeField] private Text roundText;

        public void UpdateRound(int round, int totalRounds)
        {
            if (roundText != null)
                roundText.text = $"Round {round}/{totalRounds}";
        }



        public void UpdateCountdown(string text)
        {
            if (countdownText != null)
                countdownText.text = text;
        }
    }
}
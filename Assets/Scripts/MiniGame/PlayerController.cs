using UnityEngine;

namespace MiniGame
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private void Update()
        {
            if (MiniGameManager.Instance != null && !MiniGameManager.Instance.IsGameActive) return;

            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
            transform.Translate(movement, Space.World);
        }

        public void Eliminate()
        {
            moveSpeed = 0;
            gameObject.SetActive(false); // Play death effect here later
        }

    }
}
using UnityEngine;

namespace MiniGame
{
    public class SectorController : MonoBehaviour
    {
        [SerializeField] private MeshRenderer sectorMesh;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material hazardMaterial;
        [SerializeField] private Color hazardColor = Color.red; // For pulsing effect
        
        private bool isHazard = false;
        
        private void Awake()
        {
            if (sectorMesh == null)
                sectorMesh = GetComponent<MeshRenderer>();
                
            // Load materials if not assigned
            if (normalMaterial == null)
                normalMaterial = UnityEngine.Resources.Load<Material>("Materials/SectorNormal");
            if (hazardMaterial == null)
                hazardMaterial = UnityEngine.Resources.Load<Material>("Materials/SectorHazard");
        }

        private void Update()
        {
            if (isHazard && sectorMesh != null)
            {
                // Pulse effect between Red and Dark Red
                float lerp = Mathf.PingPong(Time.time * 5f, 1f);
                Color pulsedColor = Color.Lerp(hazardColor, hazardColor * 0.5f, lerp);
                sectorMesh.material.color = pulsedColor;
            }
        }
        
        public void SetNormal()
        {
            isHazard = false;
            if (sectorMesh != null && normalMaterial != null)
                sectorMesh.material = normalMaterial;
        }
        
        public void SetHazard()
        {
            isHazard = true;
            if (sectorMesh != null && hazardMaterial != null)
                sectorMesh.material = hazardMaterial;
        }
        
        public bool IsHazard => isHazard;
        
        public bool ContainsPlayer(Transform player)
        {
            // Check if player is within this sector's angle range
            Vector3 toPlayer = player.position - transform.parent.position;
            toPlayer.y = 0;
            
            float distance = toPlayer.magnitude;
            if (distance > 5f) return false; // Outside arena
            
            // Get angle of player relative to arena center
            float playerAngle = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;
            if (playerAngle < 0) playerAngle += 360f;
            
            // Get this sector's angle range
            float sectorAngle = transform.eulerAngles.y;
            float halfAngle = 30f; // 60 degree sectors (6 sectors total)
            
            float minAngle = sectorAngle - halfAngle;
            float maxAngle = sectorAngle + halfAngle;
            
            // Handle wrap-around
            if (minAngle < 0)
            {
                return playerAngle >= (360 + minAngle) || playerAngle <= maxAngle;
            }
            if (maxAngle > 360)
            {
                return playerAngle >= minAngle || playerAngle <= (maxAngle - 360);
            }
            
            return playerAngle >= minAngle && playerAngle <= maxAngle;
        }
    }
}
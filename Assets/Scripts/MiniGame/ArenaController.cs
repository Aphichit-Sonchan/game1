using UnityEngine;
using System.Collections.Generic;

namespace MiniGame
{
    public class ArenaController : MonoBehaviour
    {
        private SectorController[] sectors;
        
        private void Start()
        {
            InitializeArena();
        }

        public void InitializeArena()
        {
            // Find all sectors in children
            sectors = GetComponentsInChildren<SectorController>();
            Debug.Log($"Arena Initialized with {sectors.Length} sectors");
            
            // Set all to normal initially
            ResetAllSectors();
        }
        
        public void ResetAllSectors()
        {
            foreach (var sector in sectors)
            {
                sector.SetNormal();
            }
        }
        
        public void SetRandomHazards(int count = 2)
        {
            ResetAllSectors();
            
            if (sectors == null || sectors.Length == 0) return;
            
            // Randomly select 'count' sectors to be hazards
            List<int> indices = new List<int>();
            for (int i = 0; i < sectors.Length; i++) indices.Add(i);
            
            for (int i = 0; i < Mathf.Min(count, sectors.Length); i++)
            {
                int randomIndex = Random.Range(0, indices.Count);
                int sectorIndex = indices[randomIndex];
                indices.RemoveAt(randomIndex);
                
                sectors[sectorIndex].SetHazard();
            }
        }
        
        public void CheckAndKillPlayers()
        {
            PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            
            foreach (var player in players)
            {
                foreach (var sector in sectors)
                {
                    if (sector.IsHazard && sector.ContainsPlayer(player.transform))
                    {
                        player.Eliminate();
                        Debug.Log($"Player {player.name} eliminated by sector hazard!");
                        break;
                    }
                }
            }
        }
    }
}
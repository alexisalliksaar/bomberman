using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public static class GameController
    {
        public static List<BombDropper> Players = new List<BombDropper>();
        public static List<MonoBehaviour> ControllerScripts = new List<MonoBehaviour>();

        public static void Reset()
        {
            Players = new List<BombDropper>();
            ControllerScripts = new List<MonoBehaviour>();
        }
        
        public static void Remove(BombDropper bombDropper)
        {
            Players.Remove(bombDropper);
            if (Players.Count == 1)
            {
                PlayerController pc = Players[0].gameObject.GetComponent<PlayerController>();
                if (pc != null)
                {
                    UIController.Instance.EndGame(true);
                }
            }
        }
        public static void DestroyScripts()
        {
            foreach (MonoBehaviour controllerScript in ControllerScripts)
            {
                if (controllerScript != null)
                    Object.Destroy(controllerScript);
            }
        }
    }
    
}
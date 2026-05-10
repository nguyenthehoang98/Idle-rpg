using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleLevel : MonoBehaviour
    {
        [SerializeField] private LevelCone prefab;

        private List<LevelCone> cones = new List<LevelCone>();
        
        private void Start()
        {
            cones.Add(prefab);
            for (int i = 0; i < 5; i++)
            {
                cones.Add(Instantiate(prefab, prefab.transform.parent));
            }

            for (int i = 0; i < cones.Count; i++)
            {
                cones[i].Init(i + 1);
            }
        }
    }
}

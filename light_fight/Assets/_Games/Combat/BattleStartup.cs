using _Games.Combat.EntityComponentSystem;
using _Games.Combat.Model;
using Unity.Entities;
using UnityEngine;

namespace _Games.Combat
{
    public class BattleStartup : MonoBehaviour
    {
        private ShareData shareData;
        private void Start()
        {
            Entity player = ECSFactory.BuildPlayer();
            shareData = new ShareData(player);
        }
    }
}

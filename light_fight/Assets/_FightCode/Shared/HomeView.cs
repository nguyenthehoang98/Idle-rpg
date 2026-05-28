using _FightCode.Utils;
using _KITSystem.Utils;
using UnityEngine;

namespace _FightCode.Shared
{
    public class HomeView : MonoBehaviour
    {
        private void OnEnable()
        {
            Fight();
        }

        public void Fight()
        {
            EntryScene.Instance.LoadScene(Const.BATTLE_SCENE);
        }
    }
}
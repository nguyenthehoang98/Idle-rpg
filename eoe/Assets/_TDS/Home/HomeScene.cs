using _KITSystem.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Game.Home
{
    public class HomeScene : MonoBehaviour
    {
        void Start()
        {
            KitEntryScene.Instance.HideLoadingScene();
            
            SceneManager.LoadScene("GamePlayScene");
        }
    }
}
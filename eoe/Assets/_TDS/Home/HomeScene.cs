using _KITSystem.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _TDS.Home
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
using _KIT.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeScene : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("GamePlayScene");
        KitEntryScene.Instance.HideLoadingScene();
    }
}

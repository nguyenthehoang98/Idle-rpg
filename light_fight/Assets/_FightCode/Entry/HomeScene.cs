using _KITSystem.Utils;
using UnityEngine;

namespace _FightCode.Entry
{
    public class HomeScene : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private HomeView homeViewPrefab;
        
        void Start()
        {
            Instantiate(homeViewPrefab, canvas.transform);
            
            EntryScene.Instance.HideLoadingScene();
        }
    }
}

using _KIT.Popup;
using _KIT.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace _Games.Combat.View
{
    public class LosePopup : PopupBase
    {
        [SerializeField] private Button buttonLose;

        private void Awake()
        {
            buttonLose.onClick.AddListener(() =>
            {
                Close();
                KitEntryScene.Instance.ShowLoadingScene();
                KitEntryScene.Instance.LoadScene("GameplayScene");
            });
        }
    }
}
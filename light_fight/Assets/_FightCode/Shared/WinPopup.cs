using _FightCode.Utils;
using _KITSystem.Popup;
using _KITSystem.Utils;

namespace _FightCode.Shared
{
    public class WinPopup : PopupBase
    {
        private void OnEnable()
        {
            Home();
        }

        public void Again()
        {
            EntryScene.Instance.LoadScene(Const.BATTLE_SCENE);
        }

        public void Home()
        {
            EntryScene.Instance.LoadScene(Const.HOME_SCENE);
        }
    }
}

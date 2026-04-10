using System;
using _KIT.Utils;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace _KIT.Popup
{
    public abstract class PopupBase : MonoBehaviour
    {
        [SerializeField] protected MMF_Player openFeedback;
        [SerializeField] protected MMF_Player closeFeedback;
        [SerializeField] protected GameObject blockInput;

        public Action OpenedCallback;
        public Action ClosedCallback;
        public int SceneId { get; set; }
        public bool IsDestroyOnLoadScene { get; set; }

        public void Open()
        {
            blockInput.SetActive(true);
            OnOpen();
            openFeedback.PlayFeedbacks();
            this.WaitInvoke(openFeedback.TotalDuration, () =>
            {
                OnOpened();
                OpenedCallback?.Invoke();
                OpenedCallback = null;
                blockInput.SetActive(false);
            });
        }

        public void Close()
        {
            blockInput.SetActive(true);
            OnClose();
            closeFeedback.PlayFeedbacks();
            this.WaitInvoke(closeFeedback.TotalDuration, () =>
            {
                OnClosed();
                ClosedCallback?.Invoke();
                ClosedCallback = null;
                blockInput.SetActive(false);
            });
        }

        protected virtual void OnClose()
        {
        }

        protected virtual void OnClosed()
        {
        }

        protected virtual void OnOpen()
        {
        }

        protected virtual void OnOpened()
        {
        }
    }
}
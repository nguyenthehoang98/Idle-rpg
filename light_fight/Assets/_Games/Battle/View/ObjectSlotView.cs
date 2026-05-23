using _KITSystem.Utils;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle.View
{
    public class ObjectSlotView : MonoBehaviour, ISlotView
    {
        [TitleGroup("Settings")] 
        [SerializeField] private Color[] selectedColors = new Color[4];
        [TitleGroup("Elements")] 
        [SerializeField] private SpriteRenderer highlight;
        [SerializeField] private Transform pivot;
        [SerializeField] private StarView[] stars;
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private MMF_Player playFeedback;
        [SerializeField] private MMF_Player zoomOutFeedback;
        [SerializeField] private MMF_Player zoomInFeedback;
        [TitleGroup("Debug")]
        [SerializeField] private float shineWidth;
        [SerializeField] private Color shineColor;
        
        private MaterialPropertyBlock colorProperty;
        private MaterialPropertyBlock widthProperty;
        private int currentStack;

        private void Awake()
        {
            pivot.gameObject.SetActive(false);
            colorProperty = new MaterialPropertyBlock();
            widthProperty = new MaterialPropertyBlock();
        }

        public ISlotView Instantiate(Transform parent, Vector3 localEulerAngles)
        {
            var view = Instantiate(transform, parent);
            view.localEulerAngles = localEulerAngles;
            return view.GetComponent<ISlotView>();
        }

        public float Initialize(float timeScale)
        {
            initFeedback.TimescaleMultiplier = timeScale;
            initFeedback.PlayFeedbacks();
            return initFeedback.TotalDuration / timeScale;
        }

        public float Play(float timeScale)
        {
            float d = 0;
            for (int i = 0; i < stars.Length; i++)
            {
                int index = i;
                d = Mathf.Max(d, index * 0.1f / timeScale);
                this.WaitInvoke(d, () => stars[index].Play(timeScale));
            }
            
            playFeedback.TimescaleMultiplier = timeScale;

            //float d1 = d + 0.5f / timeScale;
            float d2 = d + 0.5f / timeScale;
            //this.WaitInvoke(d1, () => { Debug.Log("weapon_active"); });
            this.WaitInvoke(d2, playFeedback.PlayFeedbacks);

            return d2 + playFeedback.TotalDuration / timeScale;
        }

        public float Activate(float timeScale)
        {
            zoomInFeedback.TimescaleMultiplier = timeScale;
            zoomInFeedback.PlayFeedbacks();
            return zoomInFeedback.TotalDuration / timeScale;
        }

        public float Deactivate(float timeScale)
        {
            zoomOutFeedback.TimescaleMultiplier = timeScale;
            zoomOutFeedback.PlayFeedbacks();
            return zoomOutFeedback.TotalDuration / timeScale;
        }

        public void Stack(int stack)
        {
            /*int prevStack = currentStack;
            
            currentStack = stack;
            stars[stack - 1].Active();*/
        }
        
        private void SetColor(Color color)
        {
            shineColor = color;
            highlight.GetPropertyBlock(colorProperty);
            colorProperty.SetColor("_ShineColor", color);
            highlight.SetPropertyBlock(colorProperty);
        }

        private void SetWidth(float width)
        {
            shineWidth = width;
            highlight.GetPropertyBlock(widthProperty);
            widthProperty.SetFloat("_ShineWidth", width);
            highlight.SetPropertyBlock(widthProperty);
        }
    }
}
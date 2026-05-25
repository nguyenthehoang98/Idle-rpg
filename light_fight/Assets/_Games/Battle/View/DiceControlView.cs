using System;
using System.Collections;
using System.Collections.Generic;
using _Games.Battle.Model;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Games.Battle.View
{
    public class DiceControlView : MonoBehaviour
    {
        [TitleGroup("Fills", "Settings")]
        [SerializeField, Range(0.01f, 0.2f)] private float deltaMaxValue = 0.2f;
        [SerializeField, Range(1f, 5f)] private float speed = 2f;
        [SerializeField, Range(0.01f, 5.0f)] private float offsetUpX = 0.15f;
        [SerializeField, Range(0.01f, 5.0f)] private float offsetDownX = 0.3f;
        [TitleGroup("Fills ", "Right")]
        [SerializeField] private Slider rightSlider;
        [SerializeField] private UICornersGradient rightFill;
        [TitleGroup("Fills  ", "Left")]
        [SerializeField] private Slider leftSlider;
        [SerializeField] private UICornersGradient leftFill;
        [TitleGroup("Elements")]
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private RectTransform slotGroupRect;
        [SerializeField] private HorizontalLayoutGroup itemGroup;
        [SerializeField] private TextMeshProUGUI txtProgress;
        [SerializeField] private Image[] imgFills;

        public event Action OnInitialized;
        public event Action<float> OnSpeedChanged;

        private BattleSetting setting;
        private BattleShare share;
        
        private Queue<Action> queue = new Queue<Action>();
        private float leftValue;
        private float rightValue;
        
        [SerializeField, DisableIf("@true")] private float elapsedTime;
        [SerializeField, DisableIf("@true")] private float progress;
        [SerializeField, DisableIf("@true")] private Color color;
        private bool isInitialized;

        private void Awake()
        {
            int countChildren = transform.childCount;
            for (int i = 0; i < countChildren; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isInitialized) return;
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha1)) OnLeftPointerDown();
            else if (Input.GetKeyUp(KeyCode.Alpha1)) OnLeftPointerUp();

            if (Input.GetKeyDown(KeyCode.Alpha2)) OnRightPointerDown();
            else if (Input.GetKeyUp(KeyCode.Alpha2)) OnRightPointerUp();
#endif

            if (leftValue + rightValue > 0)
            {
                float deltaTime = Time.deltaTime * speed * (rightValue - leftValue);
                bool shouldUpdateFillProgress = true;
                bool shouldUpdateTextProgress = Mathf.Abs(elapsedTime) < 1;

                float prev = elapsedTime;
                elapsedTime += deltaTime;
                elapsedTime = Mathf.Clamp(elapsedTime, -1, 1);
                float last = elapsedTime;

                if (deltaTime > 0 && prev <= 0 && last > 0)
                {
                    rightFill.gameObject.SetActive(true);
                    leftFill.gameObject.SetActive(false);
                }

                if (deltaTime < 0 && prev >= 0 && last < 0)
                {
                    rightFill.gameObject.SetActive(false);
                    leftFill.gameObject.SetActive(true);
                }

                if (shouldUpdateTextProgress)
                {
                    queue.Enqueue(() =>
                    {
                        if (elapsedTime > 0)
                        {
                            rightSlider.value = elapsedTime;
                            leftSlider.value = 0;
                        }
                        else
                        {
                            leftSlider.value = -elapsedTime;
                            rightSlider.value = 0;
                        }
                    });
                }

                if (shouldUpdateFillProgress)
                {
                    queue.Enqueue(UpdateProgress);
                }

                if (Mathf.Abs(elapsedTime) >= 1)
                {
                    if (elapsedTime >= 1) elapsedTime -= deltaMaxValue;
                    if (elapsedTime <= -1) elapsedTime += deltaMaxValue;
                }
            }
            else
            {
                float deltaTime = Time.deltaTime * speed;
                bool shouldUpdate = Mathf.Abs(elapsedTime) > 0;
                if(elapsedTime > 0)
                {
                    elapsedTime -= deltaTime;
                    elapsedTime = Mathf.Clamp(elapsedTime, 0, 1);
                }
                else if (elapsedTime < 0)
                {
                    elapsedTime += deltaTime;
                    elapsedTime = Mathf.Clamp(elapsedTime, -1, 0);
                }

                bool shouldStop = shouldUpdate && Mathf.Abs(elapsedTime) == 0;

                if (shouldUpdate)
                {
                    queue.Enqueue(() =>
                    {
                        if (elapsedTime > 0)
                        {
                            rightSlider.value = elapsedTime;
                            leftSlider.value = 0;
                        }
                        else
                        {
                            leftSlider.value = -elapsedTime;
                            rightSlider.value = 0;
                        }

                        UpdateProgress();
                    });
                }

                if (shouldStop)
                {
                    rightFill.gameObject.SetActive(false);
                    leftFill.gameObject.SetActive(false);
                }
            }
        }

        private void LateUpdate()
        {
            while (queue.Count > 0)
            {
                queue.Dequeue().Invoke();
            }
        }
        
        public void Initialize(BattleShare share, BattleSetting setting)
        {
            this.setting = setting;
            this.share = share;
            
            Color leftColor = setting.slotColorMinSpeed;
            leftSlider.fillRect.GetComponent<Image>().color = leftColor;
            leftFill.m_topRightColor = new Color(leftColor.r, leftColor.g, leftColor.b, 1f);
            leftFill.m_bottomRightColor = new Color(leftColor.r, leftColor.g, leftColor.b, 1f);
            leftFill.m_topLeftColor = new Color(leftColor.r, leftColor.g, leftColor.b, 0.1f);
            leftFill.m_bottomLeftColor = new Color(leftColor.r, leftColor.g, leftColor.b, 0.1f);
            leftFill.gameObject.SetActive(false);
            
            Color rightColor = setting.slotColorMaxSpeed;
            rightSlider.fillRect.GetComponent<Image>().color = rightColor;
            rightFill.m_topLeftColor = new Color(rightColor.r, rightColor.g, rightColor.b, 1f);
            rightFill.m_bottomLeftColor = new Color(rightColor.r, rightColor.g, rightColor.b, 1f);
            rightFill.m_topRightColor = new Color(rightColor.r, rightColor.g, rightColor.b, 0.1f);
            rightFill.m_bottomRightColor = new Color(rightColor.r, rightColor.g, rightColor.b, 0.1f);
            rightFill.gameObject.SetActive(false);

            UpdateProgress();
            StartCoroutine(Build(share, setting));
        }

        private IEnumerator Build(BattleShare share, BattleSetting setting)
        {
            int countChildren = transform.childCount;
            for (int i = 0; i < countChildren; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            
            for (int i = 0; i < setting.totalSlot; i++)
            {
                share.dices[i].Initialize(itemGroup.transform);
            }
            
            yield return null;

            float sizeX = setting.dice.RectTransformSize.x;
            float space = itemGroup.spacing;
            float width = space * (setting.totalSlot + 1) + sizeX * setting.totalSlot;
            slotGroupRect.sizeDelta = new Vector2(width, slotGroupRect.sizeDelta.y);

            yield return new WaitForSeconds(0.2f / share.timeScale);
            
            for (int i = 0; i < countChildren; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            
            initFeedback.TimescaleMultiplier = share.timeScale;
            initFeedback.PlayFeedbacks();

            yield return new WaitForSeconds(initFeedback.TotalDuration / share.timeScale);
            
            OnInitialized?.Invoke();
            
            isInitialized = true;
        }

        public void OnLeftPointerDown() => leftValue = 1;

        public void OnLeftPointerUp() => leftValue = 0;

        public void OnRightPointerDown() => rightValue = 1;

        public void OnRightPointerUp() => rightValue = 0;
        
        private void UpdateProgress()
        {
            Vector3 offset = Vector3.zero;
            if (elapsedTime > 0)
                offset.x = offsetUpX;
            else if (elapsedTime < 0)
                offset.x = -offsetDownX;
            
            rightFill.transform.position = rightSlider.handleRect.transform.position + offset;
            leftFill.transform.position = leftSlider.handleRect.transform.position + offset;

            color = setting.slotColorDefaultSpeed;
            progress = 100;
            
            if (elapsedTime > 0)
            {
                color = Color.Lerp(color, setting.slotColorMaxSpeed, elapsedTime);
                progress = Mathf.Lerp(progress, setting.slotMaxOffsetSpeed, elapsedTime);
            }
            else if (elapsedTime < 0)
            {
                color = Color.Lerp(color, setting.slotColorMinSpeed, -elapsedTime);
                progress = Mathf.Lerp(setting.slotMinOffsetSpeed, progress, -elapsedTime);
            }

            foreach (var img in imgFills) img.color = color;
            txtProgress.text = string.Format("{0}%", (int)progress);
            
            foreach (var dice in share.dices)
            {
                dice.SetColor(color);
                //dice.SetScaleTime(v);
            }
        }
    }
}

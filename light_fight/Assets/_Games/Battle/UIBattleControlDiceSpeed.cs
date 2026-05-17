using System;
using System.Collections;
using System.Collections.Generic;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Games.Battle
{
    public class UIBattleControlDiceSpeed : MonoBehaviour
    {
        [TitleGroup("Feedback")]
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private GameObject[] activeGameObjects;
        
        [TitleGroup("Fills")]
        [SerializeField] private Image[] imgColors;
        [SerializeField] private TextMeshProUGUI textProgress;
        [SerializeField] private Color upColor = new Color(0, 1, 0);
        [SerializeField] private Color centerColor = new Color(1, 1, 1);
        [SerializeField] private Color downColor = new Color(1, 0, 0);
        [SerializeField, Range(0.01f, 0.2f)] private float deltaMaxValue = 0.2f;
        [SerializeField, Range(1f, 5f)] private float speed = 2f;
        [SerializeField, Range(0.01f, 5.0f)] private float offsetUpX = 0.2f;
        [SerializeField, Range(0.01f, 5.0f)] private float offsetDownX = 0.2f;
        [SerializeField] private Slider upSlider;
        [SerializeField] private UICornersGradient upFill2;
        [SerializeField] private Slider downSlider;
        [SerializeField] private UICornersGradient downFill2;
        [SerializeField] private AnimationCurve upProgressCurve;
        [SerializeField] private AnimationCurve downProgressCurve;

        [TitleGroup("Controls")]
        [SerializeField] private Button buttonLeft;
        [SerializeField] private Button buttonRight;
        [SerializeField] private RectTransform slotGroupRect;
        [SerializeField] private HorizontalLayoutGroup itemGroup;
        [SerializeField] private UIBattleDiceSlot itemPrefab;
        
        private const int MAX_VALUE = 50;
        private float elapsedTime;
        private int progress;
        private int totalDice;
        private int totalDiceUnlock;
        private bool unlockAll;
        private float leftValue;
        private float rightValue;
        private bool isInitialized;
        private List<int> stacks = new List<int>();
        private List<UIBattleDiceSlot> dices = new List<UIBattleDiceSlot>();
        private readonly Queue<Action> queue = new Queue<Action>();

        public event Action<List<int>> OnTrigger; 

        public void PrefabBuilder(int dice, bool unlockAll)
        {
            this.unlockAll = unlockAll;
            totalDice = dice;
            
            foreach (var go in activeGameObjects)
            {
                go.SetActive(false);
            }
            
            downSlider.fillRect.GetComponent<Image>().color = downColor;
            downFill2.m_topRightColor = new Color(downColor.r, downColor.g, downColor.b, 1f);
            downFill2.m_bottomRightColor = new Color(downColor.r, downColor.g, downColor.b, 1f);
            downFill2.m_topLeftColor = new Color(downColor.r, downColor.g, downColor.b, 0.1f);
            downFill2.m_bottomLeftColor = new Color(downColor.r, downColor.g, downColor.b, 0.1f);
            downFill2.gameObject.SetActive(false);
            
            upSlider.fillRect.GetComponent<Image>().color = upColor;
            upFill2.m_topLeftColor = new Color(upColor.r, upColor.g, upColor.b, 1f);
            upFill2.m_bottomLeftColor = new Color(upColor.r, upColor.g, upColor.b, 1f);
            upFill2.m_topRightColor = new Color(upColor.r, upColor.g, upColor.b, 0.1f);
            upFill2.m_bottomRightColor = new Color(upColor.r, upColor.g, upColor.b, 0.1f);
            upFill2.gameObject.SetActive(false);

            UpdateProgress();
            StartCoroutine(BuildLayout());
        }

        public UniTask Initialize()
        {
            for (int i = 0; i < dices.Count; i++)
            {
                bool locked = unlockAll ? false : i >= dices.Count - 1;
                totalDiceUnlock += locked ? 0 : 1;
                dices[i].Init(locked, StackTrigger);
                dices[i].SetColor(centerColor);
            }

            initFeedback.PlayFeedbacks();

            this.WaitNextFrame(() =>
            {
                foreach (var go in activeGameObjects)
                {
                    go.SetActive(true);
                }
            });
            
            return UniTask.WaitForSeconds(initFeedback.TotalDuration);
        }

        private void StackTrigger(int number)
        {
            stacks.Add(number);
           
            if (stacks.Count == totalDiceUnlock)
            {
                OnTrigger?.Invoke(stacks);
                stacks.Clear();
            }
        }

        public void Play()
        {
            isInitialized = true;

            foreach (var d in dices)
            {
                d.Play();
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
                float deltaTime = Time.deltaTime * speed * (-leftValue + rightValue);
                bool shouldUpdateFillProgress = false;
                bool shouldUpdateTextProgress = Mathf.Abs(elapsedTime) < 1;;
            
                float prev = elapsedTime;
                elapsedTime += deltaTime;
                elapsedTime = Mathf.Clamp(elapsedTime, -1, 1);
                float last = elapsedTime;
                
                if (deltaTime > 0 && prev <= 0 && last > 0)
                {
                    upFill2.gameObject.SetActive(true);
                    downFill2.gameObject.SetActive(false);
                }

                if (deltaTime < 0 && prev >= 0 && last < 0)
                {
                    upFill2.gameObject.SetActive(false);
                    downFill2.gameObject.SetActive(true);
                }
                
                if (elapsedTime > 0)
                {
                    int prevValue = progress;
                    int newValue = (int)(upProgressCurve.Evaluate(elapsedTime) * MAX_VALUE);
                    if (prevValue < newValue || leftValue > 0)
                    {
                        progress = newValue;
                        shouldUpdateFillProgress = true;
                    }
                }
                else if (elapsedTime < 0)
                {
                    int prevValue = progress;
                    int newValue = -(int)(downProgressCurve.Evaluate(Mathf.Abs(elapsedTime)) * MAX_VALUE);
                    if (prevValue > newValue || rightValue > 0)
                    {
                        progress = newValue;
                        shouldUpdateFillProgress = true;
                    }
                }

                if (shouldUpdateTextProgress)
                {
                    queue.Enqueue(() =>
                    {
                        if (elapsedTime > 0)
                        {
                            upSlider.value = elapsedTime;
                            downSlider.value = 0;
                        }
                        else
                        {
                            downSlider.value = Mathf.Abs(elapsedTime);
                            upSlider.value = 0;
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
                
                if (elapsedTime > 0)
                {
                    progress = (int)(upProgressCurve.Evaluate(elapsedTime) * MAX_VALUE);
                }
                else if (elapsedTime < 0)
                {
                    progress = -(int)(downProgressCurve.Evaluate(Mathf.Abs(elapsedTime)) * MAX_VALUE);
                }

                if (shouldUpdate)
                {
                    queue.Enqueue(() =>
                    {
                        if (elapsedTime > 0)
                        {
                            upSlider.value = elapsedTime;
                            downSlider.value = 0;
                        }
                        else
                        {
                            downSlider.value = Mathf.Abs(elapsedTime);
                            upSlider.value = 0;
                        }

                        UpdateProgress();
                    });
                }

                if (shouldStop)
                {
                    upFill2.gameObject.SetActive(false);
                    downFill2.gameObject.SetActive(false);
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

        public void OnLeftPointerDown() => leftValue = 1;

        public void OnLeftPointerUp() => leftValue = 0;

        public void OnRightPointerDown() => rightValue = 1;

        public void OnRightPointerUp() => rightValue = 0;

        private void UpdateProgress()
        {
            Vector3 offset = Vector3.zero;
            if (elapsedTime > 0) offset.x = offsetUpX;
            else if (elapsedTime < 0) offset.x = -offsetDownX;
            upFill2.transform.position = upSlider.handleRect.transform.position + offset;
            downFill2.transform.position = downSlider.handleRect.transform.position + offset;

            textProgress.text = (progress + 100) + "%";

            Color color;
            if (elapsedTime > 0)
                color = Color.Lerp(centerColor, upColor, Mathf.Abs(elapsedTime));
            else if (elapsedTime < 0)
                color = Color.Lerp(centerColor, downColor, Mathf.Abs(elapsedTime));
            else color = centerColor;

            foreach (var img in imgColors)
            {
                img.color = color;
            }

            float v = progress * 0.01f;
            foreach (var dice in dices)
            {
                dice.SetColor(color);
                dice.SetScaleTime(v);
            }
        }

        private IEnumerator BuildLayout()
        {
            for (int i = 0; i < totalDice; i++)
            {
                var instance = Instantiate(itemPrefab, itemGroup.transform);
                instance.transform.SetAsLastSibling();
                dices.Add(instance);
            }

            yield return null;

            float sizeX = itemPrefab.RectTransform.sizeDelta.x;
            float space = itemGroup.spacing;
            float width = space * (totalDice + 1) + sizeX * totalDice;
            slotGroupRect.sizeDelta = new Vector2(width, slotGroupRect.sizeDelta.y);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Games.Battle
{
    public class UIBattleControlArena : MonoBehaviour
    {
        [TitleGroup("Events")] 
        [SerializeField] private UnityEvent<Vector2Int> onTrigger;
        
        [TitleGroup("Fills")]
        ////
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
        ////
        [SerializeField] private Button buttonLeft;
        [SerializeField] private Button buttonRight;
        [SerializeField] private RectTransform slotGroupRect;
        [SerializeField] private HorizontalLayoutGroup itemGroup;
        [SerializeField] private UIBattleDiceSlot itemPrefab;
        [SerializeField] private int totalItems = 2;

        private const int MAX_VALUE = 50;
        private float elapsedTime;
        private int value;
        private float leftValue;
        private float rightValue;
        private List<UIBattleDiceSlot> dices = new List<UIBattleDiceSlot>();
        private readonly Queue<Action> queue = new Queue<Action>();

        private void Awake()
        {
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
            
            StartCoroutine(BuildLayout());
        }

        private void Start()
        {
            for (int i = 0; i < dices.Count; i++)
            {
                dices[i].Init(i, i >= dices.Count - 1, (a, b) =>
                {
                    if(onTrigger != null) onTrigger.Invoke(new Vector2Int(a, b));
                });
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha1)) OnLeftPointerDown();
            else if (Input.GetKeyUp(KeyCode.Alpha1)) OnLeftPointerUp();
            
            if (Input.GetKeyDown(KeyCode.Alpha2)) OnRightPointerDown();
            else if (Input.GetKeyUp(KeyCode.Alpha2)) OnRightPointerUp();
#endif
            
            if (leftValue + rightValue > 0)
            {
                float deltaTime = Time.deltaTime * speed * (-leftValue + rightValue);
                bool shouldUpdateTextProgress = false;
                bool shouldUpdateFillProgress = Mathf.Abs(elapsedTime) < 1;;
            
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
                    int prevValue = value;
                    int newValue = (int)(upProgressCurve.Evaluate(elapsedTime) * MAX_VALUE);
                    if (prevValue < newValue || leftValue > 0)
                    {
                        value = newValue;
                        shouldUpdateTextProgress = true;
                    }
                }
                else if (elapsedTime < 0)
                {
                    int prevValue = value;
                    int newValue = -(int)(downProgressCurve.Evaluate(Mathf.Abs(elapsedTime)) * MAX_VALUE);
                    if (prevValue > newValue || rightValue > 0)
                    {
                        value = newValue;
                        shouldUpdateTextProgress = true;
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
                    value = (int)(upProgressCurve.Evaluate(elapsedTime) * MAX_VALUE);
                }
                else if (elapsedTime < 0)
                {
                    value = -(int)(downProgressCurve.Evaluate(Mathf.Abs(elapsedTime)) * MAX_VALUE);
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

            textProgress.text = value + "%";

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

            float v = value * 0.01f;
            foreach (var dice in dices)
            {
                dice.SetColor(color);
                dice.SetValue(v);
            }
        }

        private IEnumerator BuildLayout()
        {
            dices.Add(itemPrefab);
            for (int i = 1; i < totalItems; i++)
            {
                var instance = Instantiate(itemPrefab, itemGroup.transform);
                instance.transform.SetAsLastSibling();
                dices.Add(instance);
            }

            yield return null;

            float sizeX = itemPrefab.RectTransform.sizeDelta.x;
            float space = itemGroup.spacing;
            float width = space * (totalItems + 1) + sizeX * totalItems;
            slotGroupRect.sizeDelta = new Vector2(width, slotGroupRect.sizeDelta.y);
        }
    }
}
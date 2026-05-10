using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Games.Battle
{
    public class UIBattleControlArena : MonoBehaviour
    {
        [TitleGroup("Fills")]
        ////
        [SerializeField] private TextMeshProUGUI textProgress;
        [SerializeField] private Image imgProgress;
        [SerializeField] private Color upColor = new Color(0, 1, 0);
        [SerializeField] private Color centerColor = new Color(1, 1, 1);
        [SerializeField] private Color downColor = new Color(1, 0, 0);
        [SerializeField, Range(1f, 5f)] private float speed = 2f;
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
        [SerializeField] private RectTransform itemPrefab;
        [SerializeField] private int totalItems = 2;

        private const int MAX_VALUE = 50;
        private float elapsedTime;
        private int value;
        private float leftValue;
        private float rightValue;

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
                bool shouldUpdate = Mathf.Abs(elapsedTime) < 1;
            
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
                    value = (int)(upProgressCurve.Evaluate(elapsedTime) * MAX_VALUE);
                }
                else if (elapsedTime < 0)
                {
                    value = -(int)(downProgressCurve.Evaluate(Mathf.Abs(elapsedTime)) * MAX_VALUE);
                }

                if (shouldUpdate)
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
                }

                if (shouldStop)
                {
                    upFill2.gameObject.SetActive(false);
                    downFill2.gameObject.SetActive(false);
                }
            }
        }

        public void OnLeftPointerDown() => leftValue = 1;

        public void OnLeftPointerUp() => leftValue = 0;

        public void OnRightPointerDown() => rightValue = 1;

        public void OnRightPointerUp() => rightValue = 0;

        public int Value() => value;
        
        private void UpdateProgress()
        {
            upFill2.transform.position = upSlider.handleRect.transform.position;
            downFill2.transform.position = downSlider.handleRect.transform.position;

            textProgress.text = value + "%";

            Color color;
            if (elapsedTime > 0)
                color = Color.Lerp(centerColor, upColor, Mathf.Abs(elapsedTime));
            else if (elapsedTime < 0)
                color = Color.Lerp(centerColor, downColor, Mathf.Abs(elapsedTime));
            else color = centerColor;

            imgProgress.color = color;
        }

        private IEnumerator BuildLayout()
        {
            for (int i = 1; i < totalItems; i++)
            {
                Instantiate(itemPrefab, itemGroup.transform);
            }

            yield return null;

            float sizeX = itemPrefab.sizeDelta.x;
            float space = itemGroup.spacing;
            float width = space * (totalItems + 1) + sizeX * totalItems;
            slotGroupRect.sizeDelta = new Vector2(width, slotGroupRect.sizeDelta.y);
        }
    }
}
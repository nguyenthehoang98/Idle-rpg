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

        private const int MAX = 50;
        private const int MIN = -50;

        private float elapsedTime = 0;
        private int fill;
        private bool isPressing;
        private bool toLeft;

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
            if (isPressing)
            {
                bool shouldUpdate = elapsedTime < 1;
                elapsedTime += Time.deltaTime * speed;
                elapsedTime = Mathf.Clamp01(elapsedTime);
                if (toLeft)
                {
                    fill = (int)(upProgressCurve.Evaluate(elapsedTime) * MIN);
                    fill = Mathf.Clamp(fill, MIN, 0);
                }
                else
                {
                    fill = (int)(upProgressCurve.Evaluate(elapsedTime) * MAX);
                    fill = Mathf.Clamp(fill, 0, MAX);
                }

                if (shouldUpdate)
                {
                    if (toLeft) downSlider.value = elapsedTime;
                    else upSlider.value = elapsedTime;

                    UpdateProgress();
                }
            }
            else
            {
                bool shouldUpdate = elapsedTime > 0;
                elapsedTime -= Time.deltaTime * speed;
                elapsedTime = Mathf.Clamp01(elapsedTime);
                bool shouldInactiveGameObject = shouldUpdate && elapsedTime == 0;
                if (toLeft)
                {
                    fill = (int)(downProgressCurve.Evaluate(elapsedTime) * MIN);
                    fill = Mathf.Clamp(fill, MIN, 0);
                }
                else
                {
                    fill = (int)(downProgressCurve.Evaluate(elapsedTime) * MAX);
                    fill = Mathf.Clamp(fill, 0, MAX);
                }

                if (shouldUpdate)
                {
                    if (toLeft) downSlider.value = elapsedTime;
                    else upSlider.value = elapsedTime;

                    UpdateProgress();
                }

                if (shouldInactiveGameObject)
                {
                    if (toLeft) buttonRight.interactable = true;
                    else buttonLeft.interactable = true;
                    upFill2.gameObject.SetActive(false);
                    downFill2.gameObject.SetActive(false);
                }
            }
        }

        public void OnLeftPointerDown()
        {
            buttonRight.interactable = false;
            toLeft = true;
            isPressing = true;
            downFill2.gameObject.SetActive(true);
        }

        public void OnLeftPointerUp()
        {
            isPressing = false;
        }

        public void OnRightPointerDown()
        {
            buttonLeft.interactable = false;
            toLeft = false;
            isPressing = true;
            upFill2.gameObject.SetActive(true);
        }

        public void OnRightPointerUp()
        { 
            isPressing = false;
        }

        private void UpdateProgress()
        {
            upFill2.transform.position = upSlider.handleRect.transform.position;
            downFill2.transform.position = downSlider.handleRect.transform.position;
            
            textProgress.text = fill + "%";

            Color color;
            if (toLeft) color = Color.Lerp(centerColor, downColor, elapsedTime);
            else color = Color.Lerp(centerColor, upColor, elapsedTime);
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
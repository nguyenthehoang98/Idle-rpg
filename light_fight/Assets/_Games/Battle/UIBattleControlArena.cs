using System;
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
        [SerializeField] private Slider upSlider;
        [SerializeField] private Slider downSlider;
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
            StartCoroutine(BuildLayout());
        }

        private void Update()
        {
            if (isPressing)
            {
                bool shouldUpdate = elapsedTime < 1;
                elapsedTime += Time.deltaTime;
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
                elapsedTime -= Time.deltaTime;
                elapsedTime = Mathf.Clamp01(elapsedTime);
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
            }
        }

        public void OnLeftPointerDown()
        {
            buttonRight.interactable = false;
            toLeft = true;
            isPressing = true;
        }

        public void OnLeftPointerUp()
        {
            buttonRight.interactable = true;
            isPressing = false;
        }

        public void OnRightPointerDown()
        {
            buttonLeft.interactable = false;
            toLeft = false;
            isPressing = true;
        }

        public void OnRightPointerUp()
        { 
            buttonLeft.interactable = true;
            isPressing = false;
        }

        private void UpdateProgress()
        {
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
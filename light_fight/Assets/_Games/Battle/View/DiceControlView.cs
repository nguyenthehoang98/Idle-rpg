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
        [SerializeField] private Color centerColor = new Color(1, 1, 0);
        [SerializeField, Range(0.01f, 0.2f)] private float deltaMaxValue = 0.2f;
        [SerializeField, Range(1f, 5f)] private float speed = 2f;
        [SerializeField, Range(0.01f, 5.0f)] private float offsetUpX = 0.15f;
        [SerializeField, Range(0.01f, 5.0f)] private float offsetDownX = 0.3f;
        [TitleGroup("Fills ", "Right")]
        [SerializeField] private Color rightColor = new Color(0, 1, 0);
        [SerializeField] private Button buttonRight;
        [SerializeField] private Slider rightSlider;
        [SerializeField] private UICornersGradient rightFill;
        [SerializeField] private AnimationCurve rightProgressCurve;
        [TitleGroup("Fills  ", "Left")]
        [SerializeField] private Color leftColor = new Color(1, 0, 0);
        [SerializeField] private Button buttonLeft;
        [SerializeField] private Slider leftSlider;
        [SerializeField] private UICornersGradient leftFill;
        [SerializeField] private AnimationCurve leftProgressCurve;
        
        [TitleGroup("Elements")]
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private RectTransform slotGroupRect;
        [SerializeField] private HorizontalLayoutGroup itemGroup;
        [SerializeField] private TextMeshProUGUI txtProgress;
        [SerializeField] private Image[] imgFills;

        public event Action OnInitialized;

        private void Awake()
        {
            int countChildren = transform.childCount;
            for (int i = 0; i < countChildren; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public void Initialize(BattleShare share, BattleSetting setting)
        {
            leftSlider.fillRect.GetComponent<Image>().color = leftColor;
            leftFill.m_topRightColor = new Color(leftColor.r, leftColor.g, leftColor.b, 1f);
            leftFill.m_bottomRightColor = new Color(leftColor.r, leftColor.g, leftColor.b, 1f);
            leftFill.m_topLeftColor = new Color(leftColor.r, leftColor.g, leftColor.b, 0.1f);
            leftFill.m_bottomLeftColor = new Color(leftColor.r, leftColor.g, leftColor.b, 0.1f);
            leftFill.gameObject.SetActive(false);
            
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
            
            for (int i = 0; i < setting.totalDice; i++)
            {
                share.dices[i].Initialize(itemGroup.transform);
            }
            
            yield return null;

            float sizeX = setting.dice.RectTransformSize.x;
            float space = itemGroup.spacing;
            float width = space * (setting.totalDice + 1) + sizeX * setting.totalDice;
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
        }

        private void UpdateProgress()
        {
        }
    }
}

using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace _Games.Battle.View
{
    public class ObjectDiceView : MonoBehaviour, IDiceView
    {
        [SerializeField] private DiceRollController rig;
        [SerializeField] private Image imgCooldown;
        [SerializeField] private TextMeshProUGUI txtNumber;
        [SerializeField] private GameObject lockObject;
        [SerializeField] private GameObject unlockObject;

        public RectTransform RectTransform {get; private set;}

        private void Awake()
        {
            RectTransform = transform as RectTransform;
        }

        public IDiceView Instantiate()
        {
            return Instantiate(transform).GetComponent<IDiceView>();
        }

        public void Initialize(Transform parent)
        {
            transform.SetParent(parent);
            transform.SetAsLastSibling();
            transform.localScale = Vector3.one;
            SetLocked(false);
            SetValue(0);
        }

        public void SetLocked(bool locked)
        {
            lockObject.SetActive(locked);
            unlockObject.SetActive(!locked);
            imgCooldown.gameObject.SetActive(!locked);
        }

        public void SetValue(int value)
        {
            txtNumber.text = value > 0 ? value.ToString() : "?";
        }

        public void SetProgress(float progress)
        {
            imgCooldown.fillAmount = progress;
        }

        public void SetColor(Color color)
        {
            imgCooldown.color = color;
        }

        public float Roll(int value)
        {
            return rig.Roll(value, null);
        }

        public Vector3 WorldPosition => rig.transform.position;

        public float2 RectTransformSize => GetComponent<RectTransform>().sizeDelta;
    }
}
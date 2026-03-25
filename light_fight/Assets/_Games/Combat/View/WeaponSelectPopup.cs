using System;
using System.Collections.Generic;
using _Games.Combat.Event;
using _Games.Combat.Level;
using _Games.Config;
using _Games.Utils;
using _KIT.Config;
using _KIT.Event;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Games.Combat.View
{
    public partial class WeaponSelectPopup : MonoBehaviour
    {
        [SerializeField] private WeaponItemView itemViewPrefab;
        [SerializeField] private RectTransform content;
        [SerializeField] private Data[] weapons;
        
        [Header("Tween")]
        [SerializeField] private float yStartPosition;
        [SerializeField] private float closeDuration = 0.3f;
        [SerializeField] private UnityEvent onStartOpen;
        [SerializeField] private UnityEvent onStartClose;
        
        [Header("Button")]
        [SerializeField] private Button btnResume;
        
        private readonly List<WeaponData> allData = new List<WeaponData>();
        private Camera mainCamera;
        private Vector3 prevCameraPosition;
        private bool canClickButton;
        
        private void Awake()
        {
            mainCamera = Camera.main;
            btnResume.onClick.AddListener(() =>
            {
                if (canClickButton)
                {
                    canClickButton = false;
                    Close(() =>
                    {
                        EventBus.Instance.Publish(new WaveResumeEvent());                        
                    });
                }
            });
            Vector3 anchoredPosition = content.anchoredPosition3D;
            anchoredPosition.y = yStartPosition;
            content.anchoredPosition3D = anchoredPosition;
            content.gameObject.SetActive(false);
        }
        
        private void Start()
        {
            WeaponConfig weaponConfig = KitConfigManager.Get<WeaponConfig>();
            int[] weaponIds = new int[] { 2001, 2002, 2003, 2010 };
            foreach (var id in weaponIds)
            {
                weaponConfig.Find(id, out WeaponData weaponData);
                allData.Add(weaponData);
            }
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<WaveSelectWeaponEvent>(OnWaveSelectWeapon);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<WaveSelectWeaponEvent>(OnWaveSelectWeapon);
        }
        
        private void OnWaveSelectWeapon(WaveSelectWeaponEvent e)
        {
            // tính toán dữ liệu & fill vào data (weapons)
            PickWeapon();
            Open(e);
        }
        
        private void Open(WaveSelectWeaponEvent e)
        {
            float duration = e.Duration;
            prevCameraPosition = mainCamera.transform.position;
            onStartOpen?.Invoke();

            if (Mathf.Abs(duration) > 0)
            {
                content.gameObject.SetActive(true);

                TweenSettings setting = new TweenSettings(duration, Ease.OutSine);
                Tween.Position(mainCamera.transform, new (e.CameraPosition, setting));
                Tween.UIAnchoredPositionY(content, new(0, setting));
                Tween.Delay(duration, () => { canClickButton = true; });
            }
            else
            {
                content.gameObject.SetActive(true);
                
                mainCamera.transform.position = e.CameraPosition;
                content.anchoredPosition3D = Vector3.zero;
                canClickButton = true;
            }
        }

        private void Close(Action onClosed)
        {
            float duration = closeDuration;
            onStartClose?.Invoke();

            TweenSettings setting = new TweenSettings(duration, Ease.OutSine);
            Tween.Position(mainCamera.transform, new (prevCameraPosition, setting));
            Tween.UIAnchoredPositionY(content, new(yStartPosition, setting));
            Tween.Delay(duration, () =>
            {
                ReturnPool();
                onClosed?.Invoke(); 
                content.gameObject.SetActive(false);
            });
        }
        
        private void PickWeapon()
        {
            // Lấy weapon từ pool: vũ khí, máu, giáp ...
            // Random level dựa trên wave hiện tại và level hiện tại
            int length = Mathf.Min(weapons.Length, allData.Count);
            for (int i = 0; i < length; i++)
            {
                WeaponData weaponData = allData[i];
                int level = FormulaUtils.RandomEquipmentLevel(1, 1, 1);
                Data data = weapons[i];
                data.textPrice.SetText("X" + weaponData.Price(level));
                data.textTitle.SetText(weaponData.WeaponName);

                if (data.WeaponItemView == null)
                {
                    var instance = Instantiate(itemViewPrefab, data.container);
                    instance.transform.SetAsFirstSibling();
                    instance.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
                    data.WeaponItemView = instance;
                }
                
                data.WeaponItemView.Initialize(weaponData, level, () =>
                {
                    data.textPrice.SetText(String.Empty);
                    data.textTitle.SetText(String.Empty);
                });
            }
        }
        
        private void ReturnPool()
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                var eqm = weapons[i];
                if (eqm.WeaponItemView == null || eqm.WeaponItemView.transform.parent != eqm.container)
                {
                    weapons[i].WeaponItemView = null;
                }
            }
        }
    }
    
    public partial class WeaponSelectPopup
    {
        [Serializable]
        class Data
        {
            public Transform container;
            public TextMeshProUGUI textTitle;
            public TextMeshProUGUI textPrice;
            public WeaponItemView WeaponItemView { get; set; }
            public int Price { get; set; }
        }
    }
}
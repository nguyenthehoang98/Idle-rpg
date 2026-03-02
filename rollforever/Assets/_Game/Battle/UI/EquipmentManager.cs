using _Game.Battle.Ecs.Events;
using _KIT.Event;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Collections.Generic;
using _Game.Battle.Utils;
using _Game.Scripts.Configs;
using _Game.Scripts.Weapon;
using _KIT.Config;
using _KIT.Resource;
using _KIT.Utils;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Game.Battle.UI
{
    public class EquipmentManager : MonoBehaviour
    {
        [Serializable]
        public class Data
        {
            public Transform container;
            public TextMeshProUGUI textTitle;
            public TextMeshProUGUI textPrice;
         
            [HideInInspector] public EquipmentItem equipmentItem;
            [HideInInspector] public int price;
        }
        
        [SerializeField] private EquipmentItem equipmentItemPrefab;
        [SerializeField] private RectTransform content;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Data[] equipments;
        [Header("Tweens")]
        [SerializeField] private float yStartPosition;
        [SerializeField] private float cameraOrthographicSize = 17;
        [SerializeField] private float closeDuration = 0.3f;
        [SerializeField] private UnityEvent onStartPush;
        [SerializeField] private UnityEvent onStartPull;
        [Header("Buttons")]
        [SerializeField] private Button btnResume;
        private List<WeaponConfig.WeaponData> weapons = new List<WeaponConfig.WeaponData>();

        private Vector3 prevCameraPosition;
        private bool canClickButton;
        
        private void Awake()
        {
            btnResume.onClick.AddListener(() =>
            {
                if (canClickButton)
                {
                    canClickButton = false;
                    Pull(() =>
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
            foreach (var w in weaponConfig.AllKeys)
            {
                weaponConfig.Find(w, out var weaponData);
                weapons.Add(weaponData);
            }
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<ShowChooseEquipmentEvent>(OnOpenEquipmentSelection);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<ShowChooseEquipmentEvent>(OnOpenEquipmentSelection);
        }

        void OnOpenEquipmentSelection(ShowChooseEquipmentEvent e)
        {
            // tính toán dữ liệu & fill vào data (equipments)
            PickWeapon();
            Push(e);
        }

        public void Push(ShowChooseEquipmentEvent e)
        {
            onStartPush?.Invoke();
            Vector3 cameraPosition = mainCamera.transform.position;
            prevCameraPosition = cameraPosition;
            cameraPosition.x = e.CameraOffsetPosition.x;
            cameraPosition.y = e.CameraOffsetPosition.y;
            if (math.abs(e.Duration) > 0)
            {
                content.gameObject.SetActive(true);
                mainCamera.transform.DOMove(cameraPosition, e.Duration).SetEase(Ease.OutSine);
                mainCamera.DOOrthoSize(e.OrthoSize, e.Duration).SetEase(Ease.OutSine);
                content.DOAnchorPosY(0, e.Duration).SetEase(Ease.OutSine);
                this.WaitInvoke(e.Duration, () => { canClickButton = true; });
            }
            else
            {
                content.gameObject.SetActive(true);
                mainCamera.orthographicSize = e.OrthoSize;
                mainCamera.transform.position = cameraPosition;
                content.anchoredPosition3D = Vector3.zero;
                canClickButton = true;
            }
        }

        private void Pull(Action onClosed)
        {
            float duration = closeDuration;
            onStartPull?.Invoke();
            mainCamera.transform.DOMove(prevCameraPosition, duration).SetEase(Ease.OutSine);
            mainCamera.DOOrthoSize(cameraOrthographicSize, duration).SetEase(Ease.OutSine);
            content.DOAnchorPosY(yStartPosition, duration).SetEase(Ease.OutSine);
            this.WaitInvoke(duration, () =>
            {
                ReturnPool();
                content.gameObject.SetActive(false);
                onClosed();
            });
        }

        private async void PickWeapon()
        {
            // Lấy weapon từ pool: vũ khí, máu, giáp ...
            // Random level dựa trên wave hiện tại và level hiện tại
            List<WeaponConfig.WeaponData> list = new List<WeaponConfig.WeaponData>();
            list.AddRange(weapons);
            list.AddRange(weapons);
            list.AddRange(weapons);

            for (int i = 0; i < equipments.Length; i++)
            {
                WeaponConfig.WeaponData weaponData = list[i];
                WeaponSO so = await KitLoaded.LoadAsync<WeaponSO>(weaponData.WeaponId.ToString(), true);
                int equipmentLevel = FormulaUtils.RandomEquipmentLevel(1, 1, 1);
                Data data = equipments[i];
                data.textPrice.SetText("Price x" + weaponData.Price(equipmentLevel));
                data.textTitle.SetText("Name " + weaponData.Name);

                if (data.equipmentItem == null)
                {
                   EquipmentItem equipmentItem = Instantiate(equipmentItemPrefab, data.container);
                   equipmentItem.transform.SetAsFirstSibling();
                   equipmentItem.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
                   data.equipmentItem = equipmentItem;
                }

                data.equipmentItem.Init(so.WeaponIcon, weaponData, equipmentLevel, () =>
                {
                    data.textPrice.SetText(String.Empty);
                    data.textTitle.SetText(String.Empty);
                });
            }
        }

        private void ReturnPool()
        {
            for (int i = 0; i < equipments.Length; i++)
            {
                var eqm = equipments[i];
                if (eqm.equipmentItem == null || eqm.equipmentItem.transform.parent != eqm.container)
                {
                    equipments[i].equipmentItem = null;
                }
            }
        }
    }
}
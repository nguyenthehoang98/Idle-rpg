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
using UnityEngine.UI;

namespace _Game.Battle.UI
{
    public class EquipmentManager : MonoBehaviour
    {
        [Serializable]
        public class Data
        {
            public EquipmentItem equipmentItem;
            public TextMeshProUGUI textTitle;
            public TextMeshProUGUI textPrice;

            private int price;
        }
        
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Data[] equipments;
        [SerializeField] private RectTransform container;
        [SerializeField] private float yStartPosition;
        [SerializeField] private float cameraOrthographicSize = 17;
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
                    Close(() =>
                    {
                        EventBus.Instance.Publish(new WaveResumeEvent());                        
                    });
                }
            });
            Vector3 anchoredPosition = container.anchoredPosition3D;
            anchoredPosition.y = yStartPosition;
            container.anchoredPosition3D = anchoredPosition;
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
            EventBus.Instance.Subscribe<WaveShowChooseEquipmentEvent>(OnOpenEquipmentSelection);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<WaveShowChooseEquipmentEvent>(OnOpenEquipmentSelection);
        }

        void OnOpenEquipmentSelection(WaveShowChooseEquipmentEvent e)
        {
            // tính toán dữ liệu & fill vào data (equipments)
            PickWeapon();
            
            Vector3 cameraPosition = mainCamera.transform.position;
            prevCameraPosition = cameraPosition;
            cameraPosition.x = e.CameraOffsetPosition.x;
            cameraPosition.y = e.CameraOffsetPosition.y;
            if (math.abs(e.Duration) > 0)
            {
                container.gameObject.SetActive(true);
                mainCamera.transform.DOMove(cameraPosition, e.Duration).SetEase(Ease.OutSine);
                mainCamera.DOOrthoSize(e.OrthoSize, e.Duration).SetEase(Ease.OutSine);
                container.DOAnchorPosY(0, e.Duration).SetEase(Ease.OutSine);
                this.WaitInvoke(e.Duration, () => { canClickButton = true; });
            }
            else
            {
                mainCamera.orthographicSize = e.OrthoSize;
                mainCamera.transform.position = cameraPosition;
                container.anchoredPosition3D = Vector3.zero;
                container.gameObject.SetActive(true);
                canClickButton = true;
            }
        }

        private void Close(Action onClosed)
        {
            float duration = 0.3f;
            mainCamera.transform.DOMove(prevCameraPosition, duration).SetEase(Ease.OutSine);
            mainCamera.DOOrthoSize(cameraOrthographicSize, duration).SetEase(Ease.OutSine);
            container.DOAnchorPosY(yStartPosition, duration).SetEase(Ease.OutSine);
            this.WaitInvoke(duration, () =>
            {
                container.gameObject.SetActive(false);
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
                data.textPrice.SetText(weaponData.Price(equipmentLevel).ToString());
                data.textTitle.SetText(weaponData.Name);
                data.equipmentItem.Init(so.WeaponIcon, weaponData, equipmentLevel);
            }
        }
    }
}
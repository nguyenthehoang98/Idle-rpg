using System;
using System.Collections;
using System.Collections.Generic;
using _Game.GamePlay.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour
    {
        [SerializeField] private Vector3 scale = new Vector3(1.25f, 1.25f, 1.25f);
        [SerializeField] private float duration = 0.15f;
        [SerializeField] private bool enableSound;

        public event UnityAction OnClicked;

        private bool isBlockInput = false;
        public bool IsBlockInput
        {
            get => isBlockInput;
            set
            {
                isBlockInput = value;
                if (button == null) button = GetComponent<Button>();
                button.interactable = !value;
            } 
        }

        private Button button;
        private Vector3 originScale;
        private bool hasStart;
        private bool pressing;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                if (pressing || IsBlockInput) return;

                // if (enableSound) SoundManager.Instance.PlayOneShot();

                StartCoroutine(DoScaleIE(transform, scale, duration));

                OnClicked?.Invoke();
            });
        }

        private void Start()
        {
            if (transform.localScale != Vector3.zero)
            {
                originScale = transform.localScale;
            }
            else
            {
                originScale = Vector3.one;
            }

            hasStart = true;
        }

        private void OnEnable()
        {
            if (hasStart && originScale != Vector3.zero)
            {
                transform.localScale = originScale;
            }
        }

        private IEnumerator DoScaleIE(Transform target, Vector3 scaleValue, float d)
        {
            pressing = true;
            
            float halfD = d / 2f;
            Vector3 initialScale = transform.localScale;
            
            float elapsed = 0f;
            while (elapsed < halfD)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfD;
                target.localScale = Vector3.Lerp(initialScale, scaleValue, t);
                yield return null;
            }
            
            elapsed = 0f;
            while (elapsed < halfD)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfD;
                target.localScale = Vector3.Lerp(scaleValue, initialScale, t);
                yield return null;
            }

            // Đảm bảo giá trị cuối cùng chính xác
            transform.localScale = initialScale;
            
            pressing = false;
        }
    }
}
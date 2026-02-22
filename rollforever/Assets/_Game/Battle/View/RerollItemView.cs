using System;
using _Game.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RerollItemView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;

    private static bool clickable = false;
    private event Action OnSelect;
    
    void Awake()
    {
        button.onClick.AddListener(() =>
        {
            if (clickable)
            {
                if (OnSelect != null) OnSelect.Invoke();
                OnSelect = null;
                clickable = false;
            }
        });
    }

    public void Show(BuffConfig.BuffData buffData, Action onSelect)
    {
        OnSelect = onSelect;
        text.SetText($"[{buffData.StatType}] up {buffData.Value}");
        clickable = true;
    }
}

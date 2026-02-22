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

    private BuffConfig.BuffData buffData;
    private event Action<BuffConfig.BuffData> OnSelect;
    
    void Awake()
    {
        button.onClick.AddListener(() =>
        {
            if (clickable)
            {
                if (OnSelect != null) OnSelect.Invoke(buffData);
                OnSelect = null;
                clickable = false;
            }
        });
    }

    public void Show(BuffConfig.BuffData buffData, Action<BuffConfig.BuffData> onSelect)
    {
        this.buffData = buffData;
        OnSelect = onSelect;
        text.SetText($"[{buffData.StatType}] up {buffData.Value}");
        clickable = true;
    }
}

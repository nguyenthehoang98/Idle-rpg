using System;
using _Flow.Model;
using ParadoxNotion.Design;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[Category("Client")]
public class WaitClickButtonConditionTask : AbsRequestConditionTask
{
    [ParadoxNotion.Design.Header("Content")] 
    public string buttonTitleText;

    protected override void DoAction()
    {
        if (string.IsNullOrEmpty(buttonTitleText)) return;
        
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            TMP_Text tmp = btn.GetComponentInChildren<TMP_Text>();
            if (tmp != null && tmp.text.Equals(buttonTitleText, StringComparison.OrdinalIgnoreCase))
            {
                if (UISimulator.Click(btn.GetComponent<RectTransform>()))
                {
                    Completed();                
                    break;
                }
            }
        }
    }
}

using System;
using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[Category("Client")]
public class WaitClickButtonConditionTask : ConditionTask
{
    public BBParameter<string> buttonTitleName;
    public BBParameter<float> interval = new BBParameter<float>(2);
    public BBParameter<int> timeout = new BBParameter<int>(30);
    public BBParameter<bool> result = new BBParameter<bool>() { name = "CLICK_BUTTON_RESULT" };

    private Coroutine coroutine;
    private float startTriggerTime;
    private bool isClicked = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        startTriggerTime = Time.time;
        result.value = false;
        coroutine = StartCoroutine(ScanAllButtons());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if(coroutine != null) StopCoroutine(coroutine);
    }

    protected override bool OnCheck()
    {
        if (isClicked) return true;
        if (startTriggerTime + timeout.value < Time.time)
        {
            return true;
        }

        return false;
    }

    IEnumerator ScanAllButtons()
    {
        while (startTriggerTime + timeout.value > Time.time)
        {
            yield return new WaitForSeconds(interval.value);
            
            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var btn in buttons)
            {
                TMP_Text tmp = btn.GetComponentInChildren<TMP_Text>();
                if (tmp.text.Equals(buttonTitleName.value, StringComparison.OrdinalIgnoreCase))
                {
                    btn.onClick.Invoke();
                    result.value = true;
                    isClicked = true;
                }
            }
        }
    }
}

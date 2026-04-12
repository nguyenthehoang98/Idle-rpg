using System;
using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

public abstract class AbsRequestConditionTask : ConditionTask
{
    [ParadoxNotion.Design.Header("Input")] 
    public BBParameter<float> interval = new BBParameter<float>(2);
    public BBParameter<int> timeout = new BBParameter<int>(30);
    public BBParameter<bool> result = new BBParameter<bool> { name = "GAMEPLAY_REQUEST_RESULT" };

    private Coroutine coroutine;
    private float startTriggerTime;
    private bool hasSent = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        hasSent = false;
        startTriggerTime = Time.time;
        result.value = false;
        Reset();
        coroutine = StartCoroutine(SendRequest());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        hasSent = false;
    }

    void Reset()
    {
        if(coroutine != null) StopCoroutine(coroutine);
    }

    protected override bool OnCheck()
    {
        if (hasSent)
        {
            Reset();
            return true;
        }

        if (startTriggerTime + timeout.value < Time.time)
        {
            Reset();
            return true;
        }

        return false;
    }

    protected abstract void DoAction();

    protected void Completed()
    {
        hasSent = true;
        result.value = true;
    }

    protected void Failed()
    {
        hasSent = true;
        result.value = false;
    }
    
    IEnumerator SendRequest()
    {
        while (startTriggerTime + timeout.value > Time.time)
        {
            yield return new WaitForSeconds(interval.value);

            DoAction();
        }
    }
}

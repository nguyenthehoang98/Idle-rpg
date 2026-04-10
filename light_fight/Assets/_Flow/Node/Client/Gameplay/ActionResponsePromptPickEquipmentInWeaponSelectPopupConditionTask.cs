using System;
using _Flow.Model;
using _Games.Combat.Level;
using _Games.Combat.View;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Object = UnityEngine.Object;

[Category("Gameplay")]
public class ActionResponsePromptPickEquipmentInWeaponSelectPopupConditionTask : ActionTask
{
    public BBParameter<string> response = new BBParameter<string>() { name = "RESPONSE_SUCCESS" };

    protected override void OnExecute()
    {
        LevelDesign levelDesign = Object.FindAnyObjectByType<LevelDesign>(FindObjectsInactive.Exclude);
        if(levelDesign == null)
        {
            Failed("No found object: " + typeof(LevelDesign));
            return;
        }
        
        WeaponSelectPopup popup = Object.FindAnyObjectByType<WeaponSelectPopup>(FindObjectsInactive.Exclude);
        if (popup != null)
        {
            try
            {
                Data data = JsonUtility.FromJson<Data>(response.value);
                if (popup.AIFindWeaponButton(data.weaponId, out var rect))
                {
                    Vector2 from = UISimulator.GetScreenPointFromRect(rect);
                    Vector2 to = UISimulator.GetScreenPointFromWorldPosition(levelDesign.LoopPoints()[0]);
                    UISimulator.Drag(rect, from, to);
                    EndAction(true);
                }
                else
                {
                    Failed("No found WeaponButton: " + data.weaponId);
                }
            }
            catch (Exception e)
            {
                Failed("Parse json error: " + e.Message, "Json: " + response.value);
            }
        }
        else
        {
            Failed("No found popup: " + typeof(WeaponSelectPopup));
        }
    }

    void Failed(params string[] messages)
    {
        foreach (var message in messages)
        {
            Debug.LogError(message);
        }
        EndAction(false);
    }

    [Serializable]
    struct Data
    {
        public int weaponId;
    }
}
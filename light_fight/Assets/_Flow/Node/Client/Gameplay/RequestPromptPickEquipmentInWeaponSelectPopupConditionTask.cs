using System.Text;
using _Games.Combat.View;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


/*{
           "system": "You are a game AI. Choose the best item.",
           "user": {
               "items": [
               { "id": "bow", "name": "Bow", "count": 2 },
               { "id": "cannon", "name": "Cannon", "count": 2 },
               { "id": "arrow", "name": "Arrow", "count": 2 }
               ],
               "goal": "choose best weapon for long range"
           }
       }*/

[Category("Gameplay")]
public class RequestPromptPickEquipmentInWeaponSelectPopupConditionTask : AbsRequestInGameplayConditionTask
{
    [ParadoxNotion.Design.Header("Content")]
    public string promptContent = "Choose the best weapon based on highest power and lowest price. Use power/price ratio as main metric";
    [ParadoxNotion.Design.Header("Output")] 
    public BBParameter<string> prompt = new BBParameter<string>{ name = "AI_PROMPT_TEMP" };
    public BBParameter<string> systemPrompt = new BBParameter<string> { name = "AI_SYSTEM_PROMPT_TEMP" };

    private const string SYSTEM_PROMPT =
        "You are a game balancing AI. {0} . Return ONLY JSON with fields: weaponId";
    private const string PROMT =
        @"Weapon list: {0}";
    
    protected override void DoAction()
    {
        WeaponSelectPopup popup = Object.FindAnyObjectByType<WeaponSelectPopup>(FindObjectsInactive.Exclude);
        if (popup != null)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var data in popup.CurrentWeapons())
            {
                sb.AppendLine(JsonUtility.ToJson(data));
            }
            
            systemPrompt.value = string.Format(SYSTEM_PROMPT, promptContent);
            prompt.value = string.Format(PROMT,sb);
            Complete();
        }
    }
}

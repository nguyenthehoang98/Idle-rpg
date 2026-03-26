using _Games.Combat.Data;
using _Games.Combat.System;
using _KIT.Utils;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.View
{
    public class GameTimeView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textTime;

        private void Start()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = em.CreateEntityQuery(typeof(GameTimeData));
            this.WhileInvoke(0.5f, () =>
            {
                var entity = query.GetSingletonEntity();
                var gta = em.GetComponentData<GameTimeData>(entity);
                string format = TimeUtils.FormatSecondAsTime_HHMMSS((int) gta.ElapsedTime);
                textTime.SetText(format);
            });
        }
    }
}
using _KIT.Resource;
using _KIT.Utils;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.View
{
    public class GameTimeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textTime;

        public static async void Instantiate(Transform parent)
        {
            GameObject go = await KitLoaded.LoadAsync<GameObject>("GameTimeUI");
            Instantiate(go, parent).GetComponent<GameTimeUI>();
        }

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            this.WhileInvoke(0.5f, () =>
            {
                string engineTime = TimeUtils.FormatSecondAsTime_HHMMSS((int)world.Time.ElapsedTime);
                textTime.SetText(engineTime);
            });
        }
    }
}
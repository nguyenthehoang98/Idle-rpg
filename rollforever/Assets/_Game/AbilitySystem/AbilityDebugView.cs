using System.Collections.Generic;
using _KIT.Utils;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.AbilitySystem
{
    public class AbilityDebugView : MonoBehaviour
    {
        [SerializeField] private string entitiesVisitedStampBuffer;
        [SerializeField] private string entitiesCollisionBuffer;
        [SerializeField] private List<int2> entitiesVisitedStamp;
        private bool running;
        private AbilityLogic logic;

        void OnDrawGizmosSelected()
        {
            if (!running) return;

            var sourceTeam = TypeUtils.GetFieldNonPublic(logic, "sourceTeam");
            var unitId = TypeUtils.GetFieldNonPublic(logic, "unitId");
            if (sourceTeam == null || unitId == null) return;

            name = $"[{logic.AbilityData.name}]_{sourceTeam}_{unitId}";

            var monsterVisitor = TypeUtils.GetFieldNonPublic(logic, "monsterVisitor") as MonsterVisitor;
            var entityVisitedStamp = TypeUtils.GetFieldNonPublic(monsterVisitor, "entityVisitedStamp");
            if (entityVisitedStamp is NativeList<int> entityVisitedStampList && entityVisitedStampList.IsCreated)
            {
                List<int> entities = new List<int>();
                for (int i = 0; i < entityVisitedStampList.Length; i++)
                {
                    entities.Add(entityVisitedStampList[i]);
                }

                entitiesVisitedStampBuffer = string.Join(',', entities);
            }
            else entitiesVisitedStampBuffer = "Not created";

            var entitiesCollisionStamp = TypeUtils.GetFieldNonPublic(monsterVisitor, "entitiesCollisionStamp");
            if (entitiesCollisionStamp is NativeList<int> entitiesCollisionList && entitiesCollisionList.IsCreated)
            {
                List<int> entities = new List<int>();
                for (int i = 0; i < entitiesCollisionList.Length; i++)
                {
                    entities.Add(entitiesCollisionList[i]);
                }

                entitiesCollisionBuffer = string.Join(',', entities);
            }
            else entitiesCollisionBuffer = "Not created";

            var cellsVisitedStamp = TypeUtils.GetFieldNonPublic(monsterVisitor, "cellsVisitedStamp");
            if (cellsVisitedStamp is NativeList<int2> cellsVisitedStampList && cellsVisitedStampList.IsCreated)
            {
                entitiesVisitedStamp = new List<int2>();
                for (int i = 0; i < cellsVisitedStampList.Length; i++)
                {
                    entitiesVisitedStamp.Add(cellsVisitedStampList[i]);
                }
            }
            else entitiesVisitedStamp.Clear();
        }

        void Startup(AbilityLogic logic)
        {
            this.logic = logic;
            name = $"[{logic.AbilityData.name}]";
            running = true;
            container[this.logic] = this;
        }

        void Shutdown()
        {
            running = false;
            name = string.Empty;
        }

        private static GameObject parent;
        static List<AbilityDebugView> instances = new List<AbilityDebugView>();
        static Dictionary<AbilityLogic, AbilityDebugView> container = new Dictionary<AbilityLogic, AbilityDebugView>();

        public static void Release(AbilityLogic logic)
        {
            if (container.TryGetValue(logic, out var view))
            {
                view.Shutdown();
                instances.Add(view);
                container.Remove(logic);
            }
        }

        public static void Create(AbilityLogic logic)
        {
            if (instances.Count > 0)
            {
                instances[instances.Count - 1].Startup(logic);
                return;
            }

            if (parent == null)
            {
                parent = new GameObject("[Battle-Abilities]");
                DontDestroyOnLoad(parent);
            }

            GameObject go = new GameObject();
            go.transform.SetParent(parent.transform);
            go.AddComponent<AbilityDebugView>().Startup(logic);
        }
    }
}
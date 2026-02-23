using System.Collections.Generic;
using _Game.Battle.Model;
using _KIT.Utils;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.AbilitySystem
{
    public class AbilityDebugView : MonoBehaviour
    {
        [SerializeField] private string entitiesVisitedStampBuffer;
        [SerializeField] private string entitiesCollisionBuffer;
        [SerializeField] private List<int2> entitiesVisitedStamp;
        
        private AbilityLogic logic;
        private bool running;

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

            var shareData = TypeUtils.GetFieldNonPublic(logic, "shareData") as BattleStartupShareData;
            Vector2 previousPosition = Vector2.zero;
            for (var i = 0; i < entitiesVisitedStamp.Count; i++)
            {
                var p = entitiesVisitedStamp[i];
                Vector2 position = (Vector2) shareData.Matrix.CellToWorld(p);
                UnityEditor.Handles.DrawWireCube(position, new Vector3(1, 1, 1) * shareData.Matrix.CellSize);
                if (i > 0) UnityEditor.Handles.DrawLine(previousPosition, position);
                previousPosition = position;
            }
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
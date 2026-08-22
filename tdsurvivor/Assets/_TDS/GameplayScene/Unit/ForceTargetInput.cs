using System.Collections.Generic;
using _TDS.Core;
using _TDS.GameplayScene.SkillSystem;
using _Toolkit.Avoidance;
using UnityEngine;

namespace _TDS.GameplayScene.Unit
{
    /// <summary>
    /// SC04 - click/tap để force hướng tấn công:
    /// chọn quái gần điểm nhấp nhất cho tất cả weapon.
    /// Tap chỗ trống -> bỏ force, weapon tự tìm mục tiêu như cũ.
    /// </summary>
    public class ForceTargetInput : MonoBehaviour
    {
        [SerializeField] private float tapRadius = 1.5f;

        private readonly List<Weapon> heroes = new List<Weapon>();
        private Camera cam;

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            bool tap = Input.GetMouseButtonDown(0)
                       || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
            if (!tap) return;

            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;

            HandleTap(worldPos);
        }

        private void HandleTap(Vector3 worldPos)
        {
            MonsterTickRunner runner = MonsterTickRunner.Instance;
            if (runner == null) return;

            heroes.Clear();
            heroes.AddRange(FindObjectsOfType<Weapon>());
            if (heroes.Count == 0) return;

            int forced = FindNearestMonster(runner, worldPos);

            foreach (Weapon hero in heroes)
            {
                if (forced > 0) hero.ForceTarget(forced);
                else hero.ClearForceTarget();
            }
        }

        private int FindNearestMonster(MonsterTickRunner runner, Vector3 worldPos)
        {
            // ponytail: query box rộng rãi rồi lọc theo khoảng cách thật, khỏi đoán semantics size của grid
            runner.QueryAgentInRange(worldPos, Vector3.one * (tapRadius * 4f), out AgentData[] results);

            int best = 0; // entity id bắt đầu từ 1 (0 = không có)
            float bestSqr = float.MaxValue;

            for (int i = 0; i < results.Length; i++)
            {
                AgentData data = results[i];
                if (!runner.IsAlive(data.agent)) continue;

                Vector2 pos = new Vector2(data.position.x, data.position.y);
                float distSqr = (pos - (Vector2)worldPos).sqrMagnitude;
                if (distSqr < bestSqr)
                {
                    bestSqr = distSqr;
                    best = data.agent;
                }
            }

            if (best != 0 && bestSqr > tapRadius * tapRadius) return 0;
            return best;
        }
    }
}

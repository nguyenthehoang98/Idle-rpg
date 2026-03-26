using System;
using System.Collections.Generic;
using _Games.Combat.Event;
using _KIT.Event;
using _KIT.Utils;
using UnityEngine;

namespace _Games.Combat.Level
{
    public class CastleTurret : MonoBehaviour
    {
        class LineData
        {
            public Transform current;
        }

        [SerializeField] private LineRenderer lineRendererPrefab;
        [SerializeField] private Transform turretTransform;
        [SerializeField] private float distance = 1;

        private Transform[] slots;
        private List<LineRenderer> allLines = new List<LineRenderer>();
        private List<LineData> allLinesData = new List<LineData>();
        private Action<int> onTrigger;
        private float rotateSpeed = 100;
        private float angle;
        private float timeScale = 1;
        private float accumulator;
        private bool isRunning;
        
        public void Init(int totalRay, Transform[] slots, Action<int> onTrigger)
        {
            this.timeScale = KitEntryScene.Instance.GameplayScaleTime;
            this.onTrigger = onTrigger;
            this.slots = slots;
            for (int i = 0; i < totalRay; i++)
            {
                LineRenderer lr = Instantiate(lineRendererPrefab, turretTransform, true);
                lr.positionCount = 0;
                allLines.Add(lr);
                allLinesData.Add(new LineData());
            }
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<BattlePauseEvent>(OnBattlePause);
            EventBus.Instance.Subscribe<BattleResumeEvent>(OnBattleResume);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<BattlePauseEvent>(OnBattlePause);
            EventBus.Instance.Unsubscribe<BattleResumeEvent>(OnBattleResume);
        }

        private void OnBattleResume(BattleResumeEvent e)
        { 
            isRunning = true;
            
            int count = allLines.Count;
            for (var i = 0; i < count; i++)
            {
                LineRenderer line = allLines[i];
                line.positionCount = 2;
            }
        }

        private void OnBattlePause(BattlePauseEvent e)
        {
            isRunning = false;
            
            int count = allLines.Count;
            for (var i = 0; i < count; i++)
            {
                LineRenderer line = allLines[i];
                line.positionCount = 0;
            }
        }

        private void Update()
        {
            if (!isRunning) return;

            var dt = Time.deltaTime;
            accumulator += dt * timeScale;
            while (accumulator >= dt)
            {
                float delta = rotateSpeed * Time.deltaTime;
                angle += delta;
                turretTransform.Rotate(0, 0, delta);
                int count = allLines.Count;
                Vector3 origin = turretTransform.position;
                for (var i = 0; i < count; i++)
                {
                    LineRenderer line = allLines[i];
                    Vector3 direction = GetDirection(i, count, angle);
                    line.SetPosition(0, direction * 0.225f);
                    line.SetPosition(1, direction * distance);
                    RaycastHit2D hit = Physics2D.Raycast(origin, direction, 10);
                    if (hit.collider != null)
                    {
                        LineData data = allLinesData[i];
                        Transform target = hit.collider.transform;
                        int index = Array.IndexOf(slots, target);
                        if (index >= 0 && target != data.current)
                        {
                            Trigger(target, data, index);
                        }
                    }
                }
                
                accumulator -= dt;
            }
        }

        void Trigger(Transform newTarget, LineData data, int index)
        {
            SlotView view;
            if (data.current != null)
            {
                view = data.current.GetComponent<SlotView>();
                if (view != null) view.UnTrigger();
            }
            
            view = newTarget.GetComponent<SlotView>();
            if (view != null) view.Trigger();
            
            data.current = newTarget;
            onTrigger?.Invoke(index);
        }

        static Vector3 GetDirection(int index, int count, float angle)
        {
            float step = 360f / count;
            float a = angle + step * index;
            return AngleToDir(a);
        }
        
        static Vector3 AngleToDir(float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
        }
    }
}
using UnityEngine;

namespace _KITSystem.Schedule
{
    public class CpuFrame : MonoBehaviour
    {
        private CpuFrameSampler cpu;

        private void Start()
        {
            cpu = new CpuFrameSampler(45f);
        }

        private void Update()
        {
            cpu.Sample(Time.deltaTime);
        }

        private GUIStyle style;

        private void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.UpperRight;
            }

            float baseHeight = 1920f;
            float scale = Screen.height / baseHeight;

            int baseFontSize = 40; // font gốc khi 1920
            style.fontSize = Mathf.RoundToInt(baseFontSize * scale);

            float padding = baseFontSize * scale;

            Rect rect = new Rect(0, padding, Screen.width - padding, 100 * scale);

            GUI.Label(rect, $"AVG FPS={cpu.AvgFps:F1}", style);

            rect.y += padding;

            GUI.Label(rect, $"Spikes={cpu.SpikeCount}", style);

            rect.y += padding;

            GUI.Label(rect, $"Worst={cpu.WorstFrameMs:F1}ms", style);
        }

        private void OnDestroy()
        {
            Debug.Log(
                $"AVG FPS={cpu.AvgFps:F1}, " +
                $"Spikes={cpu.SpikeCount}, " +
                $"Worst={cpu.WorstFrameMs:F1}ms"
            );
        }
    }

    class CpuFrameSampler
    {
        readonly float spikeThresholdMs;

        int frameCount;
        float totalTime;
        int spikeCount;
        float worstFrameMs;

        public CpuFrameSampler(float spikeThresholdMs)
        {
            this.spikeThresholdMs = spikeThresholdMs;
            Reset();
        }

        private void Reset()
        {
            frameCount = 0;
            totalTime = 0f;
            spikeCount = 0;
            worstFrameMs = 0f;
        }

        /// Call mỗi frame (Update / Coroutine)
        public void Sample(float deltaTime)
        {
            frameCount++;
            totalTime += deltaTime;

            float ms = deltaTime * 1000f;

            if (ms > spikeThresholdMs)
            {
                spikeCount++;
                if (ms > worstFrameMs)
                    worstFrameMs = ms;
            }
        }

        public float AvgFps => frameCount > 0 ? frameCount / totalTime : 0f;

        public int SpikeCount => spikeCount;

        public float WorstFrameMs => worstFrameMs;
    }
}
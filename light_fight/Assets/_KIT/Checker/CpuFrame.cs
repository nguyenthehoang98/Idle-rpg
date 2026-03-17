using UnityEngine;

namespace _KIT.Checker
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
}
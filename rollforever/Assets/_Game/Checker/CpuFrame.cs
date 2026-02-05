using UnityEngine;

namespace Checker
{
    public class CpuFrame : MonoBehaviour
    {
        private CpuFrameSampler cpu;
        
        private void Start()
        {
            cpu = new CpuFrameSampler(33f);
        }

        private void Update()
        {
            cpu.Sample(Time.deltaTime);
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
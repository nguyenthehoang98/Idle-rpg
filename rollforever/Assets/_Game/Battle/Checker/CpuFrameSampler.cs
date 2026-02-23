namespace _Game.Battle.Checker
{
    public class CpuFrameSampler
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

        public void Reset()
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
using UnityEngine;

namespace _Game.GamePlay
{
    public class PedestalPoint : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Transform point;
        [SerializeField] private float duration = 1;
        [SerializeField] private float offset;

        private float elapsed;
        private int current;
        private Vector3 positionFrom;
        private Vector3 positionTo;
        
        private int currentIndex;

        private void OnEnable()
        {
            currentIndex = 0;
            elapsed = offset;

            positionFrom = waypoints[0].position;
            positionTo = waypoints[1].position;

            point.position = positionFrom;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            
            float f = Mathf.Clamp01(Mathf.Max(0, elapsed) / duration);
            
            point.position = Vector3.Lerp(positionFrom, positionTo, f);

            if (f >= 1)
            {
                elapsed = 0f;
                currentIndex = (currentIndex + 1) % waypoints.Length;
                int nextIndex = (currentIndex + 1) % waypoints.Length;
                positionFrom = waypoints[currentIndex].position;
                positionTo = waypoints[nextIndex].position;
            }
        }
    }
}
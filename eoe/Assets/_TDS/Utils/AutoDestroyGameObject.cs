using System.Collections;
using _GameToolkit.ResourceManagement;
using UnityEngine;

namespace _TDS.Utils
{
    public sealed class AutoDestroyGameObject : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float delay = 1f;

        private Coroutine releaseCoroutine;

        private void OnEnable()
        {
            releaseCoroutine = StartCoroutine(ReleaseAfterDelay());
        }

        private void OnDisable()
        {
            if (releaseCoroutine == null) return;

            StopCoroutine(releaseCoroutine);
            releaseCoroutine = null;
        }

        private IEnumerator ReleaseAfterDelay()
        {
            yield return new WaitForSeconds(delay);

            releaseCoroutine = null;
            Pool.Destroy(gameObject);
        }
    }
}

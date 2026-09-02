using UnityEngine;

namespace _TDS.GameConfig
{
    [System.Serializable]
    public struct PowerFormula
    {
        [SerializeField] private int id;
        [SerializeField] private float a;
        [SerializeField] private float b;

        public PowerFormula(int id, float a, float b)
        {
            this.a = a;
            this.b = b;
            this.id = id;
        }

        public int Id => id;

        public float Evaluate(float x)
        {
            return a * Mathf.Pow(x, b);
        }
    }
}

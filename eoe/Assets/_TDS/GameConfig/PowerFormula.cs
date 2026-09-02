using UnityEngine;

namespace _TDS.GameConfig
{
    [System.Serializable]
    public struct PowerFormula
    {
        [SerializeField] private int id;
        [SerializeField] private float coefficient;
        [SerializeField] private float exponent;

        public PowerFormula(int id, float coefficient, float exponent)
        {
            this.id = id;
            this.coefficient = coefficient;
            this.exponent = exponent;
        }

        public int Id => id;

        public float Evaluate(float x)
        {
            return coefficient * Mathf.Pow(x, exponent);
        }
    }
}

using UnityEngine;

namespace _TDS.GameConfig
{
    [System.Serializable]
    public struct LinearFormula
    {
        [SerializeField] private int id;
        [SerializeField] private float coefficient;
        [SerializeField] private float offset;

        public LinearFormula(int id, float coefficient, float offset)
        {
            this.id = id;
            this.coefficient = coefficient;
            this.offset = offset;
        }

        public int Id => id;

        public float Evaluate(float x)
        {
            return coefficient * x + offset;
        }
    }
}

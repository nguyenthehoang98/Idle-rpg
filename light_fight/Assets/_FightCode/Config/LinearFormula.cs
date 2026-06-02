using UnityEngine;

namespace _FightCode.Config
{
    [System.Serializable]
    public struct LinearFormula
    {
        [SerializeField] private float a;
        [SerializeField] private float b;

        public LinearFormula(float a, float b)
        {
            this.a = a;
            this.b = b;
        }

        public float Evaluate(float x)
        {
            return a * x + b;
        }
    }
}
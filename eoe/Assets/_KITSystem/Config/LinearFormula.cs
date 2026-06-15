using UnityEngine;

namespace _KITSystem.Config
{
    [System.Serializable]
    public struct LinearFormula
    {
        [SerializeField] private int id;
        [SerializeField] private float a;
        [SerializeField] private float b;

        public LinearFormula(int id, float a, float b)
        {
            this.a = a;
            this.b = b;
            this.id = id;
        }

        public int Id
        {
            get { return id; }
        }

        public float Evaluate(int x)
        {
            return a * x + b;
        }
    }
}
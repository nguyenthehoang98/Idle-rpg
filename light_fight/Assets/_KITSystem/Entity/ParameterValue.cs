using UnityEngine;

namespace _KITSystem.Entity
{
    public struct ParameterValue
    {
        public ParameterType Type;

        public int IntValue;
        
        public float FloatValue;
        
        public bool BoolValue;
        
        public string StringValue;
        
        public Vector2 VectorValue;
        
        public Vector3 Vector3Value;

        public static ParameterValue Int(int value)
        {
            return new ParameterValue
            {
                Type = ParameterType.Int,
                IntValue = value
            };
        }

        public static ParameterValue Float(float value)
        {
            return new ParameterValue
            {
                Type = ParameterType.Float,
                FloatValue = value
            };
        }

        public static ParameterValue Bool(bool value)
        {
            return new ParameterValue
            {
                Type = ParameterType.Bool,
                BoolValue = value
            };
        }

        public static ParameterValue String(string value)
        {
            return new ParameterValue
            {
                Type = ParameterType.String,
                StringValue = value
            };
        }

        public static ParameterValue Vector2(Vector2 value)
        {
            return new ParameterValue
            {
                Type = ParameterType.String,
                VectorValue = value
            };
        }

        public static ParameterValue Vector3(Vector3 value)
        {
            return new ParameterValue
            {
                Type = ParameterType.String,
                Vector3Value = value
            };
        }
    }
}
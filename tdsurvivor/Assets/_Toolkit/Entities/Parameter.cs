using UnityEngine;

namespace _Toolkit.Entities
{
    public struct Parameter
    {
        public Type TypeOfValue;

        public int Int;

        public float Float;

        public bool Boolean;

        public string String;

        public Vector2 Vector2;

        public Vector3 Vector3;

        public static Parameter CreateInt(int value)
        {
            return new Parameter
            {
                TypeOfValue = Type.Int,
                Int = value
            };
        }

        public static Parameter CreateFloat(float value)
        {
            return new Parameter
            {
                TypeOfValue = Type.Float,
                Float = value
            };
        }

        public static Parameter CreateBoolean(bool value)
        {
            return new Parameter
            {
                TypeOfValue = Type.Bool,
                Boolean = value
            };
        }

        public static Parameter CreateString(string value)
        {
            return new Parameter
            {
                TypeOfValue = Type.String,
                String = value
            };
        }

        public static Parameter CreateVector2(Vector2 value)
        {
            return new Parameter
            {
                TypeOfValue = Type.String,
                Vector2 = value
            };
        }

        public static Parameter CreateVector3(Vector3 value)
        {
            return new Parameter
            {
                TypeOfValue = Type.String,
                Vector3 = value
            };
        }

        public enum Type
        {
            Int,
            Float,
            Bool,
            String,
            Vector2,
            Vector3,
        }
    }
}
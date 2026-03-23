namespace _Games.CloudAPI.Model
{
    public interface IObjectData
    {
        public string Name();
        public object Value();
    }

    // protected this class
    internal class DefaultObjectData : IObjectData
    {
        private string name;
        private object value;

        public DefaultObjectData(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name() => name;

        public object Value() => value;
    }
}
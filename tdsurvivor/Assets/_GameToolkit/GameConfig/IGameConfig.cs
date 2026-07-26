namespace _GameToolkit.GameConfig
{
    public interface IGameConfig
    {
        void OnMappingValue();
        void OnPostImported();
        void OnValidateLinkConfig();
    }
}
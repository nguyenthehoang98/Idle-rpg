namespace _GameToolkit.GameConfig
{
    public interface IConfig
    {
        /// <summary>
        /// Gọi khi init value
        /// </summary>
        void OnMappingValue();
        
        /// <summary>
        /// Xử lý khi import file
        /// </summary>
        void OnImported();
        
        /// <summary>
        /// Import lại toàn bộ file
        /// </summary>
        void OnCompleteImported();
    }
}
namespace _GameToolkit.GameConfig
{
    public abstract class Config
    {
        /// <summary>
        /// Gọi khi init value
        /// </summary>
        public abstract void OnMappingValue();

        /// <summary>
        /// Xử lý khi import file
        /// </summary>
        public virtual void OnImported()
        {
        }

        /// <summary>
        /// Import lại toàn bộ file
        /// </summary>
        public virtual void OnCompleteImported()
        {
        }
    }
}
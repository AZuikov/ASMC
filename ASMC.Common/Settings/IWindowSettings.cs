namespace ASMC.Common.Settings
{
    /// <summary>
    /// Представляет параметры окна
    /// </summary>
    public interface IWindowSettings
    {
        /// <summary>
        /// Возвращает или задает ширину окна
        /// </summary>
        int Width
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает высоту окна
        /// </summary>
        int Height
        {
            get; set;
        }
    }
}

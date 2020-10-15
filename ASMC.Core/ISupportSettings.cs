namespace ASMC.Core
{
    /// <summary>
    /// Представляет интерфейс модели
    /// поддерживающей пользовательские
    /// параметры.
    /// </summary>
    public interface ISupportSettings
    {
        /// <summary>
        /// Возвращает или задает объект,
        /// содержащий параметры.
        /// </summary>
        object Settings
        {
            get; set;
        }
    }
}

namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Интрерфейс установки внешнего опорного источника(стандарта) частоты.
    /// </summary>
    public interface IReferenceClock
    {
        public void SetExternalReferenceClock();
        public void SetInternalReferenceClock();
    }
}
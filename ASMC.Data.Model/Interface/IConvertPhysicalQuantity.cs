using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Data.Model.Interface
{
    /// <summary>
    /// Интерфейст позволяющий конвертировать физическую величину в различные системы измерения.
    /// </summary>
    /// <typeparam name = "T"></typeparam>
    public interface IConvertPhysicalQuantity<T> where T : IPhysicalQuantity, new()
    {
        #region Methods

        /// <summary>
        /// Конвертирует физическую величину в указаную систему измерения и приподит к бозовому множителю.
        /// </summary>
        /// <param name = "u"></param>
        /// <returns></returns>
        T Convert(MeasureUnits u);

        #endregion
    }
}
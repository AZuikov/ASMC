using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;

namespace AP.Utils.Helps
{
    /// <summary>
    /// Обозначения единиц измерения.
    /// </summary>
    public enum MeasureUnits
    {
        [StringValue("В")] V,
        [StringValue("А")] I,
        [StringValue("Ом")] Ohm,
        [StringValue("Ф")] Far,
        [StringValue("Гц")] Herz,
        [StringValue("°C")] degC,
        [StringValue("°F")] DegF,
        [StringValue("дБ")] Db

    }
    /// <summary>
    /// Содержит доступные множители и их обозначения.
    /// </summary>
    public enum Multipliers
    {
        /// <summary>
        /// Множетель нано 1Е-9.
        /// </summary>
        [StringValue("н")] [DoubleValue(1E-9)] Nano,

        /// <summary>
        /// Множетель микро 1Е-6.
        /// </summary>
        [StringValue("мк")] [DoubleValue(1E-6)] Micro,

        /// <summary>
        /// Множетель мили 1Е-3.
        /// </summary>
        [StringValue("м")] [DoubleValue(1E-3)] Mili,

        /// <summary>
        /// Без множителя.
        /// </summary>
        [StringValue("")] [DoubleValue(1)] None,

        /// <summary>
        /// Множитель кило 1Е3
        /// </summary>
        [StringValue("к")] [DoubleValue(1E3)] Kilo,

        /// <summary>
        /// Мноэитель мега 1Е6
        /// </summary>
        [StringValue("М")] [DoubleValue(1E6)] Mega,

        /// <summary>
        /// Мноэитель мега 1Е6
        /// </summary>
        [StringValue("Г")] [DoubleValue(1E9)] Giga
    }
}

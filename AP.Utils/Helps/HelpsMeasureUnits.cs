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
        [StringValue("NONE")] NONE,
        [StringValue("В")] V,
        [StringValue("А")] I,
        [StringValue("Ом")] Ohm,
        [StringValue("Ф")] Far,
        [StringValue("Гц")] Frequency,
        [StringValue("°C")] degC,
        [StringValue("°F")] DegF,
        [StringValue("дБ")] Db,
        [StringValue("с")] Time,
        [StringValue("м")] Length,
        [StringValue("г")] Weight,
        /// <summary>
        /// Ньютоны
        /// </summary>
        [StringValue("Н")] N,
        [StringValue("м.рт.ст")] MercuryPressure,
        [StringValue("Па")] Pressure,
    }
    public interface IPhysicalQuantity
    {
        /// <summary>
        /// Предоставляет перечень допустимый единиц измерений. Например Давение может быть в Па и в м.рт.ст
        /// </summary>
        MeasureUnits[] Units{ get; }
        /// <summary>
        /// Позволяет задать или получить еденицу езмерения данной физической величины
        /// </summary>
        MeasureUnits Unit { get; set; }
        /// <summary>
        /// Позволяет задать или получить множитель фезической величины
        /// </summary>
        UnitMultipliers Multipliers { get; set; }
        /// <summary>
        /// Позволяет задать или получить знаенчие физической величины
        /// </summary>
        decimal Value { get; set; }
    }

    /// <summary>
    /// Интерфейст позволяющий конвертировать физическую величину в различные системы измерения.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConvertPhysicalQuantity<T> where T: IPhysicalQuantity, new()
    {
        /// <summary>
        /// Конвертирует физическую величину в указаную систему измерения и приподит к бозовому множителю.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        T Convert(MeasureUnits u);
    }
    /// <summary>
    /// Предоставляет базовую реализацию физической величины
    /// </summary>
    public abstract class PhysicalQuantity: IPhysicalQuantity
    {
        private MeasureUnits _unit;

        /// <inheritdoc />
        public MeasureUnits[] Units { get; protected set; }

        /// <inheritdoc />
        public MeasureUnits Unit
        {
            get => _unit;
            set
            {
                if (!CheckedAttachmentUnits(value))
                    throw new ArgumentOutOfRangeException($@"{value} не входит в допустимый список едениц измиериний");

                _unit = value;
            }

        }
        /// <summary>
        /// Возвращает результат проверки пренадлишности едениц измерения к физической величине.
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        protected bool CheckedAttachmentUnits(MeasureUnits units)
        {
            return Array.FindIndex(Units, item => (item) == units) != -1;
        }

        protected IPhysicalQuantity ThisConvetToSi()
        {
            var pq = (IPhysicalQuantity) Activator.CreateInstance(this.GetType());
            pq.Value = Value * (decimal) Multipliers.GetDoubleValue();
            pq.Multipliers = UnitMultipliers.None;
            pq.Unit = this.Unit;
            return pq;
        }
        /// <inheritdoc />
        public override string ToString()
        {
            return $@"{Value} {Multipliers.GetStringValue()}{Unit.GetStringValue()}";
        }
        /// <inheritdoc />
        public UnitMultipliers Multipliers { get; set; }

        /// <inheritdoc />
        public decimal Value { get; set; }
    }
    /// <summary>
    /// Содержит доступные множители и их обозначения.
    /// </summary>
    public enum UnitMultipliers
    { 
        /// <summary>
        /// Без множителя.
        /// </summary>
        [StringValue("")] [DoubleValue(1)] None,
        /// <summary>
        ///  Множетель пико 1Е-12.
        /// </summary>
        [StringValue("п")] [DoubleValue(1E-12)] Pico,

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

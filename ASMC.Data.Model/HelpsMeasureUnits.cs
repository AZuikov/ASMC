using System;
using System.Collections;
using AP.Utils.Data;

namespace ASMC.Data.Model
{

    /// <summary>
    /// Класс расширяющий <see cref="System.Enum"/>
    /// </summary>
    public static class EnumExtensions

    {

        private static readonly Hashtable UnitMultipliersValues = new Hashtable();
        private static readonly Hashtable MeasureUnitValues = new Hashtable();
        public static UnitMultiplier GetUnitMultipliersValue(this Enum value)
        {
            UnitMultiplier output = UnitMultiplier.None;
            var type = value.GetType();
            if (UnitMultipliersValues.ContainsKey(value))
                output = ((UnitMultipliersAttribute) UnitMultipliersValues[value]).Value;
            else
            {
                var fi = type.GetField(value.ToString());
                if (!(fi.GetCustomAttributes(typeof(UnitMultipliersAttribute), false) is UnitMultipliersAttribute[]
                        attrs) ||
                    attrs.Length <= 0) return output;
                UnitMultipliersValues.Add(value, attrs[0]);
                output = attrs[0].Value;
            }

            return output;
        }
        public static MeasureUnits GetMeasureUnitsValue(this Enum value)
        {
            MeasureUnits output = MeasureUnits.NONE;
            var type = value.GetType();
            if (UnitMultipliersValues.ContainsKey(value))
                output = ((MeasureUnitsValueAttribute)MeasureUnitValues[value]).Value;
            else
            {
                var fi = type.GetField(value.ToString());
                if (!(fi.GetCustomAttributes(typeof(MeasureUnitsValueAttribute), false) is MeasureUnitsValueAttribute[]
                        attrs) ||
                    attrs.Length <= 0) return output;
                UnitMultipliersValues.Add(value, attrs[0]);
                output = attrs[0].Value;
            }

            return output;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
     public class MeasureUnitsValueAttribute : Attribute
     {
     /// &lt;inheritdoc /&gt;
     public MeasureUnitsValueAttribute(MeasureUnits value)
     {
     Value = value;
     }
     /// &lt;summary&gt;
     /// Gets the value.
     /// &lt;/summary&gt;
     /// &lt;value&gt;&lt;/value&gt;
     public MeasureUnits Value { get; }
     }
     
     /// &lt;inheritdoc /&gt;
     [AttributeUsage(AttributeTargets.Field)]
     public class UnitMultipliersAttribute : Attribute
     {
     /// &lt;inheritdoc /&gt;
     public UnitMultipliersAttribute(UnitMultiplier value)
     {
     Value = value;
     }
     /// &lt;summary&gt;
     /// Gets the value.
     /// &lt;/summary&gt;
     /// &lt;value&gt;&lt;/value&gt;
     public UnitMultiplier Value { get; }
     }
    
    /// 
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
        [StringValue("Па")] Pressure
    }


    /// <summary>
    /// Содержит доступные множители и их обозначения.
    /// </summary>
    public enum UnitMultiplier
    {
        /// <summary>
        /// Без множителя.
        /// </summary>
        [StringValue("")] [DoubleValue(1)] None,

        /// <summary>
        /// Множетель пико 1Е-12.
        /// </summary>
        [StringValue("п")] [DoubleValue(1E-12)]
        Pico,

        /// <summary>
        /// Множетель нано 1Е-9.
        /// </summary>
        [StringValue("н")] [DoubleValue(1E-9)] Nano,

        /// <summary>
        /// Множетель микро 1Е-6.
        /// </summary>
        [StringValue("мк")] [DoubleValue(1E-6)]
        Micro,

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
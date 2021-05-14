using System;
using System.Collections;
using System.Globalization;
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
     public class MeasureUnitsValueAttribute : System.Attribute
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
     public class UnitMultipliersAttribute : System.Attribute
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
        [StringValue("NONE"),StringValue("NONE", "en-US")] NONE,
        [StringValue("В"), StringValue("V","en-US")] V,
        [StringValue("А"), StringValue("A", "en-US")] I,
        [StringValue("Ом"), StringValue("Ohm", "en-US")] Resistance,
        [StringValue("Ф"), StringValue("Far", "en-US")] Far,
        [StringValue("Гц"),StringValue("Hz", "en-US")] Frequency,
        [StringValue("°C"),StringValue("CEL", "en-US")] degC,
        [StringValue("°"),StringValue("°", "en-US")] degreas,
        [StringValue("°F"),StringValue("FAR", "en-US")] DegF,
        [StringValue("дБ"), StringValue("dB", "en-US")] dB,
        [StringValue("дБм"), StringValue("dBm", "en-US")] dBm,
        [StringValue("с"),StringValue("sec", "en-US")] Time,
        [StringValue("м"),StringValue("metr", "en-US")] Length,
        [StringValue("г"), StringValue("gramm", "en-US")] Weight,

        /// <summary>
        /// Ньютоны
        /// </summary>
        [StringValue("Н"), StringValue("Nu", "en-US")] N,
        [StringValue("Вт"), StringValue("W", "en-US")] Watt,

        [StringValue("мм.рт.ст"), StringValue("MercuryPressure", "en-US")] MercuryPressure,
        [StringValue("Па"), StringValue("Pressure", "en-US")] Pressure,
        [StringValue("%"), StringValue("%", "en-US")] Percent
    }

    /// <summary>
    /// Единицы измерения амплитуды сигналов.
    /// </summary>
    public enum MeasureUnitsAmplitude
    {
        [StringValue("В пик-пик"), StringValue("Vpp", "en-US")] Vpp,
        [StringValue("В ск"), StringValue("Vrms", "en-US")] Vrms,
        [StringValue("А пик-пик"), StringValue("App", "en-US")] App,
        [StringValue("A ск"), StringValue("Arms", "en-US")] Arms,
    }


    /// <summary>
    /// Содержит доступные множители и их обозначения.
    /// </summary>
    public enum UnitMultiplier
    {
        /// <summary>
        /// Без множителя.
        /// </summary>
        [StringValue("")] [StringValue("", "en-US")] [DoubleValue(1)] None,

        /// <summary>
        /// Множитель пико 1Е-12.
        /// </summary>
        [StringValue("п")]
        [StringValue("p", "en-US")]
        [DoubleValue(1E-12)]
        Pico,

        /// <summary>
        /// Множитель нано 1Е-9.
        /// </summary>
        [StringValue("н")]
        [StringValue("n", "en-US")]
        [DoubleValue(1E-9)] Nano,

        /// <summary>
        /// Множитель микро 1Е-6.
        /// </summary>
        [StringValue("мк")]
        [StringValue("u", "en-US")]
        [DoubleValue(1E-6)]
        Micro,

        /// <summary>
        /// Множитель мили 1Е-3.
        /// </summary>
        [StringValue("м")]
        [StringValue("m", "en-US")]
        [DoubleValue(1E-3)] Mili,

       

        /// <summary>
        /// Множитель кило 1Е3
        /// </summary>
        [StringValue("к")]
        [StringValue("k", "en-US")]
        [DoubleValue(1E3)] Kilo,

        /// <summary>
        /// Множитель мега 1Е6
        /// </summary>
        [StringValue("М")]
        [StringValue("M", "en-US")]
        [DoubleValue(1E6)] Mega,

        /// <summary>
        /// Множитель гига 1Е9
        /// </summary>
        [StringValue("Г")]
        [StringValue("G", "en-US")]
        [DoubleValue(1E9)] Giga
    }
}
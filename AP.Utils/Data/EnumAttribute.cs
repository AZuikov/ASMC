using System;
using System.Collections;
using AP.Utils.Helps;

namespace AP.Utils.Data
{
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Field)]
    public class MeasureUnitsValueAttribute : Attribute
    {
        /// <inheritdoc />
        public MeasureUnitsValueAttribute(MeasureUnits value)
        {
            Value = value;
        }
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value></value>
        public MeasureUnits Value { get; }
    }

    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Field)]
    public class UnitMultipliersAttribute : Attribute
    {
        /// <inheritdoc />
        public UnitMultipliersAttribute(UnitMultipliers value)
        {
            Value = value;
        }
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value></value>
        public UnitMultipliers Value { get; }
    }

    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Field)]
    public class StringValueAttribute : Attribute
    {
        /// <inheritdoc />
        public StringValueAttribute(string value)
        {
            Value = value;
        }      
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value></value>
        public string Value { get; }
    }

    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Field)]
    public class DoubleValueAttribute : Attribute
    {
        /// <inheritdoc />
        public DoubleValueAttribute(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value></value>
        public double Value { get; }
    }
    /// <summary>
    /// Класс расширяющий <see cref="System.Enum"/>
    /// </summary>
    public static class EnumExtensions

    {
        private static readonly Hashtable StringValues = new Hashtable();
        private static readonly Hashtable DoubleValues = new Hashtable();
        /// <summary>
        /// Предоставляет возможность получить значение аттрибута  <see cref="StringValueAttribute"/>
        /// </summary>
        /// <returns> возвращает строку </returns>
        public static string GetStringValue(this Enum value)
        {
            string output;
            var type = value.GetType();

            if (StringValues.ContainsKey(value))
                output = (StringValues[value] as StringValueAttribute)?.Value;
            else
            {
                var fi = type.GetField(value.ToString());
                var attrs =  fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null && attrs.Length <= 0) return null;
                if (attrs == null) return null;
                StringValues.Add(value, attrs[0]);
                output = attrs[0].Value;
            }

            return output;
        }

        /// <summary>
        /// Предоставляет возможность получить значение аттрибута  <see cref="DoubleValueAttribute"/>
        /// </summary>
        /// <returns> Возвращает значение с плавоющей точкой двойной точности указзанное в аттрибуте </returns>
        public static double GetDoubleValue(this Enum value)
        {
            double output = 0;
            var type = value.GetType();
            if (DoubleValues.ContainsKey(value))
                output = ((DoubleValueAttribute) DoubleValues[value]).Value;
            else
            {
                var fi = type.GetField(value.ToString());
                if (!(fi.GetCustomAttributes(typeof(DoubleValueAttribute), false) is DoubleValueAttribute[] attrs) ||
                    attrs.Length <= 0) return output;
                DoubleValues.Add(value, attrs[0]);
                output = attrs[0].Value;
            } 
            return output;
        }
    }
}
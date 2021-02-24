using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace AP.Utils.Data
{
    /// <inheritdoc />
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class StringValueAttribute : Attribute
    {
        #region Property

        public CultureInfo CultureInfo { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value></value>
        public string Value { get; }

        #endregion

        /// <inheritdoc />
        public StringValueAttribute(string value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public StringValueAttribute(string value, string inCultureInfo)
        {
            Value = value;
            CultureInfo = CultureInfo.GetCultureInfo(inCultureInfo);
        }
    }

    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Field)]
    public class DoubleValueAttribute : Attribute
    {
        #region Property

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value></value>
        public double Value { get; }

        #endregion

        /// <inheritdoc />
        public DoubleValueAttribute(double value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Класс расширяющий <see cref = "System.Enum" />
    /// </summary>
    public static class EnumExtensions

    {
        private static readonly Hashtable DoubleValues = new Hashtable();
        private static readonly Hashtable StringValuesNoCultureInfo = new Hashtable();
        private static readonly Hashtable StringValuesWithCultureInfo = new Hashtable();

        #region Methods

        /// <summary>
        /// Предоставляет возможность получить значение аттрибута  <see cref = "StringValueAttribute" />
        /// </summary>
        /// <returns> возвращает строку </returns>
        public static string GetStringValue(this Enum value)
        {
            string output;
            var type = value.GetType();

            var fi = type.GetField(value.ToString());
            var attrs = fi.GetCustomAttributes(false)
                          .First(q => q.GetType() == typeof(StringValueAttribute) &&
                                      ((StringValueAttribute) q).CultureInfo == null);

            if (((StringValueAttribute) attrs).CultureInfo == null && StringValuesNoCultureInfo.ContainsKey(value))
            {
                output = (StringValuesNoCultureInfo[value] as StringValueAttribute)?.Value;
            }
            else
            {
                if (attrs == null) return null;
                StringValuesNoCultureInfo.Add(value, attrs);
                output = ((StringValueAttribute) attrs).Value;
            }

            return output;
        }

        public static string GetStringValue(this Enum value, CultureInfo cultureInfo)
        {
            string output;
            var type = value.GetType();

            if (StringValuesWithCultureInfo.ContainsKey(value))
            {
                output = (StringValuesWithCultureInfo[value] as StringValueAttribute)?.Value;
            }
            else
            {
                var fi = type.GetField(value.ToString());
                var attrs = fi.GetCustomAttributes(false)
                              .FirstOrDefault(q => q.GetType() == typeof(StringValueAttribute) && (
                                                  ((StringValueAttribute) q)
                                                 .CultureInfo?.Name.Equals(cultureInfo.Name) ?? false));

                if (attrs == null) return null;
                StringValuesWithCultureInfo.Add(value, attrs);
                output = ((StringValueAttribute) attrs).Value;
            }

            return output;
        }

        /// <summary>
        /// Предоставляет возможность получить значение аттрибута  <see cref = "DoubleValueAttribute" />
        /// </summary>
        /// <returns> Возвращает значение с плавоющей точкой двойной точности указзанное в аттрибуте </returns>
        public static double GetDoubleValue(this Enum value)
        {
            double output = 0;
            var type = value.GetType();
            if (DoubleValues.ContainsKey(value))
            {
                output = ((DoubleValueAttribute) DoubleValues[value]).Value;
            }
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

        #endregion
    }
}
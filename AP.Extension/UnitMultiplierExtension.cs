using System;
using System.Globalization;
using AP.Utils.Data;
using ASMC.Data.Model;

namespace AP.Extension
{
    public static class UnitMultiplierExtension
    {
        #region Methods

        public static UnitMultiplier ParseUnitMultiplier(string inStr)
        {
            var buffer = inStr.Trim();
            if (string.IsNullOrWhiteSpace(buffer) || string.Equals(buffer, "NA")) return UnitMultiplier.None;

            foreach (UnitMultiplier unit in Enum.GetValues(typeof(UnitMultiplier)))
                if (buffer.Equals(unit.GetStringValue()))
                    return unit;

            throw new ArgumentException($"Неизвестный множитель единицы измерения: {inStr}");
        }

        public static UnitMultiplier ParseUnitMultiplier(string inStr, CultureInfo cultureInfo)
        {
            var buffer = inStr.Trim();
            if (string.IsNullOrWhiteSpace(buffer) || string.Equals(buffer, "NA")) return UnitMultiplier.None;

            foreach (UnitMultiplier unit in Enum.GetValues(typeof(UnitMultiplier)))
                if (buffer.Equals(unit.GetStringValue(cultureInfo)))
                    return unit;

            throw new ArgumentException($"Неизвестный множитель единицы измерения: {inStr}");
        }

        public static bool TryParseUnitMultiplier(string str, ref UnitMultiplier result)
        {
            try
            {
                result = ParseUnitMultiplier(str);
                return true;
            }
            catch (ArgumentException e)
            {
                return false;
            }
        }

        public static bool TryParseUnitMultiplier(string str, CultureInfo cultureInfo, ref UnitMultiplier result)
        {
            try
            {
                result = ParseUnitMultiplier(str, cultureInfo);
                return true;
            }
            catch (ArgumentException e)
            {
                return false;
            }
        }

        #endregion
    }
}
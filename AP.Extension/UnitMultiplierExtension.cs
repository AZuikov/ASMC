using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Core.Converter;
using ASMC.Data.Model;

namespace AP.Extension
{
    public static class UnitMultiplierExtension
    {
        public static UnitMultiplier ParseUnitMultiplier(string inStr)
        {
            string buffer=inStr.Trim();
            if (string.IsNullOrWhiteSpace(buffer)) return default;

            foreach (UnitMultiplier unit in Enum.GetValues(typeof(UnitMultiplier)))
            {
                if (buffer.Equals(unit.GetStringValue())) return unit;
            }

            throw new ArgumentException($"Неизвестный множитель единицы измерения: {inStr}");
        }

        static bool TrueParseUnitMultiplier(string str, ref UnitMultiplier  result)
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
    }
}

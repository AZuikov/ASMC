using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;

namespace ASMC.Devices
{
    /// <summary>
    /// Классы точности
    /// </summary>
    public static class AccuracyClass
    {
        public class Hybrid
        {
            public string Symbol { get; set; }
            public double Value { get; set; }
            public override string ToString()
            {
                return Symbol + Value;
            }
        }

        public enum Standart
        {
            /// <summary>
            /// Нулевой класс
            /// </summary>
            [StringValue("Класс точности 0")]
            Zero,
            /// <summary>
            /// Первый класс
            /// </summary>
            [StringValue("Класс точности 1")]
            First,
            /// <summary>
            /// Второй класс
            /// </summary>
            [StringValue("Класс точности 2")]
            Second
        }
      
    }
}

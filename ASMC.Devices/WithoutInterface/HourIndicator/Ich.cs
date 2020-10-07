using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ASMC.Data.Model;

namespace ASMC.Devices.WithoutInterface.HourIndicator
{
    public class Ich:IUserType
    {
        public Ich(string userType)
        {
            UserType = userType;
        }
        public MeasPoint Range { get; set; }

        /// <summary>
        /// Максимальное допутсимое усилие
        /// </summary>
       public  class MaxMeasuringForce
        {
            /// <summary>
            /// Максимальное усилие DataGridViewHeaderBorderStyle прямом ходе
            /// </summary>
            public MeasPoint StraightRun { get; set; }
            /// <summary>
            /// Колибание при прямом/обратном ходу
            /// </summary>
            public MeasPoint Oscillatons { get; set; }
            /// <summary>
            /// Измерение хода
            /// </summary>
            public MeasPoint ChangeCourse { get; set; }
        }


        /// <inheritdoc />
        public string UserType { get; }
    }
}

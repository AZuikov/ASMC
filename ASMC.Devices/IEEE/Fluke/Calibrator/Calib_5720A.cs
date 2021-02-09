// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{  
    public class Calib_5720A : CalibrMain
    {
        public Calib_5720A() 
        {
            UserType = "5720A";
            //this.Out.HerzRanges = new ICommand[] { new RangeCalibr(" M", "Множитель мега", 1E6, "HZ"), new RangeCalibr("", "Без множителя", 1E6, "HZ") };
        }
        public struct CurrPost
        {
            /// <summary>
            /// Обычная работа
            /// </summary>
            public const string Normal = "CUR_POST NORMAL";
            /// <summary>
            /// Принудительно блок
            /// </summary>
            public const string Amplifer = "CUR_POST IB5725";
        }

        protected override string GetError()
        {
            return "fault?";
        }
    }
}

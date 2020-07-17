// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{  
    public class Calib_5720A : Main_Claibr
    {
        public Calib_5720A() : base()
        {
            DeviceType = "5720A";
        }
        public Calib_5720A(string connect) : this()
        {
            StringConnection = connect;
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
    }
}

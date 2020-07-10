// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Calib_5522A:Main_Claibr
    {
        public Calib_5522A() : base()
        {
            DeviseType = "5522A";
        }
        public Calib_5522A(string connect) : this()
        {
            Stringconection = connect;
        }
    }
}

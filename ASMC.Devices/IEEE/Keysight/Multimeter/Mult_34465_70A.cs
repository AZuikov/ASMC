// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class Mult_34465_70A: Main_Mult
    {
        public Mult_34465_70A() : base()
        {
            DeviceType = "344**A";
        }
        public Mult_34465_70A(string connect) : this()
        {
            StringConnection = connect;
        }       
    }
}

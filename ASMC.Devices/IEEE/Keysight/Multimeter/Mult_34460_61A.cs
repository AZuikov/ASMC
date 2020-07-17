// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class Mult_34460_61A : Main_Mult
    {
        public Mult_34460_61A() : base()
        {
            DeviseType = "3446*A";
            //secured_code_calibr = "HP034401";
        }
        public Mult_34460_61A(string connect) : this()
        {
            StringConnection = connect;
        }
        
    }
}

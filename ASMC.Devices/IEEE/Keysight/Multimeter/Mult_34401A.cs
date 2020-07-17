// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class Mult_34401A : Main_Mult
    {
        public Mult_34401A() : base()
        {
            DeviseType = "34401A";
            secured_code_calibr = "HP034401";
        }
        public Mult_34401A(string connect) : this()
        {
            StringConnection = connect;
        }
        public class Filtr : AC.Voltage.Filtr
        {
            /// <summary>
            /// Установить фильтр 3 Гц
            /// </summary>
            public new const  string Filtr_3 = "SENS:DET:BAND 3";
            /// <summary>
            /// Установить фильтр 20 Гц, по умолчанию
            /// </summary>
            public new const string Filtr_20_Def = "SENS:DET:BAND 20";
            /// <summary>
            /// Установить фильтр 200 Гц
            /// </summary>
            public new const string Filtr_200 = "SENS:DET:BAND 200";
        }
    }
}

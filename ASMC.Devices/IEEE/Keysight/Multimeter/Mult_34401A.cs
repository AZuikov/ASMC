// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using AP.Utils.Data;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class Mult_34401A : MultMain
    {
        public Mult_34401A()
        {
            UserType = "34401A";
            SecuredCodeCalibr = "HP034401";
            this.Ac.Voltage.Filtr = new MFiltr01(this);
            
        }
     
    }
    public class MFiltr01 : AcvFiltr
    {
        private readonly MultMain _multMain;
        public new enum EFiltrs
        {
            /// <summary>
            /// Установить фильтр 3 Гц
            /// </summary>
            [StringValue("SENS:DET:BAND 3")]
            [DoubleValue(3)]
            F3,
            /// <summary>
            /// Установить фильтр 20 Гц, по умолчанию
            /// </summary>
            [StringValue("SENS:DET:BAND 20")]
            [DoubleValue(20)]
            F20,
            /// <summary>
            /// Установить фильтр 200 Гц
            /// </summary>
            [StringValue("SENS:DET:BAND 200")]
            [DoubleValue(200)]
            F200,
        }
        public MFiltr01(MultMain multMain) : base(multMain)
        {
            _multMain = multMain;
            Filters = new ICommand[]
            {
                new Command("SENS:DET:BAND 3", "ФВЧ 3 Гц",3),
                new Command("SENS:DET:BAND 20", "ФВЧ 20 Гц",20),
                new Command("SENS:DET:BAND 200", "ФВЧ 200 Гц",200),
            };
        }
        public MultMain Set(EFiltrs range = EFiltrs.F20)
        {
            _multMain.WriteLine(range.GetStringValue());
            return _multMain;
        }
    }
}

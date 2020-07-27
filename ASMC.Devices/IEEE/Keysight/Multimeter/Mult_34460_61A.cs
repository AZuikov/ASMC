// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using AP.Utils.Data;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class Mult_34460_61A : MultMain
    {
        public Mult_34460_61A() : base()
        {
            UserType = "3446*A";
            this.Dc.Current.Range = new MRange(this);
            //secured_code_calibr = "HP034401";
        }
        public Mult_34460_61A(string connect) : this()
        {
            StringConnection = connect;
        }
         public class MRange : MDc.MCurrent.MRange
         {
            public enum ERanges
            {
                /// <summary>
                /// Команда автоматического выбора придела
                /// </summary>
                [StringValue("CONF:CURR:DC:RANG AUTO ON")]
                [DoubleValue(0)]
                Auto,

                /// <summary>
                /// Предел 100 мкА
                /// </summary>
                [StringValue("CONF:CURR:DC 10 UA")]
                [DoubleValue(10E-6)]
                U10,

                /// <summary>
                /// Предел 100 мА
                /// </summary>
                [StringValue("CONF:CURR:DC 100 MA")]
                [DoubleValue(100E-3)]
                M100,

                /// <summary>
                /// Предел 10 мА
                /// </summary>
                [StringValue("CONF:CURR:DC 10 MA")]
                [DoubleValue(10E-3)]
                M10,

                /// <summary>
                /// Предел 1 мА
                /// </summary>
                [StringValue("CONF:CURR:DC 1 MA")]
                [DoubleValue(1E-3)]
                M1,

                /// <summary>
                /// Предел 1 А
                /// </summary>
                [StringValue("CONF:CURR:DC 1")]
                [DoubleValue(1)]
                A1,
                /// <summary>
                /// Предел 3 А
                /// </summary>
                [StringValue("CONF:CURR:DC 3")]
                [DoubleValue(3)]
                A3,
                /// <summary>
                /// Предел 10 А
                /// </summary>
                [StringValue("CONF:CURR:DC 10")]
                [DoubleValue(10)]
                A10,
            }

             private readonly MultMain _multMain;
             public MultMain Set(ERanges range = ERanges.Auto)
             {
                 _multMain.WriteLine(range.GetStringValue());
                 return _multMain;
             }
            public MRange(MultMain multMain) : base(multMain)
            {
                _multMain = multMain;
                Ranges = new ICommand[]{
                    new Command("CONF:CURR:DC:RANG AUTO ON", "Автоматичсекий выбор предела", 0),
                    new Command("CONF:CURR:DC 10 UA", "10 мкА", 10E-6),
                    new Command("CONF:CURR:DC 100 MA", "100 мА", 100E-3),
                    new Command("CONF:CURR:DC 10 MA", "10 мА", 10E-3),
                    new Command("CONF:CURR:DC 1 MA", "1 мА", 1E-3),
                    new Command("CONF:CURR:DC 1 A","1 А", 1),
                    new Command("CONF:CURR:DC 3 A","3 А", 3),
                    new Command("CONF:CURR:DC 10 A","10 А", 10)  };
            }
         }
    }
}

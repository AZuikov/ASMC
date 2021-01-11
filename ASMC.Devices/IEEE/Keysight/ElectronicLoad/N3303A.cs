//using AP.Reports.Utils;
using AP.Utils.Data;
using NLog;

namespace ASMC.Devices.IEEE.Keysight.ElectronicLoad
{
    public class N3303A : MainN3300
    {
        public N3303A() 
        {
            UserType = "N3303A";
            ModuleModel = UserType;

            //Пределы воспроизведения сопротивлений в режиме CR

            this.RangeVoltArr = new decimal[2] { 24, 240 };
            this.RangeCurrentArr = new decimal[2] { 1, 10 };
          
           this.ResistanceLoad= new Resistance03(this);

          

        }
        
        
    }
    public class Resistance03 : ResistanceLoad
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly MainN3300 _mainN3300;
        public enum ERanges
        {
            [StringValue("RESistance:RANGe 48")]
            [DoubleValue(48)]
            Res48,
            [StringValue("RESistance:RANGe 480")]
            [DoubleValue(480)]
            Res480,
            [StringValue("RESistance:RANGe 4800")]
            [DoubleValue(4800)]
            Res4800,
            [StringValue("RESistance:RANGe 12000")]
            [DoubleValue(12000)]
            Res12000
        }
        /// <summary>
        /// Устанавливает ВЕЛИЧИНУ сопротивления для режима CR
        /// </summary>
        /// <param name = "value"></param>
        /// <param name = "mult"></param>
        public MainN3300 Set(ERanges value)
        {
            _mainN3300.WriteLine(value.GetStringValue());
            return _mainN3300;
        }
        public Resistance03(MainN3300 mainN3300) : base(mainN3300)
        {
            _mainN3300 = mainN3300;
            this.Ranges = new ICommand[]
            {
                new Command("RESistance:RANGe 48", "", 48), new Command("RESistance:RANGe 480", "", 480), new Command("RESistance:RANGe 4800", "", 4800), new Command("RESistance:RANGe 12000", "", 12000)
            };
        }
    }
}

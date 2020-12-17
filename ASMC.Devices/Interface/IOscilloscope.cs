using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;

namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Интерфейс осциллографа.
    /// </summary>
    public interface IOscilloscope
    {
        public  IOscillChanel[] Chanels { get; }
        /// <summary>
        /// Выбирает канал, с которым дальше работаем
        /// </summary>
        void ChanelOn(int ChanelNumber);
        void ChanelOff(int ChanelNumber);
        

    }
    /// <summary>
    /// Интерфейс канала осциллографа.
    /// </summary>
   public interface IOscillChanel:IOscillCoupling
    {
        public IeeeBase Device { get; }
        public int Number { get;  }
       public bool IsEnable { get; set; }
       public MeasPoint<Voltage> VerticalOffset { get; set; }
       public MeasPoint<Voltage> Vertical { get; set; }
       public MeasPoint<Resistance> Impedance { get; set; }
       
       public int Probe { get; set; }
       /// <summary>
       /// Получение всех параметров канала с прибора.
       /// </summary>
       public void Getting();
       /// <summary>
       /// Запись всех параметров канала в прибор.
       /// </summary>
       public void Setting();

    }

   /// <summary>
   /// Полоса пропускания.
   /// </summary>
   public interface IOscillBandWidth
   {
       MeasPoint<Frequency> BandWidth { get; set; }
   }

   public interface IOscillCoupling
   {
        public Coupling coupling { get; set; }
   }

   public enum Coupling
   {
        AC,
        DC,
        AC_DC,
        GND
   }

   

   public interface IOscillMeasure
   {
       /// <summary>
       /// Получение значениея измеренного параметра с канала осциллографа.
       /// </summary>
       /// <param name="inChanel">Номер канала осциллографа.</param>
       /// <param name="inMeasParam">Наименование измереяемого параметра</param>
       /// <param name="measNum">Номер измерения канала (опционально, может не применятся у конкретного осциллографа).</param>
       /// <returns></returns>
       IMeasPoint<IPhysicalQuantity> GetParametr(IOscillChanel inChanel, MeasParam inMeasParam, int? measNum);
      
   }

   public class MeasParam : ICommand
   {
       public MeasParam(string inStrCommand, string inDescription, double value)
       {
           StrCommand = inStrCommand;
           inDescription = inDescription;
           Value = value;
       }
       public string StrCommand { get; }
       public string Description { get; }
       public double Value { get; }
   }

}

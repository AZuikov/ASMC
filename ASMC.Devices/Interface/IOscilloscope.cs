using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Интерфейс осциллографа.
    /// </summary>
    public interface IOscilloscope
    {
        public  IChanel[] Chanels { get; }
        /// <summary>
        /// Выбирает канал, с которым дальше работаем
        /// </summary>
        void ChanelOn(int ChanelNumber);
        void ChanelOff(int ChanelNumber);
        

    }
    /// <summary>
    /// Интерфейс канала осциллографа.
    /// </summary>
   public interface IChanel
    {
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
   public interface IBandWidth
   {
       MeasPoint<Frequency> BandWidth { get; set; }
   }

   public interface ICoupling
   {
        public Coupling coupling { get; set; }
   }

   public enum Coupling
   {
        AC,
        DC,
        AC_DC
   }

   public interface IMeasure
   {
       public IChanel source { get; set; }

       MeasPoint<Frequency> GetFrequency();
       MeasPoint<Time> GetPeriod();
       MeasPoint<Time> GetRiseTime();
       MeasPoint<Time> GetFallTime();
       MeasPoint<Voltage> GetAmplitude();
       MeasPoint<Voltage> GetRmsVolt();
       MeasPoint<Voltage> GetMeanVolt();



   }

}

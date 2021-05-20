using ASMC.Devices.Interface;
using System;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public abstract class CounterAbstract : ICounter
    {
        
        protected IeeeBase device = new IeeeBase();
        public ICounterInput InputA { get; set; }
        public ICounterInput InputB { get; set; }
        public ICOunterInputHighFrequency InputC_HighFrequency { get; set; }
        public ICounterInputDualChanelMeasure DualChanelMeasure { get; set; }
        public ICounterAverageMeasure AverageMeasure { get; protected set; }
        public string UserType { get; protected set; }
        
        

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        /// <inheritdoc />
        public abstract  void Initialize();
        

        public string StringConnection
        {
            get => device.StringConnection;
            set
            {
                device.StringConnection = value;
                Initialize();
                

            }
        }

        public virtual void SetExternalReferenceClock()
        {
            device.WriteLine($":ROSCillator:SOURce EXT");
            device.WaitingRemoteOperationComplete();
        }

        public virtual void SetInternalReferenceClock()
        {
            device.WriteLine($":ROSCillator:SOURce INT");
            device.WaitingRemoteOperationComplete();
        }

      


    }

    public   class CounterAverageMeasure :ICounterAverageMeasure
    {
        public bool isAverageOn { get;  set; }

        private decimal _averCount;
        public PhysicalRange<NoUnits> AverageCountRange { get; private set; }
        public virtual decimal averageCount { get=>  _averCount;
            set
            {
                if (value < AverageCountRange.Start.MainPhysicalQuantity.GetNoramalizeValueToSi())
                    _averCount = AverageCountRange.Start.MainPhysicalQuantity.GetNoramalizeValueToSi();
                else if (value > AverageCountRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi())
                    _averCount = AverageCountRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi();
                else
                {
                    _averCount = value;
                }
            }
        }
        /// <summary>
        /// Настройки усреднения измерения.
        /// </summary>
        /// <param name="averageMinVal">Минимально допустимое значение усреднения.</param>
        /// <param name="averageMaxVal">Максимально допустимое значение усреднения.</param>
        public CounterAverageMeasure(int averageMinVal, int averageMaxVal)
        {
            AverageCountRange = new PhysicalRange<NoUnits>
            {
                Start = new MeasPoint<NoUnits>(averageMinVal),
                End = new MeasPoint<NoUnits>(averageMaxVal)
            };
            isAverageOn = false;
            averageCount = AverageCountRange.Start.MainPhysicalQuantity.GetNoramalizeValueToSi();
        }

    }
}
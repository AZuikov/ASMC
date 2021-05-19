using ASMC.Devices.Interface;
using System;
using System.Threading.Tasks;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public abstract class CounterAbstract : ICounter
    {
        
        protected IeeeBase device = new IeeeBase();

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

        public ICounterInput InputA { get; set; }
        public ICounterInput InputB { get; set; }
        public ICOunterInputHighFrequency InputC_HighFrequency { get; set; }
        public ICounterInputDualChanelMeasure DualChanelMeasure { get; set; }
    }
}
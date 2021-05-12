using ASMC.Devices.Interface;
using System;
using System.Threading.Tasks;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public abstract class CounterAbstract : ICounter
    {
        
        protected IeeeBase counter = new IeeeBase();

        public string UserType { get; protected set; }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        /// <inheritdoc />
        public abstract  Task InitializeAsync();
        

        public string StringConnection
        {
            get => counter.StringConnection;
            set
            {
                counter.StringConnection = value;
                InitializeAsync();

            }
        }

        public virtual void SetExternalReferenceClock()
        {
            counter.WriteLine($":ROSCillator:SOURce EXT");
            counter.WaitingRemoteOperationComplete();
        }

        public virtual void SetInternalReferenceClock()
        {
            counter.WriteLine($":ROSCillator:SOURce INT");
            counter.WaitingRemoteOperationComplete();
        }

        public ICounterInput InputA { get; set; }
        public ICounterInput InputB { get; set; }
        public ICOunterInputHighFrequency InputC_HighFrequency { get; set; }
        public ICounterInputDualChanelMeasure DualChanelMeasure { get; set; }
    }
}
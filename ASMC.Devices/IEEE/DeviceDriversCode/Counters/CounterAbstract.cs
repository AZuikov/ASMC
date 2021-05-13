using ASMC.Devices.Interface;
using System;
using System.Threading.Tasks;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public abstract class CounterAbstract : ICounter
    {
        
        protected IeeeBase DeviceIeeeBase = new IeeeBase();

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
            get => DeviceIeeeBase.StringConnection;
            set
            {
                DeviceIeeeBase.StringConnection = value;
                InitializeAsync();

            }
        }

        public virtual void SetExternalReferenceClock()
        {
            DeviceIeeeBase.WriteLine($":ROSCillator:SOURce EXT");
            DeviceIeeeBase.WaitingRemoteOperationComplete();
        }

        public virtual void SetInternalReferenceClock()
        {
            DeviceIeeeBase.WriteLine($":ROSCillator:SOURce INT");
            DeviceIeeeBase.WaitingRemoteOperationComplete();
        }

        public ICounterInput InputA { get; set; }
        public ICounterInput InputB { get; set; }
        public ICOunterInputHighFrequency InputC_HighFrequency { get; set; }
        public ICounterInputDualChanelMeasure DualChanelMeasure { get; set; }
    }
}
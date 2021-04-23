using ASMC.Devices.Interface;
using System;
using System.Threading.Tasks;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public abstract class CounterAbstract : ICounter
    {
        /// этот объект нужен только для установки параметров внешнего/внутреннего источника частоты (опорного стандарта).
        private IeeeBase deviceForReferenceClock = new IeeeBase();

        public string UserType { get; protected set; }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        public virtual async Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public string StringConnection
        {
            get => deviceForReferenceClock.StringConnection;
            set
            {
                deviceForReferenceClock.StringConnection = value;
                InputA.StringConnection = value;
                InputB.StringConnection = value;
                InputC_HighFrequency.StringConnection = value;
            }
        }

        public virtual void SetExternalReferenceClock()
        {
            deviceForReferenceClock.WriteLine($":ROSCillator:SOURce EXT");
            deviceForReferenceClock.WaitingRemoteOperationComplete();
        }

        public virtual void SetInternalReferenceClock()
        {
            deviceForReferenceClock.WriteLine($":ROSCillator:SOURce INT");
            deviceForReferenceClock.WaitingRemoteOperationComplete();
        }

        public ICounterInput InputA { get; set; }
        public ICounterInput InputB { get; set; }
        public ICOunterInputHighFrequency InputC_HighFrequency { get; set; }
        public ICounterInputDualChanelMeasure DualChanelMeasure { get; set; }
    }
}
﻿using System;
using System.Threading.Tasks;
using ASMC.Devices.Interface;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public abstract class CounterAbstract : ICounter
    {
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

        public string StringConnection { get; set; }

        public virtual void SetExternalReferenceClock()
        {
            //:ROSCillator:SOURce EXT
            throw new NotImplementedException();
        }

        public virtual void SetInternalReferenceClock()
        {
            //:ROSCillator:SOURce INT
            throw new NotImplementedException();
        }

        public ICounterInputStandartSingleChanelMeasure InputA { get; set; }
        public ICounterInputStandartSingleChanelMeasure InputB { get; set; }
        public ICounterInputStandartSingleChanelMeasure InputE { get; set; }
    }
}
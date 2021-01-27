﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes.E36XXa;

namespace E363xAPlugin
{
    public class E3633A : Program<Operation<E3633ADevice>>
    {
        public E3633A(ServicePack servicePack) : base(servicePack)
        {
            Grsi = "26951-04";
            Type = "E3633A";

            var testPowerSupply = new E3633ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }
}

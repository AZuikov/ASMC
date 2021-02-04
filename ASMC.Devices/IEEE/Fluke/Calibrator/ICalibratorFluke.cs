using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class ICalibratorFluke: 
      
        ISourcePhysicalQuantity<Current>, 
        ISourcePhysicalQuantity<Voltage,Frequency>,
        ISourcePhysicalQuantity<Resistance>,
        ISourcePhysicalQuantity<Temperature>
    {
        private IRangePhysicalQuantity<Current> _rangeStorage;
        private IRangePhysicalQuantity<Voltage, Frequency> _rangeStorage1;
        private IRangePhysicalQuantity<Resistance> _rangeStorage2;
        private IRangePhysicalQuantity<Temperature> _rangeStorage3;
        private IRangePhysicalQuantity<Current> _rangeStorage4;
        private IRangePhysicalQuantity<Voltage, Frequency> _rangeStorage5;
        private IRangePhysicalQuantity<Resistance> _rangeStorage6;
        private IRangePhysicalQuantity<Temperature> _rangeStorage7;

        /// <inheritdoc />
        public void Getting()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Setting()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetValue<T>()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsEnableOutput { get; }

        /// <inheritdoc />
        public void OutputOn()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void OutputOff()
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        IRangePhysicalQuantity<Current> ISourcePhysicalQuantity<Current>.RangeStorage
        {
            get => _rangeStorage4;
            set => _rangeStorage4 = value;
        }

        /// <inheritdoc />
        IRangePhysicalQuantity<Voltage, Frequency> ISourcePhysicalQuantity<Voltage, Frequency>.RangeStorage
        {
            get => _rangeStorage5;
            set => _rangeStorage5 = value;
        }

        /// <inheritdoc />
        IRangePhysicalQuantity<Resistance> ISourcePhysicalQuantity<Resistance>.RangeStorage
        {
            get => _rangeStorage6;
            set => _rangeStorage6 = value;
        }

        /// <inheritdoc />
        IRangePhysicalQuantity<Temperature> ISourcePhysicalQuantity<Temperature>.RangeStorage
        {
            get => _rangeStorage7;
            set => _rangeStorage7 = value;
        }
    }
}

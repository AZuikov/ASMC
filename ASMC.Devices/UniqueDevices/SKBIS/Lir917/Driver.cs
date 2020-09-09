using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;

namespace ASMC.Devices.UniqueDevices.SKBIS.Lir917
{
    public class Driver: IDeviceBase
    {
        /// <inheritdoc />
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string UserType { get; }

        /// <inheritdoc />
        public void Close()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Open()
        {
            throw new NotImplementedException();
        }
    }
}

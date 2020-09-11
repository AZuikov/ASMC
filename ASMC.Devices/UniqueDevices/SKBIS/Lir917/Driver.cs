using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Devices.USB_Device.SiliconLabs;
using NLog;

namespace ASMC.Devices.UniqueDevices.SKBIS.Lir917
{
    public class Driver: IDeviceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private UsbExpressWrapper Wrapper;
        public int? NubmerDevice { get; set; }
        public Driver()
        {
            Wrapper = new UsbExpressWrapper();
        }
        /// <inheritdoc />
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string UserType { get; protected set; }

        /// <inheritdoc />
        public void Close()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Open()
        {
            if (NubmerDevice != null)
            {
                IsOpen = true;
                return;
            }
            try
            {
                Wrapper.Open((int)NubmerDevice);
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Не удалось открыть порт {UserType}");
                throw;
            }
        }

        /// <inheritdoc />
        public bool IsOpen { get; protected set; }
    }
}

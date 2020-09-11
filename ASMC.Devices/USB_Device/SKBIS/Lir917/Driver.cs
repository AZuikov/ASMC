﻿using System;
using ASMC.Data.Model;
using ASMC.Devices.USB_Device.SiliconLabs;
using NLog;

namespace ASMC.Devices.USB_Device.SKBIS.Lir917
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
            try
            {
                Wrapper.Close();
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Не удалось закрыть порт {UserType}");
                throw;
            }
            finally
            {
                IsOpen = false;
            }
        }

        /// <inheritdoc />
        public void Open()
        {
            if (NubmerDevice == null)
            {
                IsOpen = false;
                return;
            }
            try
            {
                Wrapper.Open((int)NubmerDevice);
                IsOpen = true;
            }
            catch (Exception e)
            {
                IsOpen = false;
                Logger.Error(e, $@"Не удалось открыть порт {UserType}");
                throw;
            }
        }

        /// <inheritdoc />
        public bool IsOpen { get; protected set; }
    }
}
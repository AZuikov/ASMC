using System;
using System.Linq;
using ASMC.Data.Model;
using ASMC.Devices.USB_Device.SiliconLabs;
using NLog;

namespace ASMC.Devices.USB_Device.SKBIS.Lir917
{
    public class Driver: IDeviceBase
    {
        public delegate void AnchorRefereceFlag();

        protected enum CommandQuery:byte
        {
            /// <summary>
            /// команда сброса относительной координаты
            /// </summary>
            DropRelCoordinate = 0x30,
            /// <summary>
            /// команда сброса абсолютной координаты
            /// </summary>
            DropAbsCoordinate = 0x31,
            /// <summary>
            /// команда запроса пакета
            /// </summary>
            PackageRequest = 0x33
        }
        protected enum Address 
        {
        /// <summary>
        /// номер абсолютной координаты в пакете
        /// </summary>
        AbsCoordNum = 1,
        /// <summary>       
        /// номер относительной координаты в пакете
        /// </summary>      
        RelCoordNum = 5,
        /// <summary>       
        /// Yомер координаты референтой метки в пакете
        /// </summary>      
        RefCoordNum = 9
    }
        /// <summary>
        /// размер считываемого пакета
        /// </summary>
        protected const uint InPackageSize = 13;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private UsbExpressWrapper Wrapper;
        public int? NubmerDevice { get; set; }
        public Driver()
        {
            UserType = "Lir917";
            Wrapper = new UsbExpressWrapper();
        }
        /// <summary>
        /// позволяет получить ключество подключенных устройств
        /// </summary>
        public static int CountDeviceConnect
        {
            get => UsbExpressWrapper.GetCountDevices();
        }
        /// <inheritdoc />
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string UserType { get; protected set; }

        /// <summary>
        /// флаг питания датчика
        /// </summary>
        public const byte VccFlag = 0x40;

        /// <summary>
        /// флаг захвата референтной метки
        /// </summary>
        public const byte RefMarkFlag = 0x80;
        public void ReadPackeg()
        {
            Open();
         uint byteCount=0;
        Wrapper.Write((byte)CommandQuery.PackageRequest , ref byteCount, IntPtr.Zero);
        Byffer = Wrapper.Read(InPackageSize, ref byteCount, IntPtr.Zero);
       if ((Byffer[0] & VccFlag) != 0) throw new Efm32USBEpressException($"Отсутствиет питиание на разъме датчика {UserType}.");
       Close();

        }

        public int Relative
        {
            get
            {
                return BitConverter.ToInt32(Byffer, (int) Address.RelCoordNum);
            }
        }
        public int Absolute
        {
            get
            {
                return BitConverter.ToInt32(Byffer, (int)Address.AbsCoordNum);
            }
        }
        public int LastReferenceValue
        {
            get
            {
                return BitConverter.ToInt32(Byffer, (int)Address.RefCoordNum);
            }
        }
        public event AnchorRefereceFlag AnchorRefereceFlagEvent;

        protected byte[] Byffer { get; set; }
        public bool IsRefereceFlag
        {
            get
            {
                var flag = (Byffer.FirstOrDefault() & RefMarkFlag) != 0;
                if(flag) AnchorRefereceFlagEvent?.Invoke();
                return flag;
            }
        }

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
                Wrapper.FlushBuffers(true, true);
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

using System;
using System.Linq;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Devices.USB_Device.SiliconLabs;
using NLog;

namespace ASMC.Devices.USB_Device.SKBIS.Lir917
{
    public class Driver : IDeviceRemote
    {
       
        public delegate void AnchorRefereceFlag(int anchorRefereceValue);

        /// <summary>
        /// размер считываемого пакета
        /// </summary>
        protected const uint InPackageSize = 13;

        /// <summary>
        /// флаг захвата референтной метки
        /// </summary>
        public const byte RefMarkFlag = 0x80;

        /// <summary>
        /// флаг питания датчика
        /// </summary>
        public const byte VccFlag = 0x40;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private uint _byteCount;
        private readonly UsbExpressWrapper Wrapper;

        #endregion

        public event AnchorRefereceFlag AnchorRefereceFlagEvent;

        #region Property

        public int Absolute => BitConverter.ToInt32(Byffer, (int) Address.AbsCoordNum);

        /// <summary>
        /// Позволяет получить количество подключенных устройств
        /// </summary>
        public static int CountDeviceConnect => UsbExpressWrapper.GetCountDevices();

        public bool IsRefereceFlag
        {
            get
            {
                var flag = (Byffer.FirstOrDefault() & RefMarkFlag) != 0;
                if (flag) AnchorRefereceFlagEvent?.Invoke(LastReferenceValue);
                return flag;
            }
        }

        public int LastReferenceValue => BitConverter.ToInt32(Byffer, (int) Address.RefCoordNum);

        public int? NubmerDevice { get; set; }

        public int Relative => BitConverter.ToInt32(Byffer, (int) Address.RelCoordNum);

        protected byte[] Byffer { get; set; }

        #endregion

        public Driver()
        {
            UserType = "Lir917";
            Wrapper = new UsbExpressWrapper();
        }

        #region Methods

        public void Query()
        {
            if (!IsOpen) throw new Efm32USBEpressException($"порт закрыт для устройства {UserType}");
            Wrapper.Write((byte) CommandQuery.PackageRequest, ref _byteCount, IntPtr.Zero);
            Byffer = Wrapper.Read(InPackageSize, ref _byteCount, IntPtr.Zero);
            if ((Byffer[0] & VccFlag) != 0)
                throw new Efm32USBEpressException($"Отсутствиет питиание на разъме датчика {UserType}.");
        }

        public void ResetAbsolute()
        {
            if (!IsOpen) throw new Efm32USBEpressException($"Порт закрыт для устройства {UserType}");
            Wrapper.Write((byte) CommandQuery.DropAbsCoordinate, ref _byteCount, IntPtr.Zero);
        }

        public void ResetAll()
        {
            ResetAbsolute();
            ResetRelative();
        }

        public void ResetRelative()
        {
            if (!IsOpen) throw new Efm32USBEpressException($"Порт закрыт для устройства {UserType}");
            Wrapper.Write((byte) CommandQuery.DropRelCoordinate, ref _byteCount, IntPtr.Zero);
        }

        #endregion

        /// <inheritdoc />
        public virtual void Dispose()
        {
            Close();
        }


        /// <inheritdoc />
        public string UserType { get; protected set; }

        /// <inheritdoc />
        public void Close()
        {
            if (!IsOpen) return;
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
            if (IsOpen) return;
            if (NubmerDevice == null)
            {
                IsOpen = false;
                return;
            }

            try
            {
                Wrapper.Open((int) NubmerDevice);
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

        /// <inheritdoc />
        public virtual bool IsTestConnect
        {
            get
            {
                Open();
                try
                {
                    return Wrapper.ProductInfo != null;
                }
                catch 
                {
                    return false;
                }
            }
        }

        public async Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        protected enum CommandQuery : byte
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
    }
}
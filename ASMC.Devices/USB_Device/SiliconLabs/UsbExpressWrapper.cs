using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NLog;

namespace ASMC.Devices.USB_Device.SiliconLabs
{
    public class NativeMethods
    {
        private const string SiUsbXp = "SiUSBXp.dll";

        #region Methods

        [DllImport(SiUsbXp)]
        public static extern int SI_GetNumDevices(
            ref uint lpdwNumDevices
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_GetProductString(
            uint dwDeviceNum,
            byte[] lpvDeviceString,
            uint dwFlags
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_Open(
            uint dwDevice,
            ref IntPtr cyHandle
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_Close(
            IntPtr cyHandle
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_Read(
            IntPtr cyHandle,
            byte[] lpBuffer,
            uint dwBytesToRead,
            ref uint lpdwBytesReturned,
            IntPtr o
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_Write(
            IntPtr cyHandle,
            byte[] lpBuffer,
            uint dwBytesToWrite,
            ref uint lpdwBytesWritten,
            IntPtr o
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_DeviceIOControl(
            IntPtr cyHandle,
            uint dwIoControlCode,
            byte[] lpInBuffer,
            uint dwBytesToRead,
            byte[] lpOutBuffer,
            uint dwBytesToWrite,
            ref uint lpdwBytesSucceeded
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_FlushBuffers(
            IntPtr cyHandle,
            byte flushTransmit,
            byte flushReceive
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_SetTimeouts(
            uint dwReadTimeout,
            uint dwWriteTimeout
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_GetTimeouts(
            ref uint lpdwReadTimeout,
            ref uint lpdwWriteTimeout
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_CheckRXQueue(
            IntPtr cyHandle,
            ref uint lpdwNumBytesInQueue,
            ref uint lpdwQueueStatus
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_SetBaudRate(
            IntPtr cyHandle,
            uint dwBaudRate
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_SetBaudDivisor(
            IntPtr cyHandle,
            ushort wBaudDivisor
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_SetLineControl(
            IntPtr cyHandle,
            ushort wLineControl
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_SetFlowControl(
            IntPtr cyHandle,
            byte bCtsMaskCode,
            byte bRtsMaskCode,
            byte bDtrMaskCode,
            byte bDsrMaskCode,
            byte bDcdMaskCode,
            bool bFlowXonXoff
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_GetModemStatus(
            IntPtr cyHandle,
            ref byte modemStatus
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_SetBreak(
            IntPtr cyHandle,
            ushort wBreakState
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_ReadLatch(
            IntPtr cyHandle,
            ref byte lpbLatch
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_WriteLatch(
            IntPtr cyHandle,
            byte bMask,
            byte bLatch
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_GetPartNumber(
            IntPtr cyHandle,
            ref byte lpbPartNum
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_GetDeviceProductString(
            IntPtr cyHandle,
            byte[] lpProduct,
            ref byte lpbLength,
            bool bConvertToAscii
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_GetDLLVersion(
            ref uint highVersion,
            ref uint lowVersion
        );

        [DllImport(SiUsbXp)]
        public static extern int SI_GetDriverVersion(
            ref uint highVersion,
            ref uint lowVersion
        );

        #endregion
    }

    /// <summary>
    /// </summary>
    /// <remarks>USBXPRESS® PROGRAMMER’S GUIDE</remarks>
    public class UsbExpressWrapper
    {
        /// <summary>
        /// Коды, возвращаемые функциями библиотеки.
        /// </summary>
        public enum StatusCode
        {
            /// <summary>
            /// Ошибок нет.
            /// </summary>
            [Description("Ошибок нет.")] Success,

            [Description("Не верный дескриптор.")] InvalidHandle,

            /// <summary>
            /// Ошибка чтения.
            /// </summary>
            [Description("Ошибка чтения.")] ReadError,

            [Description("Превышена.")] RxQueueNotReady,

            /// <summary>
            /// Ошибка записи
            /// </summary>
            [Description("Ошибка записи.")] WriteError,

            /// <summary>
            /// Ошбка сброса
            /// </summary>
            [Description("Ошибка сброса.")] ResetError,

            /// <summary>
            /// Неккоректный параметр.
            /// </summary>
            [Description("Неккоректный параметр.")]
            InvalidParameter,

            /// <summary>
            /// Превышена длинна запроса
            /// </summary>
            [Description("Превышена длинна запроса.")]
            InvalidRequestLength,

            [Description("Превышена.")] DeviceIoFailed,

            /// <summary>
            /// Не корректная скорость передачи данных.
            /// </summary>
            [Description("Не корректная скорость передачи данных.")]
            InvalidBaudrate,

            /// <summary>
            /// Функция не поддерживается.
            /// </summary>
            [Description("Функция не поддерживается.")]
            FunctionNotSupported,

            /// <summary>
            /// Ошибка данных.
            /// </summary>
            [Description("Ошибка данных")] GlobalDataError,

            /// <summary>
            /// Системная ошибка.
            /// </summary>
            [Description("Системная ошибка.")] SystemErrorCode,

            /// <summary>
            /// Превышено время чтения.
            /// </summary>
            [Description("Превышено время чтения.")]
            ReadTimedOut,

            /// <summary>
            /// Превышено время записи.
            /// </summary>
            [Description("Превышено время записи.")]
            WriteTimedOut,

            /// <summary>
            /// Устройство в ожидании.
            /// </summary>
            [Description("Устройство в ожидании.")]
            IoPending,

            /// <summary>
            /// Устройство не найдено.
            /// </summary>
            [Description("Устройство не найдено")] DeviceNotFound = 0xFF
        }


        // GetDeviceVersion() return codes
        public const byte Cp2101Version = 0x01;

        public const byte Cp2102Version = 0x02;
        public const byte Cp2103Version = 0x03;
        public const byte Cp2104Version = 0x04;


        public const byte FirmwareControlled = 0x02;

        // Mask and Latch value bit definitions
        public const byte Gpio0 = 0x01;

        public const byte Gpio1 = 0x02;
        public const byte Gpio2 = 0x04;
        public const byte Gpio3 = 0x08;
        public const byte HandshakeLine = 0x01;

        public const byte HeldActive = 0x01;

        // Input and Output pin Characteristics
        public const byte HeldInactive = 0x00;

        /// <summary>
        /// размер считываемого пакета
        /// </summary>
        public const uint InPackageSize = 13;

        // Buffer size limits
        private const int MaxDeviceStrlen = 256;
        private const int MaxReadSize = 4096 * 16;
        private const int MaxWriteSize = 4096;


        // RX Queue status flags
        public const byte RxNoOverrun = 0x00;
        public const byte RxOverrun = 0x01;
        public const byte RxReady = 0x02;
        public const byte StatusInput = 0x00;

        /// <summary>
        /// смещение байта статуса в пакете
        /// </summary>
        public const uint StatusOffset = 0;

        public const byte TransmitActiveSignal = 0x03;

        /// <summary>
        /// флаг питания датчика
        /// </summary>
        public const byte VccFlag = 0x40;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private IntPtr _handle = IntPtr.Zero;

        #endregion

        #region Property

        /// <summary>
        /// Предоставляет описание всех подключенных устройств
        /// </summary>
        public static Product[] FindAllDevice
        {
            get
            {
                var poduct = new List<Product>();
                for (var i = 0; i < GetCountDevices(); i++)
                {
                        poduct.Add(GetProduct(i));
                        poduct[i].Number = i;
                }

                return poduct.ToArray();
            }
        }

        public IntPtr Handle
        {
            get => _handle;
            protected set => _handle = value;
        }

        /// <summary>
        /// Предаставлет сведенья об устройстве.
        /// </summary>
        public Product ProductInfo { get; protected set; }

        /// <summary>
        /// Получает и устанавливает знаечние таймаута на чтение.
        /// </summary>
        public static uint ReadTimeout
        {
            get
            {
                uint writeTimeout = 0;
                uint readTimeout = 0;
                GetTimeouts(ref readTimeout, ref writeTimeout);
                return readTimeout;
            }
            set
            {
                uint writeTimeout = 0;
                uint readTimeout = 0;
                GetTimeouts(ref readTimeout, ref writeTimeout);
                SetTimeouts(value, writeTimeout);
            }
        }

        /// <summary>
        /// Получает и устанавливает знаечние таймаута на запись.
        /// </summary>
        public static uint WriteTimeout
        {
            get
            {
                uint writeTimeout = 0;
                uint readTimeout = 0;
                GetTimeouts(ref readTimeout, ref writeTimeout);
                return writeTimeout;
            }
            set
            {
                uint writeTimeout = 0;
                uint readTimeout = 0;
                GetTimeouts(ref readTimeout, ref writeTimeout);
                SetTimeouts(readTimeout, value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Считывает число подключенных к компютеру устройств.
        /// </summary>
        /// <returns>Число найденых устройств</returns>
        public static int GetCountDevices()
        {
            uint count = 0;
            var code = NativeMethods.SI_GetNumDevices(ref count);
            try
            {
                GenericException(nameof(GetCountDevices), (StatusCode)code);
            }
            catch (Efm32USBEpressException)
            {

            }
           
            return (int) count;
        }

        /// <summary>
        /// Позволяет поулчить описание устройства.
        /// </summary>
        /// <param name = "dwDeviceNum"></param>
        /// <returns></returns>
        public static Product GetProduct(int dwDeviceNum)
        {
            return new Product
            {
                SerialNumber = GetProductString(dwDeviceNum, PropertyUsb.SerialNumber),
                Description = GetProductString(dwDeviceNum, PropertyUsb.Description),
                LinkName = GetProductString(dwDeviceNum, PropertyUsb.LinkName),
                Pid = GetProductString(dwDeviceNum, PropertyUsb.Pid),
                Vid = GetProductString(dwDeviceNum, PropertyUsb.Vid)
            };
        }

        /// <summary>
        /// Считывает строку-идентификатор устройства.
        /// </summary>
        /// <param name = "dwDeviceNum">
        /// Номер устройства (от 0 до значения, полученного вызовом <see cref = "GetCountDevices" />
        /// </param>
        /// <param name = "lpvDeviceString">Указатель, по адресу которого будет записана строка-идентификатор</param>
        /// <param name = "flag">флаги функции</param>
        /// <returns>Статус операции (если прошла успешно, равен <see cref = "StatusCode.Success" />)</returns>
        private static string GetProductString(int dwDeviceNum, PropertyUsb flag)
        {
            if (dwDeviceNum < 0) throw new ArgumentOutOfRangeException("Адрес устройства не может быть меньше 0");
            var array = new byte[MaxDeviceStrlen];
            var code = NativeMethods.SI_GetProductString((uint) dwDeviceNum, array, (uint) flag);
            GenericException(nameof(GetProductString), (StatusCode) code);
            var list = array.ToList();
            list.RemoveAll(q => q == 0);
            return Encoding.ASCII.GetString(list.ToArray());
        }

        private static void GenericException(string functionName, StatusCode code)
        {
            switch (code)
            {
                case StatusCode.Success:
                    return;
                case StatusCode.InvalidHandle:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.ReadError:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.RxQueueNotReady:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.WriteError:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.ResetError:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.InvalidParameter:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.InvalidRequestLength:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.DeviceIoFailed:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.InvalidBaudrate:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.FunctionNotSupported:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.GlobalDataError:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.SystemErrorCode:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.ReadTimedOut:
                    throw new IOTimeoutException {Code = code, Source = functionName};
                case StatusCode.WriteTimedOut:
                    throw new IOTimeoutException {Code = code, Source = functionName};
                case StatusCode.IoPending:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                case StatusCode.DeviceNotFound:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
                default:
                    throw new Efm32USBEpressException {Code = code, Source = functionName};
            }
        }

        public static StatusCode DeviceIoControl(IntPtr cyHandle, uint dwIoControlCode, byte[] lpInBuffer,
            uint dwBytesToRead, byte[] lpOutBuffer, uint dwBytesToWrite, ref uint lpdwBytesSucceeded)
        {
            return (StatusCode) NativeMethods.SI_DeviceIOControl(cyHandle, dwIoControlCode, lpInBuffer, dwBytesToRead,
                                                                 lpOutBuffer, dwBytesToWrite, ref lpdwBytesSucceeded);
        }

        /// <summary>
        /// Устанавливает таймауты на запись и чтение.
        /// </summary>
        /// <param name = "readTimeout">Таймаут на чтение в милисекундах</param>
        /// <param name = "writeTimeout">Таймаут на запись в милисекундах,</param>
        public static void SetTimeouts(uint readTimeout, uint writeTimeout)
        {
            var code = NativeMethods.SI_SetTimeouts(readTimeout, writeTimeout);
            GenericException(nameof(GetProductString), (StatusCode) code);
            Logger.Debug($@"Установлен таймаут для чтения {readTimeout} и для записи {writeTimeout}.");
        }

        /// <summary>
        /// Считывает таймауты на запись и чтение.
        /// </summary>
        /// <param name = "lpdwReadTimeout">указатель, по адресу которого будет записан таймаут на чтение в милисекундах</param>
        /// <param name = "lpdwWriteTimeout">указатель, по адресу которого будет записан таймаут на запись в милисекундах</param>
        /// <returns>Статус операции (если прошла успешно, равен <see cref = "StatusCode.Success" />)</returns>
        private static void GetTimeouts(ref uint lpdwReadTimeout, ref uint lpdwWriteTimeout)
        {
            var code = NativeMethods.SI_GetTimeouts(ref lpdwReadTimeout, ref lpdwWriteTimeout);
            GenericException(nameof(GetProductString), (StatusCode) code);
        }

        /// <summary>
        /// Проверка количества байтов в устройстве
        /// </summary>
        /// <param name = "cyHandle"></param>
        /// <param name = "lpdwNumBytesInQueue"></param>
        /// <param name = "lpdwQueueStatus"></param>
        /// <returns></returns>
        public static StatusCode CheckRxQueue(IntPtr cyHandle, ref uint lpdwNumBytesInQueue, ref uint lpdwQueueStatus)
        {
            return (StatusCode) NativeMethods.SI_CheckRXQueue(cyHandle, ref lpdwNumBytesInQueue, ref lpdwQueueStatus);
        }

        public static void SetBaudRate(IntPtr cyHandle, uint dwBaudRate)
        {
            var code = NativeMethods.SI_SetBaudRate(cyHandle, dwBaudRate);
            GenericException(nameof(GetProductString), (StatusCode) code);
        }

        /// <summary>
        /// установка дилителя скорости передачи данных
        /// </summary>
        /// <param name = "cyHandle"></param>
        /// <param name = "wBaudDivisor"></param>
        /// <returns></returns>
        public static void SetBaudDivisor(IntPtr cyHandle, ushort wBaudDivisor)
        {
            var code = NativeMethods.SI_SetBaudDivisor(cyHandle, wBaudDivisor);
            GenericException(nameof(GetProductString), (StatusCode) code);
        }

        public static StatusCode SetLineControl(IntPtr cyHandle, ushort wLineControl)
        {
            return (StatusCode) NativeMethods.SI_SetLineControl(cyHandle, wLineControl);
        }

        public static StatusCode GetModemStatus(IntPtr cyHandle, ref byte modemStatus)
        {
            return (StatusCode) NativeMethods.SI_GetModemStatus(cyHandle, ref modemStatus);
        }

        public static StatusCode SetFlowControl(IntPtr cyHandle, byte bCtsMaskCode, byte bRtsMaskCode,
            byte bDtrMaskCode, byte bDsrMaskCode, byte bDcdMaskCode, bool bFlowXonXoff)
        {
            return (StatusCode) NativeMethods.SI_SetFlowControl(cyHandle, bCtsMaskCode, bRtsMaskCode, bDtrMaskCode,
                                                                bDsrMaskCode, bDcdMaskCode, bFlowXonXoff);
        }

        public static StatusCode SetBreak(IntPtr cyHandle, ushort wBreakState)
        {
            return (StatusCode) NativeMethods.SI_SetBreak(cyHandle, wBreakState);
        }

        public static StatusCode ReadLatch(IntPtr cyHandle, ref byte lpbLatch)
        {
            return (StatusCode) NativeMethods.SI_ReadLatch(cyHandle, ref lpbLatch);
        }

        public static StatusCode WriteLatch(IntPtr cyHandle, byte bMask, byte bLatch)
        {
            return (StatusCode) NativeMethods.SI_WriteLatch(cyHandle, bMask, bLatch);
        }

        public static StatusCode GetPartNumber(IntPtr cyHandle, ref byte lpbPartNum)
        {
            return (StatusCode) NativeMethods.SI_GetPartNumber(cyHandle, ref lpbPartNum);
        }

        /// <summary>
        /// Позволяет поучить версию DLL
        /// </summary>
        /// <returns></returns>
        public static Version GetDllVersion()
        {
            uint highVersion = 0, lowVersion = 0;
            var code = NativeMethods.SI_GetDLLVersion(ref highVersion, ref lowVersion);
            GenericException(nameof(GetProductString), (StatusCode) code);
            return
                Version.Parse($"{(highVersion >> 16) & 0xffff}.{highVersion & 0xffff}.{(lowVersion >> 16) & 0xffff}.{lowVersion & 0xffff}");
        }

        /// <summary>
        /// Позволяет получить версию устройства.
        /// </summary>
        /// <returns></returns>
        public static Version GetDriverVersion()
        {
            uint highVersion = 0, lowVersion = 0;
            var code = NativeMethods.SI_GetDriverVersion(ref highVersion, ref lowVersion);
            GenericException(nameof(GetProductString), (StatusCode) code);
            return
                Version.Parse($"{(highVersion >> 16) & 0xffff}.{highVersion & 0xffff}.{(lowVersion >> 16) & 0xffff}.{lowVersion & 0xffff}");
        }

        /// <summary>
        /// Освобождает ресурсы устройства.
        /// </summary>
        public void Close()
        {
            var code = NativeMethods.SI_Close(Handle);
            GenericException(nameof(GetProductString), (StatusCode) code);
            Handle = IntPtr.Zero;
        }

        /// <summary>
        /// Ошичат входной и выходной буффер
        /// </summary>
        /// <param name = "clearTansmit"></param>
        /// <param name = "clearReceive"></param>
        public void FlushBuffers(bool clearTansmit, bool clearReceive)
        {
            if (Handle == IntPtr.Zero) return;
            var code = NativeMethods.SI_FlushBuffers(Handle, Convert.ToByte(clearTansmit),
                                                     Convert.ToByte(clearReceive));
            GenericException(nameof(GetProductString), (StatusCode) code);
        }

        /// <summary>
        /// Захватывает ресурс для работы с устройством.
        /// </summary>
        /// <param name = "dwDevice">
        /// Номер устройства (от 0 до значения, полученного вызовом <see cref = "GetCountDevices" />
        /// </param>
        public void Open(int dwDevice)
        {
            if (dwDevice < 0) throw new ArgumentOutOfRangeException("Адрес устройства не может быть меньше 0");
            if (Handle != IntPtr.Zero)
            {
                Logger.Debug("Порт уже открыт");
                return;
            }

            var code = NativeMethods.SI_Open((uint) dwDevice, ref _handle);
            GenericException(nameof(GetProductString), (StatusCode) code);
            ProductInfo = GetProduct(dwDevice);
        }

        /// <summary>
        /// Считывает данные устройства в заданный буфер.
        /// </summary>
        /// <param name = "dwBytesToRead">число байт для считывания</param>
        /// <param name = "lpdwBytesReturned">указатель на число фактически считанных байт</param>
        /// <param name = "o">структура Win32 API для асинхронного считывания/записи данных (см. документацию Win32 API)</param>
        public byte[] Read(uint dwBytesToRead, ref uint lpdwBytesReturned, IntPtr o)
        {
            if (dwBytesToRead > MaxReadSize)
                throw new ArgumentOutOfRangeException($"Считывание возможно не более {MaxReadSize} байт.");
            var lpBuffer = new byte[dwBytesToRead];
            var code = NativeMethods.SI_Read(Handle, lpBuffer, dwBytesToRead, ref lpdwBytesReturned, o);
            GenericException(nameof(GetProductString), (StatusCode) code);
            return lpBuffer;
        }

        /// <summary>
        /// Записывает данные из буфера  в устройство.
        /// </summary>
        /// <param name = "lpBuffer">указатель на буфер, из которого будут записаны данные</param>
        /// <param name = "lpdwBytesWritten">указатель на число фактически записанных байт</param>
        /// <param name = "o">структура Win32 API для асинхронного считывания/записи данных (см. документацию Win32 API)</param>
        public void Write(byte[] lpBuffer, ref uint lpdwBytesWritten, IntPtr o)
        {
            if (lpBuffer.Length > MaxWriteSize)
                throw new ArgumentOutOfRangeException($"Запись возможна не более {MaxWriteSize} байт.");
            foreach (var buff in lpBuffer) Write(buff, ref lpdwBytesWritten, o);
        }

        /// <summary>
        /// Записывает данные из буфера  в устройство.
        /// </summary>
        /// <param name = "lpBuffer">указатель на буфер, из которого будут записаны данные</param>
        /// <param name = "lpdwBytesWritten">указатель на число фактически записанных байт</param>
        /// <param name = "o">структура Win32 API для асинхронного считывания/записи данных (см. документацию Win32 API)</param>
        public void Write(byte lpBuffer, ref uint lpdwBytesWritten, IntPtr o)
        {
            var code = NativeMethods.SI_Write(Handle, new[] {lpBuffer}, 1, ref lpdwBytesWritten, o);
            GenericException(nameof(GetProductString), (StatusCode) code);
        }

        #endregion


        /// <summary>
        /// Параметры USB Устройства.
        /// </summary>
        protected enum PropertyUsb
        {
            /// <summary>
            /// Заводской номер.
            /// </summary>
            SerialNumber,

            /// <summary>
            /// Описание устройства.
            /// </summary>
            Description,

            /// <summary>
            /// Ссылка устройства.
            /// </summary>
            LinkName,

            /// <summary>
            /// Вид устройства.
            /// </summary>
            Vid,

            /// <summary>
            /// Пид устройства.
            /// </summary>
            Pid
        }

        public class Product
        {
            #region Property

            public string Description { get; set; }
            public string LinkName { get; set; }
            public int Number { get; set; }
            public string Pid { get; set; }
            public string SerialNumber { get; set; }
            public string Vid { get; set; }

            #endregion
        }
    }
}
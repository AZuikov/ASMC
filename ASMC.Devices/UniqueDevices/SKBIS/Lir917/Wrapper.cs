using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.UniqueDevices.SKBIS.Lir917
{
    public class NativeMethods
    {
        private const string SiUSBXp = "SiUSBXp.dll";
        [DllImport(SiUSBXp)]
        public static extern int SI_GetNumDevices(
        ref uint lpdwNumDevices
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_GetProductString(
        uint dwDeviceNum,
        StringBuilder lpvDeviceString,
        uint dwFlags
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_Open(
        uint dwDevice,
        ref IntPtr cyHandle
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_Close(
        IntPtr cyHandle
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_Read(
        IntPtr cyHandle,
        byte[] lpBuffer,
        uint dwBytesToRead,
        ref uint lpdwBytesReturned,
        IntPtr o
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_Write(
        IntPtr cyHandle,
        byte[] lpBuffer,
        uint dwBytesToWrite,
        ref uint lpdwBytesWritten,
        IntPtr o
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_DeviceIOControl(
        IntPtr cyHandle,
        uint dwIoControlCode,
        byte[] lpInBuffer,
        uint dwBytesToRead,
        byte[] lpOutBuffer,
        uint dwBytesToWrite,
        ref uint lpdwBytesSucceeded
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_FlushBuffers(
        IntPtr cyHandle,
        byte flushTransmit,
        byte flushReceive
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_SetTimeouts(
        uint dwReadTimeout,
        uint dwWriteTimeout
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_GetTimeouts(
        ref uint lpdwReadTimeout,
        ref uint lpdwWriteTimeout
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_CheckRXQueue(
        IntPtr cyHandle,
        ref uint lpdwNumBytesInQueue,
        ref uint lpdwQueueStatus
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_SetBaudRate(
        IntPtr cyHandle,
        uint dwBaudRate
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_SetBaudDivisor(
        IntPtr cyHandle,
        ushort wBaudDivisor
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_SetLineControl(
        IntPtr cyHandle,
        ushort wLineControl
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_SetFlowControl(
        IntPtr cyHandle,
        byte bCtsMaskCode,
        byte bRtsMaskCode,
        byte bDtrMaskCode,
        byte bDsrMaskCode,
        byte bDcdMaskCode,
        bool bFlowXonXoff
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_GetModemStatus(
        IntPtr cyHandle,
        ref byte modemStatus
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_SetBreak(
        IntPtr cyHandle,
        ushort wBreakState
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_ReadLatch(
        IntPtr cyHandle,
        ref byte lpbLatch
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_WriteLatch(
        IntPtr cyHandle,
        byte bMask,
        byte bLatch
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_GetPartNumber(
        IntPtr cyHandle,
        ref byte lpbPartNum
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_GetDeviceProductString(
        IntPtr cyHandle,
        byte[] lpProduct,
        ref byte lpbLength,
        bool bConvertToAscii
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_GetDLLVersion(
        ref uint highVersion,
        ref uint lowVersion
        );

        [DllImport(SiUSBXp)]
        public static extern int SI_GetDriverVersion(
        ref uint highVersion,
        ref uint lowVersion
        );
    }
    public static class Wrapper
    {
        /// <summary>
        /// размер считываемого пакета
        /// </summary>
        public const uint InPackageSize = 13;
        /// <summary>
        /// размер передаваемого пакета
        /// </summary>
        public const uint OutPackageSize = 1;
        /// <summary>
        /// флаг захвата референтной метки
        /// </summary>
        public const byte RefMarkFlag = 0x80;
        /// <summary>
        /// флаг питания датчика
        /// </summary>
        public const byte VccFlag = 0x40;
        /// <summary>
        /// команда запроса пакета
        /// </summary>
        public const byte PackageRequest = 0x33;
        /// <summary>
        /// команда сброса абсолютной координаты
        /// </summary>
        public const byte DropAbsCoordinate = 0x31;
        /// <summary>
        /// команда сброса относительной координаты
        /// </summary>
        public const byte DropRelCoordinate = 0x30;
        /// <summary>
        /// номер абсолютной координаты в пакете
        /// </summary>
        public const int AbsCoordNum = 1;
        /// <summary>
        /// номер относительной координаты в пакете
        /// </summary>
        public const int RelCoordNum = 5;
        /// <summary>
        /// номер координаты референтой метки в пакете
        /// </summary>
        public const int RefCoordNum = 9;
        /// <summary>
        /// смещение байта статуса в пакете
        /// </summary>
        public const uint StatusOffset = 0;

        // Return codes
        public const byte SiSuccess = 0x00;
        public const byte SiDeviceNotFound = 0xFF;
        public const byte SiInvalidHandle = 0x01;
        public const byte SiReadError = 0x02;
        public const byte SiRxQueueNotReady = 0x03;
        public const byte SiWriteError = 0x04;
        public const byte SiResetError = 0x05;
        public const byte SiInvalidParameter = 0x06;
        public const byte SiInvalidRequestLength = 0x07;
        public const byte SiDeviceIoFailed = 0x08;
        public const byte SiInvalidBaudrate = 0x09;
        public const byte SiFunctionNotSupported = 0x0a;
        public const byte SiGlobalDataError = 0x0b;
        public const byte SiSystemErrorCode = 0x0c;
        public const byte SiReadTimedOut = 0x0d;
        public const byte SiWriteTimedOut = 0x0e;
        public const byte SiIoPending = 0x0f;

        // GetProductString() function flags
        public const byte SiReturnSerialNumber = 0x00;
        public const byte SiReturnDescription = 0x01;
        public const byte SiReturnLinkName = 0x02;
        public const byte SiReturnVid = 0x03;
        public const byte SiReturnPid = 0x04;

        // RX Queue status flags
        public const byte SiRxNoOverrun = 0x00;
        public const byte SiRxEmpty = 0x00;
        public const byte SiRxOverrun = 0x01;
        public const byte SiRxReady = 0x02;

        // Buffer size limits
        public const int SiMaxDeviceStrlen = 256;
        public const int SiMaxReadSize = 4096 * 16;
        public const int SiMaxWriteSize = 4096;

        // Input and Output pin Characteristics
        public const byte SiHeldInactive = 0x00;
        public const byte SiHeldActive = 0x01;
        public const byte SiFirmwareControlled = 0x02;
        public const byte SiReceiveFlowControl = 0x02;
        public const byte SiTransmitActiveSignal = 0x03;
        public const byte SiStatusInput = 0x00;
        public const byte SiHandshakeLine = 0x01;

        // Mask and Latch value bit definitions
        public const byte SiGpio0 = 0x01;
        public const byte SiGpio1 = 0x02;
        public const byte SiGpio2 = 0x04;
        public const byte SiGpio3 = 0x08;

        // GetDeviceVersion() return codes
        public const byte SiCp2101Version = 0x01;
        public const byte SiCp2102Version = 0x02;
        public const byte SiCp2103Version = 0x03;
        public const byte SiCp2104Version = 0x04;

        public static int SI_GetNumDevices(ref uint lpdwNumDevices)
        {
            return NativeMethods.SI_GetNumDevices(ref lpdwNumDevices);
        }
        public static int SI_GetProductString(uint dwDeviceNum, StringBuilder lpvDeviceString, uint dwFlags)
        {
            return NativeMethods.SI_GetProductString(dwDeviceNum, lpvDeviceString, dwFlags);
        }

        public static int SI_Open(uint dwDevice, ref IntPtr cyHandle)
        {
            return NativeMethods.SI_Open(dwDevice, ref cyHandle);
        }
        public static int SI_Close(IntPtr cyHandle)
        {
            return NativeMethods.SI_Close(cyHandle);
        }

        public static int SI_Read(IntPtr cyHandle, byte[] lpBuffer, uint dwBytesToRead, ref uint lpdwBytesReturned, IntPtr o)
        {
            return NativeMethods.SI_Read(cyHandle, lpBuffer, dwBytesToRead, ref lpdwBytesReturned, o);
        }

        public static int SI_Write(IntPtr cyHandle, byte[] lpBuffer, uint dwBytesToWrite, ref uint lpdwBytesWritten, IntPtr o)
        {
            return NativeMethods.SI_Write(cyHandle, lpBuffer, dwBytesToWrite, ref lpdwBytesWritten, o);
        }

        public static int SI_DeviceIOControl(IntPtr cyHandle, uint dwIoControlCode, byte[] lpInBuffer, uint dwBytesToRead, byte[] lpOutBuffer, uint dwBytesToWrite, ref uint lpdwBytesSucceeded)
        {
            return NativeMethods.SI_DeviceIOControl(cyHandle, dwIoControlCode, lpInBuffer, dwBytesToRead, lpOutBuffer, dwBytesToWrite, ref lpdwBytesSucceeded);
        }

        public static int SI_FlushBuffers(IntPtr cyHandle, byte flushTransmit, byte flushReceive)
        {
            return NativeMethods.SI_FlushBuffers(cyHandle, flushTransmit, flushReceive);
        }

        public static int SI_SetTimeouts(uint dwReadTimeout, uint dwWriteTimeout)
        {
            return NativeMethods.SI_SetTimeouts(dwReadTimeout, dwWriteTimeout);
        }

        public static int SI_GetTimeouts(ref uint lpdwReadTimeout, ref uint lpdwWriteTimeout)
        {
            return NativeMethods.SI_GetTimeouts(ref lpdwReadTimeout, ref lpdwWriteTimeout);
        }

        public static int SI_CheckRXQueue(IntPtr cyHandle, ref uint lpdwNumBytesInQueue, ref uint lpdwQueueStatus)
        {
            return NativeMethods.SI_CheckRXQueue(cyHandle, ref lpdwNumBytesInQueue, ref lpdwQueueStatus);
        }

        public static int SI_SetBaudRate(IntPtr cyHandle, uint dwBaudRate)
        {
            return NativeMethods.SI_SetBaudRate(cyHandle, dwBaudRate);
        }

        public static int SI_SetBaudDivisor(IntPtr cyHandle, ushort wBaudDivisor)
        {
            return NativeMethods.SI_SetBaudDivisor(cyHandle, wBaudDivisor);
        }

        public static int SI_SetLineControl(IntPtr cyHandle, ushort wLineControl)
        {
            return NativeMethods.SI_SetLineControl(cyHandle, wLineControl);
        }

        public static int SI_GetModemStatus(IntPtr cyHandle, ref byte modemStatus)
        {
            return NativeMethods.SI_GetModemStatus(cyHandle, ref modemStatus);
        }

        public static int SI_SetFlowControl(IntPtr cyHandle, byte bCtsMaskCode, byte bRtsMaskCode, byte bDtrMaskCode, byte bDsrMaskCode, byte bDcdMaskCode, bool bFlowXonXoff)
        {
            return NativeMethods.SI_SetFlowControl(cyHandle, bCtsMaskCode, bRtsMaskCode, bDtrMaskCode, bDsrMaskCode, bDcdMaskCode, bFlowXonXoff);
        }

        public static int SI_SetBreak(IntPtr cyHandle, ushort wBreakState)
        {
            return NativeMethods.SI_SetBreak(cyHandle, wBreakState);
        }

        public static int SI_ReadLatch(IntPtr cyHandle, ref byte lpbLatch)
        {
            return NativeMethods.SI_ReadLatch(cyHandle, ref lpbLatch);
        }

        public static int SI_WriteLatch(IntPtr cyHandle, byte bMask, byte bLatch)
        {
            return NativeMethods.SI_WriteLatch(cyHandle, bMask, bLatch);
        }
        public static int SI_GetPartNumber(IntPtr cyHandle, ref byte lpbPartNum)
        {
            return NativeMethods.SI_GetPartNumber(cyHandle, ref lpbPartNum);
        }

        public static int SI_GetDLLVersion(ref uint highVersion, ref uint lowVersion)
        {
            return NativeMethods.SI_GetDLLVersion(ref highVersion, ref highVersion);
        }

        public static int SI_GetDriverVersion(ref uint highVersion, ref uint lowVersion)
        {
            return NativeMethods.SI_GetDriverVersion(ref highVersion, ref highVersion);
        }

        public static int SI_GetDeviceProductString(IntPtr cyHandle, byte[] lpProduct, ref byte lpbLength, bool bConvertToAscii)
        {
            return NativeMethods.SI_GetDeviceProductString(cyHandle, lpProduct, ref lpbLength, bConvertToAscii);
        }

    }
}

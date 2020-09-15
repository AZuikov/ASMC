using System;
using System.Runtime.Serialization;
using Palsys.Utils.Data;

namespace ASMC.Devices.USB_Device.SiliconLabs
{
    [Serializable]
    public class Efm32USBEpressException:Exception
    {
        public UsbExpressWrapper.StatusCode Code
        {
            get; set;
        }

        public string CodeMessage
        {
            get
            {
                string str;
                try
                {
                    str = Code.GetDescription();

                }
                catch 
                {
                    str = "Неожиданное исключение";
                }

                return str;
            }
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Code), this.Code);
        }

        public Efm32USBEpressException()
        {

        }

        public Efm32USBEpressException(string message):base(message)
        {
        }
        public Efm32USBEpressException(string message, Exception inner) : base(message,inner)
        {
        }
        protected Efm32USBEpressException(SerializationInfo inf, StreamingContext context) :base(inf,context)
        {
                this.Code = (UsbExpressWrapper.StatusCode) inf.GetInt32(nameof(Code));
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return $@"Вызвано исключение: с кодом {(int)Code} {CodeMessage} "+ base.ToString();
        }
    }


    [Serializable]
    public class DeviceIoFailedException : Efm32USBEpressException
    {

        public DeviceIoFailedException()
        {
        }

        public DeviceIoFailedException(string message) : base(message)
        {
        }

        public DeviceIoFailedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DeviceIoFailedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class IOTimeoutException : Efm32USBEpressException
    {

        public IOTimeoutException()
        {
        }

        public IOTimeoutException(string message) : base(message)
        {
        }

        public IOTimeoutException(string message, Exception inner) : base(message, inner)
        {
        }

        protected IOTimeoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

}

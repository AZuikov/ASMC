using System;
using System.Runtime.Serialization;
using ASMC.Devices.OWEN;
using Palsys.Utils.Data;

namespace ASMC.Devices.Port.OWEN
{
    [Serializable]
    public class TrmException:Exception
    {
        public TRM202Device.TrmError Code
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

        public TrmException()
        {

        }

       
        public TrmException(string message, Exception inner) : base(message,inner)
        {
        }
        protected TrmException(SerializationInfo inf, StreamingContext context) :base(inf,context)
        {
            this.Code = (TRM202Device.TrmError) inf.GetInt32(nameof(Code));
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return $@"Вызвано исключение: с кодом {(int)Code} {CodeMessage} "+ base.ToString();
        }
    }
}
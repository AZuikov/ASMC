using ASMC.Common;
using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;

namespace ASMC.Data.Model
{
    public abstract class Program : IProgram
    {
        protected Program(ServicePack service)
        {
            Service = service;
        }
        /// <inheritdoc />
        public string Type { get; protected set; }

        /// <inheritdoc />
        public string Grsi { get; protected set; } 
        /// <inheritdoc />
        public string Range { get; protected set; } 
        /// <inheritdoc />
        public string Accuracy { get; protected set; } 
       
        /// <inheritdoc />
        public OperationMetrControlBase Operation { get; protected set; }
       
        public ServicePack Service { get; }
    }
    public class ServicePack
    {
        /// <summary>
        /// Позволяет получать или задавать сервис сообщения.
        /// </summary>
       public IMessageBoxService MessageBox { get; set; }
        /// <summary>
        /// Позволяет получать или задавать сервис схемы.
        /// </summary>
        public IFormService ShemForm { get; set; }
        /// <summary>
        /// Позволяет получать или задавать сервис диалога опробования или внешнего осмотра.
        /// </summary>
        public IFormService TestingDialog { get; set; }
    }
}
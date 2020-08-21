using ASMC.Common;
using DevExpress.Mvvm;

namespace ASMC.Core.Model
{
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
        public IFormService QuestionText { get; set; }
    }
}
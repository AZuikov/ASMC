using System;
using DevExpress.Mvvm;

namespace ASMC.Core.Model
{
    public class ServicePack
    {
        /// <summary>
        /// Позволяет получать или задавать сервис сообщения.
        /// </summary>
        public Func<IMessageBoxService> MessageBox { get; set; }
        /// <summary>
        /// Позволяет получать или задавать сервис схемы.
        /// </summary>
        public Func<IFormService> ShemForm { get; set; }
        /// <summary>
        /// Позволяет получать или задавать сервис диалога опробования или внешнего осмотра.
        /// </summary>
        public Func<IFormService> QuestionText { get; set; }

        public Func<IWindowService> FreeWindow { get; set; }
    }
}
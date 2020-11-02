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
        public Func<ISelectionService> ShemForm { get; set; }
        /// <summary>
        /// Позволяет получать или задавать сервис диалога опробования или внешнего осмотра.
        /// </summary>
        public Func<ISelectionService> QuestionText { get; set; }

        public Func<ISelectionService> FreeWindow { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ASMC.Core
{
    /// <summary>
    /// Представляет результат
    /// пользовательского выбора
    /// в расширенном диалоге.
    /// </summary>
    public enum TaskMessageResult
    {
        /// <summary>
        /// Нет результата.
        /// </summary>
        None = 0,
        /// <summary>
        /// Задает ОК.
        /// </summary>
        Ok = 1,
        /// <summary>
        /// Задает Да.
        /// </summary>
        Yes = 2,
        /// <summary>
        /// Задает Нет.
        /// </summary>
        No = 4,
        /// <summary>
        /// Задает Отмена.
        /// </summary>
        Cancel = 8,
        /// <summary>
        /// Задает Повтор.
        /// </summary>
        Retry = 16,
        /// <summary>
        /// Задает Закрыть.
        /// </summary>
        Close = 32,
        /// <summary>
        /// Задает иной выбор.
        /// </summary>
        CustomButtonClicked = 256
    }
    /// <summary>
    /// Представляет стандартные
    /// кнопки пользовательского выбора
    /// в расширенном диалоге.
    /// </summary>
    [Flags]
    public enum TaskMessageButton
    {
        None = 0,
        Ok = 1,
        Yes = 2,
        No = 4,
        Cancel = 8,
        Retry = 16,
        Close = 32
    }
    /// <summary>
    /// Представляет иконку в
    /// расширенном диалоге.
    /// </summary>
    public enum TaskMessageIcon
    {
        None = 0,
        Shield = 65532,
        Information = 65533,
        Error = 65534,
        Warning = 65535
    }
    public class TaskMessageClosingEventArgs : CancelEventArgs
    {
        public TaskMessageResult Result
        {
            get;
        }

        public string ButtonName
        {
            get;
        }

        public TaskMessageClosingEventArgs(TaskMessageResult result, string buttonName, bool cancel)
            : base(cancel)
        {
            Result = result;
            ButtonName = buttonName;
        }
    }
    /// <summary>
    /// Представляет сервис для
    /// расширенного информационного
    /// диалога.
    /// </summary>
    public interface ITaskMessageService
    {
        string InstructionText
        {
            get; set;
        }

        string FooterText
        {
            get; set;
        }

        TaskMessageIcon FooterIcon
        {
            get; set;
        }

        string FooterCheckBoxText
        {
            get; set;
        }

        bool? FooterCheckBoxChecked
        {
            get; set;
        }

        IEnumerable<TaskDialogControl> Controls
        {
            get; set;
        }

        TaskMessageResult Show(string text, string caption, TaskMessageButton buttons, TaskMessageIcon icon);

        void Close();
    }
}

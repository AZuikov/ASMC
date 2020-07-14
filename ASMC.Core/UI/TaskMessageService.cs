using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using Microsoft.WindowsAPICodePack.Dialogs;
using Application = System.Windows.Application;

namespace ASMC.Core.UI
{
    /// <summary>
    /// Представляет реализацию сервиса
    /// расширенного информационного диалога.
    /// </summary>
    public class TaskMessageService : ServiceBase, ITaskMessageService
    {
        public static readonly DependencyProperty InstructionTextProperty =
            DependencyProperty.Register(
                nameof(InstructionText),
                typeof(string),
                typeof(TaskMessageService));

        public static readonly DependencyProperty FooterTextProperty =
            DependencyProperty.Register(
                nameof(FooterText),
                typeof(string),
                typeof(TaskMessageService));

        public static readonly DependencyProperty FooterIconProperty =
            DependencyProperty.Register(
                nameof(FooterIcon),
                typeof(TaskMessageIcon),
                typeof(TaskMessageService));

        public static readonly DependencyProperty FooterCheckBoxTextProperty =
            DependencyProperty.Register(
                nameof(FooterCheckBoxText),
                typeof(string),
                typeof(TaskMessageService));

        public static readonly DependencyProperty FooterCheckBoxCheckedProperty =
            DependencyProperty.Register(
                nameof(FooterCheckBoxChecked),
                typeof(bool?),
                typeof(TaskMessageService),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ControlsProperty =
            DependencyProperty.Register(
                nameof(Controls),
                typeof(IEnumerable<TaskDialogControl>),
                typeof(TaskMessageService));

        private TaskDialog _taskDialog;

        public string InstructionText
        {
            get => (string)GetValue(InstructionTextProperty);
            set => SetValue(InstructionTextProperty, value);
        }

        public string FooterText
        {
            get => (string)GetValue(FooterTextProperty);
            set => SetValue(FooterTextProperty, value);
        }

        public TaskMessageIcon FooterIcon
        {
            get => (TaskMessageIcon)GetValue(FooterIconProperty);
            set => SetValue(FooterIconProperty, value);
        }

        public string FooterCheckBoxText
        {
            get => (string)GetValue(FooterCheckBoxTextProperty);
            set => SetValue(FooterCheckBoxTextProperty, value);
        }

        public bool? FooterCheckBoxChecked
        {
            get => (bool)GetValue(FooterCheckBoxCheckedProperty);
            set => SetValue(FooterCheckBoxCheckedProperty, value);
        }

        public IEnumerable<TaskDialogControl> Controls
        {
            get => (IEnumerable<TaskDialogControl>)GetValue(ControlsProperty);
            set => SetValue(ControlsProperty, value);
        }

        public TaskMessageResult Show(string text, string caption, TaskMessageButton buttons, TaskMessageIcon icon)
        {
            if(!CheckAccess())
                return TaskMessageResult.None;

            var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            try
            {
                var td = _taskDialog = new TaskDialog();

                td.Text = text;
                td.InstructionText = InstructionText;
                td.Caption = caption ?? owner?.Title;
                td.Icon = ConvertToTaskDialogStandardIcon(icon);
                td.OwnerWindowHandle = owner != null ? new WindowInteropHelper(owner).Handle : IntPtr.Zero;
                td.StandardButtons = ConvertToTaskDialogStandardButtons(buttons);
                td.FooterCheckBoxText = FooterCheckBoxText;
                td.FooterCheckBoxChecked = FooterCheckBoxChecked;
                td.FooterText = FooterText;
                td.FooterIcon = ConvertToTaskDialogStandardIcon(FooterIcon);
                td.Cancelable = true;

                if(Controls != null)
                {
                    foreach(var ctl in Controls)
                        td.Controls.Add(ctl);
                }
                else
                    td.Controls.Clear();

                var ret = td.Show();

                FooterCheckBoxChecked = td.FooterCheckBoxChecked;

                return ConvertToTaskMessageResult(ret);
            }
            finally
            {
                foreach(var ctl in _taskDialog.Controls)
                    ctl.HostingDialog = null;

                _taskDialog.Dispose();
                _taskDialog = null;
            }
        }

        public void Close()
        {
            _taskDialog?.Close();
        }

        private TaskMessageResult ConvertToTaskMessageResult(TaskDialogResult result)
        {
            return (TaskMessageResult)result;
        }

        private TaskDialogStandardButtons ConvertToTaskDialogStandardButtons(TaskMessageButton buttons)
        {
            return (TaskDialogStandardButtons)buttons;
        }

        private TaskDialogStandardIcon ConvertToTaskDialogStandardIcon(TaskMessageIcon icon)
        {
            return (TaskDialogStandardIcon)icon;
        }

    }
}

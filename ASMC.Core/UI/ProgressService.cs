using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ASMC.Core.Interface;
using ASMC.Core.View;
using DevExpress.Mvvm.UI;

namespace ASMC.Core.UI
{
    public class ProgressService : ServiceBase, IProgressService
    {
        private const int TimeRemainingInterval = 5000;

        private readonly Stopwatch _sw = new Stopwatch();

        #region Properties

        public Progress<int> ProgressCallback
        {
            get; set;
        }

        public Progress<string> StatusCallback
        {
            get; set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Отображает диалоговое окно выполнения асинхронной операции
        /// </summary>
        /// <param name="messageText">Строка, содержащая описание асинхронной операции</param>
        /// <param name="caption">PЗаголовок окна</param>
        /// <param name="taskToRun">Задача, которая выполняется асинхронно</param>
        /// <param name="tokenSource">Токен для отмены</param>
        public void Show(string messageText, string caption, Task taskToRun, CancellationTokenSource tokenSource = null)
        {
            var parent = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            var window = new ProgressWindow
            {
                Owner = parent,
                Title = caption ?? (parent?.Title ?? string.Empty),
                AllowCancel = (tokenSource != null),
                PART_Content = { Text = messageText },
                PART_Progress = { IsIndeterminate = (ProgressCallback == null) }
            };

            var task = DoAsync(window, taskToRun, tokenSource, ProgressCallback, StatusCallback);

            if(!task.IsCompleted)
                window.ShowDialog();

            try
            {
                task.Wait();
            }
            catch(Exception e)
            {
                if(e.InnerException != null)
                    throw e.InnerException;

                throw;
            }
        }

        private async Task DoAsync(ProgressWindow window, Task task, CancellationTokenSource tokenSource, Progress<int> progress, Progress<string> status)
        {
            if(task.Status == TaskStatus.Created && !task.IsCompleted)
                task.Start();

            var ps = 0L;

            var cancelHandler = new EventHandler((s, e) =>
            {
                if(window.Cancelling)
                    tokenSource?.Cancel();
            });

            var progressHandler = new EventHandler<int>((s, e) =>
            {
                window.PART_Progress.Value = e;
            });

            var statusHandler = new EventHandler<string>((s, e) => window.PART_Status.Text = e);

            var dpd = DependencyPropertyDescriptor.FromProperty(ProgressWindow.CancellingProperty, typeof(ProgressWindow));
            dpd?.AddValueChanged(window, cancelHandler);

            if(progress != null)
                progress.ProgressChanged += progressHandler;
            if(status != null)
                status.ProgressChanged += statusHandler;

            var timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.ApplicationIdle,
                (s, e) => window.PART_TimeRemaining.Text = GetTimeRemaining((int)window.PART_Progress.Value), window.Dispatcher);

            timer.Start();
            _sw.Start();
            try
            {
                await task;
            }
            finally
            {
                _sw.Stop();
                timer.Stop();

                window.Tag = true;
                window.Close();

                if(progress != null)
                    progress.ProgressChanged -= progressHandler;
                if(status != null)
                    status.ProgressChanged -= statusHandler;

                dpd?.RemoveValueChanged(window, cancelHandler);
            }
        }

        private string GetTimeRemaining(int currentProgress)
        {
            var timeString = string.Empty;

            if(currentProgress <= 0)
                return timeString;

            var duration = TimeSpan.FromMilliseconds((double)_sw.ElapsedMilliseconds / currentProgress * (100 - currentProgress));

            if (duration.Hours > 0)
                timeString = duration.Hours+":" + duration.Minutes;
            else if(duration.Minutes > 0)
                timeString =duration.Minutes.ToString();
            else
                timeString =duration.Seconds.ToString();

            return timeString;
        }

        #endregion
    }
}

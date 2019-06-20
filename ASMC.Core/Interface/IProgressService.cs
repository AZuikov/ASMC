using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ASMC.Core.Interface
{
    public interface IProgressService
    {
        /// <summary>
        /// Возвращает или задает метод, вызываемый сервисом
        /// при изменении прогресса задачи во время выполнения
        /// </summary>
        Progress<int> ProgressCallback
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает метод, вызываемый сервисом
        /// при изменении состояния задачи во время выполнения 
        /// </summary>
        Progress<string> StatusCallback
        {
            get; set;
        }

        void Show(string messageText, string caption, Task taskToRun, CancellationTokenSource tokenSource = null);
    }
}

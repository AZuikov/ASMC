using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Common.UI;
using ASMC.Data.Model;
using DevExpress.Mvvm;

namespace ASMC.Core.Helps
{
    /// <summary>
    /// Класс используется для вывода информации о забракованной точке.
    /// </summary>
    public class TolleranceDialog : ParagraphBase
    {
        public TolleranceDialog(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }

        /// <summary>
        /// Метод отображает сообщение для пользователя, с информацией о текущей точке.
        /// </summary>
        /// <param name="operation">Результаты измерения по текущей операции.</param>
        /// <returns>Вовзращает выбор пользователя (повторять/не повторять измерение).</returns>
        public MessageResult ShowTolleranceDialog(BasicOperationVerefication<decimal> operation) => this.UserItemOperation.ServicePack.MessageBox.Show($"Текущая точка {operation.Expected} не проходит по допуску:\n" +
                                                               $"Минимально допустимое значение {operation.LowerTolerance}\n" +
                                                               $"Максимально допустимое значение {operation.UpperTolerance}\n" +
                                                               $"Допустимое значение погрешности {operation.Error}\n" +
                                                               $"ИЗМЕРЕННОЕ значение {operation.Getting}\n" +
                                                               $"ФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                               "Повторить измерение этой точки?",
                "Информация по текущему измерению", MessageButton.YesNo, MessageIcon.Question, MessageResult.Yes);

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork()
        {
            throw new NotImplementedException();
        }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}

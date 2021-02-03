using System.Data;
using System.Linq;
using System.Threading;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Core.Model;
using ASMC.Core.UI;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.WithoutInterface.HourIndicator.IchGost577;
using ASMC.Devices.WithoutInterface.HourIndicator.MC52415_13;

namespace DialIndicatorSeries801
{
    /// <summary>
    ///     Придоставляет базувую реализацию для пунктов поверки
    /// </summary>
    /// <typeparam name="TOperation"></typeparam>
    public abstract class OperationBase<TOperation> : ParagraphBase<TOperation>
    {
        #region Property

        /// <summary>
        ///     Предоставляет индикатор частового типа
        /// </summary>
        protected DIMC52415_13 Ich { get; private set; }

        /// <summary>
        /// Предоставляет сервис окна ввода данных.
        /// </summary>
        protected SelectionService Service { get; private set; }

        /// <summary>
        ///     Позволяет получить конец диапазона чисоваого индикатора
        /// </summary>
        protected MeasPoint<Length> EndRange { get; private set; }

        #endregion

        /// <inheritdoc />
        protected OperationBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }


        #region Methods

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name;
        }


        /// <param name="token"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            Ich = ((IControlPannelDevice)UserItemOperation.TestDevices.First().SelectedDevice).Device as DIMC52415_13;
            EndRange = (MeasPoint<Length>)Ich.RangesFull.Ranges.Max().End;
            Service = UserItemOperation.ServicePack.FreeWindow() as SelectionService;
            base.InitWork(token);
        }

        #endregion
    }


    /// <summary>
    ///     Предоставляет реализацию внешнего осномотра.
    /// </summary>
    public sealed class VisualInspection : OperationBase<bool>
    {
        /// <inheritdoc />
        public VisualInspection(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = base.FillData();

            var dataRow = data.NewRow();
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperation<bool>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Getting ? "соответствует требованиям" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.InsetrTextByMark.GetStringValue() + GetType().Name;
        }

        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, "Внешний осмотр"));
        }
    }

    /// <summary>
    ///     Предоставляет операцию опробывания.
    /// </summary>
    public sealed class Testing : OperationBase<bool>
    {
        /// <inheritdoc />
        public Testing(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробывание";
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.InsetrTextByMark.GetStringValue() + GetType().Name;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, "Опробывание"));
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = base.FillData();

            var dataRow = data.NewRow();
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperation<bool>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Getting ? "соответствует требованиям" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }
    }


    public sealed class DeterminationError : OperationBase<MeasPoint<Length>[]>
    {
        /// <inheritdoc />
        public DeterminationError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности на всем диапазоне";
        }
        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Участок индикатора", "0", "20", "40", "60", "80", "100", "На всём диапазоне измерений", "Допуск на всём диапазоне измерений" };
        }
    }

}
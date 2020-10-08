using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.WithoutInterface.HourIndicator;
using DevExpress.Mvvm.UI;
using Indicator_10.ViewModel;
using WindowService = ASMC.Common.UI.WindowService;

namespace Indicator_10
{
    /// <summary>
    /// Измерительное усилие
    /// </summary>
    public class MeasuringForce : ParagraphBase<MeasPoint>
    {
        /// <inheritdoc />
        public MeasuringForce(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение измерительного усилия и его колебания";
        }

        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Expected?.Description;
                dataRow[1] = dds.Getting?.Description;
                dataRow[2] = dds.LowerTolerance?.Description;
                dataRow[3] = dds.UpperTolerance?.Description;
                if (dds.IsGood == null)
                    dataRow[5] = ConstNotUsed;
                else
                    dataRow[5] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override DataColumn[] GetColumnName()
        {
            return new[]
            {
                new DataColumn("Точка диапазона измерений индикатора, мм"),
                new DataColumn("Показания весов, г"),
                new DataColumn("При изменении направления хода изм. стержня"),
                new DataColumn("Колебание при прямом/обратном ходе"),
                new DataColumn("Максимальное при прямом ходе")
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "Определение измерительного усилия и его колебания";
        }

        /// <inheritdoc />
        protected override void InitWork()
        {
            base.InitWork();
            var operation = new BasicOperation<MeasPoint>();
            operation.InitWork = async () =>
            {
                var a = UserItemOperation.ServicePack.FreeWindow as WindowService;
                var vm =
                    new MeasuringForceViewModel(UserItemOperation.TestDevices.FirstOrDefault().SelectedDevice as Ich);
                a.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                a.SizeToContent = SizeToContent.WidthAndHeight;
                a.Show("MeasuringForceView", vm, null, null);
            };
            DataRow.Add(operation);
        }

        #endregion
    }

    public class PerpendicularPressure : ParagraphBase<MeasPoint>
    {
        /// <inheritdoc />
        public PerpendicularPressure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        /// <inheritdoc />
        protected override DataColumn[] GetColumnName()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
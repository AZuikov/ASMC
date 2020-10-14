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
        protected override string[] GenerateDataColumnTypeObject()
        {
            return (string[]) new[]
            {
                "Точка диапазона измерений индикатора, мм",
                "Показания весов, г",
                "Колебание при прямом/обратном ходе",
                "Максимальное при прямом ходе"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray(); ;
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
                var a = UserItemOperation.ServicePack.FreeWindow() as WindowService;
                var vm = new MeasuringForceViewModel(UserItemOperation.TestDevices.FirstOrDefault().SelectedDevice as IchBase);
                a.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                a.SizeToContent = SizeToContent.WidthAndHeight;
                a.Show("MeasuringForceView", vm, null, null);
            };
            DataRow.Add(operation);
        }
        #endregion
    }
    /// <summary>
    /// Определение изменений показаний индикатор при нажиме на измерительный стержень в направлении перпендикулярном его оси.
    /// </summary>
    public class PerpendicularPressure : ParagraphBase<MeasPoint>
    {
        /// <inheritdoc />
        public PerpendicularPressure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return (string[]) new[]
            {
                "№ Измерения", "Изменение показаний индикатора, делений шкалы",
                "Допустимые изменения показаний, делений шкалы"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray(); ;
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    /// <summary>
    /// Определение размаха показаний
    /// </summary>
    public class RangeIndications : ParagraphBase<MeasPoint>
    {
        /// <inheritdoc />
        public RangeIndications(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Определение вариации показаний
    /// </summary>
    public class VariationReading : ParagraphBase<MeasPoint>
    {
        /// <inheritdoc />
        public VariationReading(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            throw new NotImplementedException();
        }
    }
}
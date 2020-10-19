using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using AP.Extension;
using AP.Utils.Data;
using AP.Utils.Helps;
using ASMC.Common.ViewModel;
using ASMC.Core.Model;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.WithoutInterface.HourIndicator;
using DevExpress.Mvvm.UI;
using Indicator_10.ViewModel;
using WindowService = ASMC.Core.UI.WindowService;

namespace Indicator_10
{
    /// <summary>
    /// Измерительное усилие
    /// </summary>
    public class MeasuringForce : ParagraphBase<MeasPoint<Force>>
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
                var dds = row as BasicOperationVerefication<MeasPoint<Force>>;
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
            var operation = new BasicOperation<MeasPoint<Force>>();
            IEnumerable<MeasPoint<Force>> straight = null;
            IEnumerable<MeasPoint<Force>> reverse = null;
            IEnumerable<MeasPoint<Force>> straightReverse = null;
            operation.InitWork = async () =>
            {
                var a = UserItemOperation.ServicePack.FreeWindow() as WindowService;
                var ich = UserItemOperation.TestDevices.First().SelectedDevice as IchBase;
                var arrPoints = ich.Range.GetArayMeasPointsInParcent(0, 50, 100).ToArray();

                arrPoints.ToArray().Reverse();
                arrPoints.Reverse();
                var nameCell = ich.Range.MainPhysicalQuantity.Unit;
                var first = CreateTable("Прямой ход", arrPoints.ToArray(), nameCell);
                var too = CreateTable("Обратный ход", arrPoints.Reverse().ToArray(), nameCell);
                var tree = CreateTable("Прямой/обратный ход", ich.Range.GetArayMeasPointsInParcent(50, 50).ToArray(), nameCell);
                var vm = new MeasuringForceViewModel();
                vm.Content.Add(first);
                vm.Content.Add(too);
                vm.Content.Add(tree);
                a.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                a.SizeToContent = SizeToContent.WidthAndHeight;
                a.Show("MeasuringForceView", vm, null, null);
                straight= Fill(vm.Content[0]);
                reverse= Fill(vm.Content[1]);
                straightReverse=Fill(vm.Content[2]);
               
            };
            operation.BodyWork = () =>
            {
               var val= straight.Max() - straight.Min();
            };
            //operation.CompliteWork = () =>
            //{

            //    return true;
            //}
            DataRow.Add(operation);

            IEnumerable<MeasPoint<Force>> Fill(IItemTable item)
            {
                foreach (var cell in item.Cells)
                {
                    var val = cell.Value.ToString().Trim().Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                    var res =
                        new Weight { Unit = MeasureUnits.Weight, Value = decimal.Parse(val) }.ConvertToForce();
                    yield return new MeasPoint<Force>(res);
                }
            }
            TableViewModel CreateTable(string name, MeasPoint<Length>[] measPoints, MeasureUnits UnitCell)
            {
                var table = new TableViewModel { Header = name };
                for (var i = 0; i < measPoints.Length; i++)
                    table.Cells.Add(new Cell { ColumnIndex = 0, RowIndex = i, Name = string.Join(" ", measPoints[i].MainPhysicalQuantity.Value, UnitCell.GetStringValue()), StringFormat = @"{0} "+ UnitCell.GetStringValue() });
                return table;
            }
        }
        #endregion
    }
    /// <summary>
    /// Определение изменений показаний индикатор при нажиме на измерительный стержень в направлении перпендикулярном его оси.
    /// </summary>
    public class PerpendicularPressure : ParagraphBase<double>
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
    public class RangeIndications : ParagraphBase<MeasPoint<Length>>
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
    public class VariationReading : ParagraphBase<MeasPoint<Length>>
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
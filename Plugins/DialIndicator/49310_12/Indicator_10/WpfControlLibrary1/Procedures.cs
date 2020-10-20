using System;
using System.Collections;
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
    public class MeasuringForce : ParagraphBase<object>
    {
        /// <inheritdoc />
        public MeasuringForce(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение измерительного усилия и его колебания";
            Sheme = new ShemeImage()
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема",
                Number = 1,
                FileName = @"appa_10XN_ma_5522A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };


        }

        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as MultiErrorMeasuringOperation<object>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Expected?.ToString();
                dataRow[1] = dds.Getting?.ToString();
                dataRow[2] = dds.Error[0];
                dataRow[3] = dds.Error[1];
                dataRow[4] = dds.Error[2];
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
            var operation = new MultiErrorMeasuringOperation<object>();

            var ich = UserItemOperation.TestDevices.First().SelectedDevice as IchBase;
            var arrPoints = ich.Range.GetArayMeasPointsInParcent(0, 50, 100).ToArray();
            var arrReversePoint = arrPoints.Reverse().ToArray();
            var arrstraightReversePoint = ich.Range.GetArayMeasPointsInParcent(50, 50).ToArray();
            var fullPoints = arrPoints.Concat(arrReversePoint).Concat(arrstraightReversePoint).ToArray();
            MeasPoint<Weight>[] fullGettingPoints = null;
            IEnumerable<MeasPoint<Force>> fullMeasPoints=null;
            IEnumerable<MeasPoint<Force>> straight = null;
            IEnumerable<MeasPoint<Force>> reverse = null;
            IEnumerable<MeasPoint<Force>> straightReverse = null;
            IEnumerable<MeasPoint<Weight>> straightGetting = null;
            IEnumerable<MeasPoint<Weight>> reverseGetting = null;
            IEnumerable<MeasPoint<Weight>> straightReverseGetting = null;
            
            operation.InitWork = async () =>
            {
                var a = UserItemOperation.ServicePack.FreeWindow() as WindowService;
                arrPoints = ich.Range.GetArayMeasPointsInParcent(0, 50, 100).ToArray();

                arrPoints.ToArray().Reverse();
                arrPoints.Reverse();
                var nameCell = "г";
                var first = CreateTable("Прямой ход", arrPoints, nameCell);
                var too = CreateTable("Обратный ход", arrReversePoint, nameCell);
                var tree = CreateTable("Прямой/обратный ход", arrstraightReversePoint, nameCell);
                var vm = new MeasuringForceViewModel();
                vm.Content.Add(first);
                vm.Content.Add(too);
                vm.Content.Add(tree);
                a.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                a.SizeToContent = SizeToContent.WidthAndHeight;
                a.Show("MeasuringForceView", vm, null, null);
                straightGetting=Fill(vm.Content[0]);
                reverseGetting= Fill(vm.Content[1]);
                straightReverseGetting = Fill(vm.Content[2]);
                straight = ConvertWeightToForce(straightGetting.ToArray());
                reverse = ConvertWeightToForce(reverseGetting.ToArray());
                straightReverse = ConvertWeightToForce(straightReverseGetting.ToArray());
                fullMeasPoints = straight.Concat(reverse);
                fullGettingPoints = straightGetting.Concat(reverseGetting).Concat(straightReverseGetting).ToArray();

            };
  
        
                operation.BodyWork = () =>
                {    
                    for (int i = 0; i < fullPoints.Length-1; i++)
                    {
                        operation.Expected = fullPoints[i].Clone();
                        operation.Getting = fullGettingPoints[i].Clone();
                        operation = (MultiErrorMeasuringOperation<object>) operation.Clone();
                        DataRow.Add(operation);
                    }
                };
                operation.ErrorCalculation = new Func<object, object, object>[]
                {
                    (expected,getting)=>
                    {
                        return fullMeasPoints.Max()- fullMeasPoints.Min();
                    },
                    (expected,getting)=>
                    {
                        return straight.Max()- straight.Min();
                    },
                    (expected,getting)=>
                    {
                        return straightReverse.First()- straightReverse.Last();
                    }
                };
            //operation.CompliteWork = () =>
            //{

            //    return true;
            //}

            DataRow.Add(operation);


            MeasPoint<Force>[] ConvertWeightToForce(MeasPoint<Weight>[] arr)
            {
                return arr.Select(q => new MeasPoint<Force>(((Weight)q.MainPhysicalQuantity)
                                                           .ConvertToForce())).ToArray();
            }

            IEnumerable<MeasPoint<Weight>> Fill(IItemTable item)
            {
                foreach (var cell in item.Cells)
                {
                    var val = cell.Value.ToString().Trim().Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    yield return new MeasPoint<Weight>( new Weight {Unit = MeasureUnits.Weight, Value = decimal.Parse(val)});
                }
            }
            TableViewModel CreateTable(string name, MeasPoint<Length>[] measPoints, string UnitCell)
            {
                var table = new TableViewModel { Header = name };
                for (var i = 0; i < measPoints.Length; i++)
                    table.Cells.Add(new Cell { ColumnIndex = 0, RowIndex = i, Name = measPoints[i].Description, StringFormat = @"{0} "+ UnitCell});
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
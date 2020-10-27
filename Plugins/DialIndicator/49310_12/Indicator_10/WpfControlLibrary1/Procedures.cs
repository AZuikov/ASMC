using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using AP.Extension;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.ViewModel;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.WithoutInterface.HourIndicator;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using Indicator_10.ViewModel;
using WindowService = ASMC.Core.UI.WindowService;

namespace Indicator_10
{
    /// <summary>
    ///     Придоставляет базувую реализацию для пунктов поверки индикаторов частового типа
    ///     <see cref="ASMC.Devices.WithoutInterface.HourIndicator.IchBase" />
    /// </summary>
    /// <typeparam name="TOperation"></typeparam>
    public abstract class MainIchProcedur<TOperation> : ParagraphBase<TOperation>
    {
        protected class SettingTableViewModel
        {
            /// <summary>
            /// Форматирование яческий
            /// </summary>
            public string CellFormat=null;
            /// <summary>
            /// Расположение ячеек горизонтальное
            /// </summary>
            public bool IsHorizontal=true;
            /// <summary>
            /// Рабите ячеек на столцы/строки в зависимости от <see cref="IsHorizontal"/>
            /// </summary>
            public int? Breaking=null;
          
        }
        #region Property

        /// <summary>
        ///     Предоставляет индикатор частового типа
        /// </summary>
        protected IchBase IchBase { get; private set; }

        #endregion

        /// <inheritdoc />
        protected MainIchProcedur(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        /// <summary>
        ///     Создает VM
        /// </summary>
        /// <param name="name">Наименование таблицы</param>
        /// <param name="measPoints">Массив измерительных точек</param>
        /// <param name="UnitCell">отознаечние едениц измерения в ячеках таблицы</param>
        /// <param name="isHorizantal">При значении <see cref="true" /> распологает ячейки горизотально</param>
        /// <returns></returns>
        protected virtual TableViewModel CreateTable(string name, IMeasPoint<Length>[] measPoints, SettingTableViewModel setting)
        {
            var table = new TableViewModel {Header = name};
            var columnIndex = 0;
            var rowIndex = 0;
            foreach (var t in measPoints)
            {
                table.Cells.Add(new Cell
                {
                    ColumnIndex = columnIndex, RowIndex = rowIndex, Name = t.Description,
                    StringFormat = @"{0} " + setting?.CellFormat
                });
                if (setting.IsHorizontal) {
                    columnIndex++;
                    if (setting.Breaking == null) continue;
                    if (columnIndex % setting.Breaking == 0) {rowIndex++;
                        columnIndex = 0;
                    } 
                }
                else
                {
                    rowIndex++;
                    if (setting.Breaking == null) continue;
                    if (rowIndex % setting.Breaking == 0) {columnIndex++;
                        rowIndex = 0;
                    }
                }
            }

            return table;
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name;
        }

        /// <inheritdoc />
        protected override void InitWork()
        {
            IchBase = UserItemOperation.TestDevices.First().SelectedDevice as IchBase;
            base.InitWork();
        }

        /// <summary>
        ///     Приобразует строку в децимал.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected decimal ObjectToDecimal(object obj)
        {
            if (string.IsNullOrEmpty(obj.ToString())) return 0;

            return decimal.Parse(obj.ToString().Trim()
                .Replace(".",
                    Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator));
        }

        #endregion
    }

    /// <summary>
    ///     Измерительное усилие
    /// </summary>
    public sealed class MeasuringForce : MainIchProcedur<object>
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
                var dds = row as MultiErrorMeasuringOperation<object>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Expected?.ToString();
                dataRow[1] = dds.Getting?.ToString();
                dataRow[2] = dds.Error[0];
                dataRow[3] = dds.Error[1];
                dataRow[4] = dds.Error[2];
                //todo Указать погрешность  
                dataRow[5] = IchBase.MeasuringForce.StraightRun;
                dataRow[6] = IchBase.MeasuringForce.Oscillatons;
                dataRow[7] = IchBase.MeasuringForce.ChangeCourse;
                if (dds.IsGood == null)
                    dataRow[8] = ConstNotUsed;
                else
                    dataRow[8] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Точка диапазона измерений индикатора",
                "Показания весов",
                "При изменении направления хода изм. стержня",
                "Колебание при прямом/обратном ходе",
                "Максимальное при прямом ходе",
                "Допуск При изменении направления хода изм. стержня",
                "Допуск Колебание при прямом/ обратном ходе",
                "Допуск Максимальное при прямом ходе"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            ;
        }

        /// <inheritdoc />
        protected override void InitWork()
        {
            base.InitWork();
            var operation = new MultiErrorMeasuringOperation<object>();

            //var arrPoints = IchBase.Range.GetArayMeasPointsInParcent(0, 50, 100).ToArray();
            var maxPoint = IchBase.RangesFull.Ranges.Max().Stop;
            var arrPoints = maxPoint.GetArayMeasPointsInParcent(0, 50, 100).ToArray();

            var arrReversePoint = arrPoints.Reverse().ToArray();
            var arrstraightReversePoint = maxPoint.GetArayMeasPointsInParcent(50, 50).ToArray();
            var fullPoints = arrPoints.Concat(arrReversePoint).Concat(arrstraightReversePoint).ToArray();
            MeasPoint<Weight>[] fullGettingPoints = Array.Empty<MeasPoint<Weight>>();
       

            operation.InitWork = async () =>
            {
                var a = UserItemOperation.ServicePack.FreeWindow() as WindowService;

                var nameCell = "г"; /*Форматирование ячеек в таблице*/
                var setting = new SettingTableViewModel {CellFormat = nameCell};
                var first = CreateTable("Прямой ход", arrPoints, setting);
                var too = CreateTable("Обратный ход", arrReversePoint, setting);
                var tree = CreateTable("Прямой/обратный ход", arrstraightReversePoint, setting);
                var vm = new MeasuringForceViewModel();
                vm.Content.Add(first);
                vm.Content.Add(too);
                vm.Content.Add(tree);
                a.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                a.SizeToContent = SizeToContent.WidthAndHeight;
                a.Show("MeasuringForceView", vm, null, null);
                fullGettingPoints = vm.Content.Aggregate(fullGettingPoints, (current, item) => current.Concat(Fill(item)).ToArray());
            };

            operation.BodyWorkAsync = () =>
            {
                for (var i = 0; i < fullPoints.Length; i++)
                {
                    if (i > 0) operation = (MultiErrorMeasuringOperation<object>) operation.Clone();
                    operation.Expected = fullPoints[i].Clone();
                    operation.Getting = fullGettingPoints[i].Clone();
                    if (i > 0) DataRow.Add(operation);
                }
            };
            //todo Указать погрешность  
            operation.ErrorCalculation = new Func<object, object, object>[]
            {
                (expected, getting) =>
                {
                    var fullMeasPoints = new List<MeasPoint<Weight>>(8);
                    fullMeasPoints.AddRange(DataRow.Select(t => (MeasPoint<Weight>) t.Getting));

                    var fullMeasPointForces = ConvertWeightToForce(fullMeasPoints.ToArray());

                    var array = fullMeasPointForces.Take(6).ToArray();
                    return array.Max() - array.Min();
                },
                (expected, getting) =>
                {
                    var fullMeasPoints = new List<MeasPoint<Weight>>(8);
                    fullMeasPoints.AddRange(DataRow.Select(t => (MeasPoint<Weight>) t.Getting));

                    var fullMeasPointForces = ConvertWeightToForce(fullMeasPoints.ToArray());

                    var array = fullMeasPointForces.Take(3).ToArray();
                    return array.Max() - array.Min();
                },
                (expected, getting) =>
                {
                    var fullMeasPoints = new List<MeasPoint<Weight>>(8);
                    fullMeasPoints.AddRange(DataRow.Select(t => (MeasPoint<Weight>) t.Getting));

                    var fullMeasPointForces = ConvertWeightToForce(fullMeasPoints.ToArray());

                    var array = fullMeasPointForces.Skip(6).Take(2).ToArray();
                    return array.Max() - array.Min();
                }
            };
            operation.CompliteWork = async () =>
                operation.Error[0] as MeasPoint<Force> <= IchBase.MeasuringForce.StraightRun
                &&
                operation.Error[1] as MeasPoint<Force> <= IchBase.MeasuringForce.Oscillatons
                &&
                operation.Error[2] as MeasPoint<Force> <= IchBase.MeasuringForce.Oscillatons;

            DataRow.Add(operation);

            MeasPoint<Force>[] ConvertWeightToForce(MeasPoint<Weight>[] arr)
            {
                return arr.Select(q => new MeasPoint<Force>(q.MainPhysicalQuantity
                    .ConvertToForce())).ToArray();
            }

            IEnumerable<MeasPoint<Weight>> Fill(IItemTable item)
            {
                foreach (var cell in item.Cells)
                    yield return new MeasPoint<Weight>(new Weight
                    {
                        Unit = MeasureUnits.Weight, Value = ObjectToDecimal(cell.Value)
                    });
            }
        }

        #endregion
    }

    /// <summary>
    ///     Определение изменений показаний индикатор при нажиме на измерительный стержень в направлении перпендикулярном его
    ///     оси.
    /// </summary>
    public sealed class PerpendicularPressure : MainIchProcedur<double>
    {
        /// <inheritdoc />
        public PerpendicularPressure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Изменение показания индикатора при нажиме с усилием";
        }

        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            var ich = UserItemOperation.TestDevices.First().SelectedDevice as IchBase;
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as MeasuringOperation<double>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Expected.ToString();
                dataRow[1] = dds.Getting.ToString();
                dataRow[2] = dds.Error;
                if (dds.IsGood == null)
                    dataRow[3] = ConstNotUsed;
                else
                    dataRow[3] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "№ Измерения", "Изменение показаний индикатора, делений шкалы",
                "Допустимые изменения показаний, делений шкалы"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
            ;
        }


        /// <inheritdoc />
        protected override void InitWork()
        {
            base.InitWork();
            var operation = new MeasuringOperation<double>();

            var arrPoints = IchBase.RangesFull.Ranges.Max().Stop.GetArayMeasPointsInParcent(50, 50, 50, 50).ToArray();
            double[] arrGetting = null;

            operation.InitWork = async () =>
            {
                var a = UserItemOperation.ServicePack.FreeWindow() as WindowService;
                var vm = CreateTable("Изменение показаний индикатора, делений шкалы", arrPoints, new SettingTableViewModel { IsHorizontal = false });
                a.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                a.SizeToContent = SizeToContent.WidthAndHeight;
                a.Show("PerpendicularPressureView", vm, null, null);
                /*Получаем измерения*/
                arrGetting= vm.Cells.Select((cell)=>(double)ObjectToDecimal(cell)).ToArray();
            };

            operation.BodyWorkAsync = () =>
            {
                for (var i = 0; i < arrGetting.Length; i++)
                {
                    if (i > 0) operation = (MeasuringOperation<double>) operation.Clone();
                    operation.Expected = (double) arrPoints[i].Clone();
                    operation.Getting = arrGetting[i];
                    if (i > 0) DataRow.Add(operation);
                }
            };
            operation.ErrorCalculation = (expected, getting) => DataRow.Max(q => q.Getting);
            //todo Указать погрешность  
            operation.CompliteWork = async () => operation.Error <= IchBase.PerpendicularPressureMax;

            DataRow.Add(operation);

        }

        #endregion
    }

    /// <summary>
    ///     Определение размаха показаний
    /// </summary>
    public sealed class RangeIndications : MainIchProcedur<MeasPoint<Length>[]>
    {
        /// <inheritdoc />
        public RangeIndications(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение размаха показаний";
        }

        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as MeasuringOperation<MeasPoint<Length>[]>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;

                dataRow[0] = dds.Expected.ToString();
                for (var i = 0; i < 5; i++) dataRow[i + 1] = dds.Getting[i].ToString();

                dataRow[5] = dds.Error;
                if (dds.IsGood == null)
                    dataRow[6] = ConstNotUsed;
                else
                    dataRow[6] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Точка диапазона измерений индикатора", "Показания при арретировании",
                "Показания при арретировании2", "Показания при арретировании3",
                "Показания при арретировании4", "Показания при арретировании5",
                "Размах показаний"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <inheritdoc />
        protected override void InitWork()
        {
            base.InitWork();
            var operation = new MeasuringOperation<MeasPoint<Length>[]>();
            var arrPoints = IchBase.RangesFull.Ranges.Max().Stop.GetArayMeasPointsInParcent(0,0,0,0,0,50, 50, 50, 50,50,100,100,100,100,100).ToArray();
            MeasPoint<Length>[] arrGetting = null;

            operation.InitWork = async () =>
            {
                var a = UserItemOperation.ServicePack.FreeWindow() as WindowService;
                var setting = new SettingTableViewModel{Breaking = 5, CellFormat = "мк" };

                var vm = CreateTable("Показания при арретировании", arrPoints, setting);
                a.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                a.SizeToContent = SizeToContent.WidthAndHeight;
                a.Show("RangeIcdicationView", vm, null, null);
                /*Получаем измерения*/
                arrGetting = vm.Cells.Select(cell => new MeasPoint<Length>(ObjectToDecimal(cell), UnitMultiplier.Micro)).ToArray();
            };

            operation.BodyWorkAsync = () =>
            {
                for (var i = 0; i < 3; i++)
                {
                    if (i > 0) operation = (MeasuringOperation<MeasPoint<Length>[]>)operation.Clone();
                    operation.Expected = (MeasPoint<Length>[])arrPoints[i*5].Clone();
                    operation.Getting = arrGetting.Skip(i*5).Take(5).ToArray();
                    if (i > 0) DataRow.Add(operation);
                }
            };
            operation.ErrorCalculation =
                (expected, getting) => { expected-getting.m };
            operation.CompliteWork = async () => operation.Error <= IchBase.PerpendicularPressureMax;

            DataRow.Add(operation);

          
            IEnumerable<double> Fill(IEnumerable<ICell> cells)
            {
                foreach (var cell  in cells)
                {
                    yield return (double) ObjectToDecimal(cell.Value);

                }
            }
        }

        #endregion
    }

    /// <summary>
    ///     Определение вариации показаний
    /// </summary>
    public sealed class VariationReading : MainIchProcedur<MeasPoint<Length>>
    {
        /// <inheritdoc />
        public VariationReading(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение вариации показаний";
        }
    }
}
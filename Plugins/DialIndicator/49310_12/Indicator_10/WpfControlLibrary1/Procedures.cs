using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Accord.Video.DirectShow;
using AP.Extension;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.ViewModel;
using ASMC.Core.Model;
using ASMC.Core.UI;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.UInterface.AnalogDevice.ViewModel;
using ASMC.Devices.UInterface.RemoveDevice.ViewModel;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.Devices.USB_Device.WebCam;
using ASMC.Devices.WithoutInterface.HourIndicator;
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
        #region Property

        /// <summary>
        ///     Предоставляет индикатор частового типа
        /// </summary>
        protected IchBase IchBase { get; private set; }

        /// <summary>
        ///     Позволяет получить конец диапазона чисоваого индикатора
        /// </summary>
        protected MeasPoint<Length> EndRange { get; private set; }

        #endregion

        /// <inheritdoc />
        protected MainIchProcedur(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Nested type: SettingTableViewModel

        protected class SettingTableViewModel
        {
            #region Field

            /// <summary>
            ///     Рабите ячеек на столцы/строки в зависимости от <see cref="IsHorizontal" />
            /// </summary>
            public int? Breaking;

            /// <summary>
            ///     Форматирование яческий
            /// </summary>
            public string CellFormat;

            /// <summary>
            ///     Расположение ячеек горизонтальное
            /// </summary>
            public bool IsHorizontal = true;

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Создает VM
        /// </summary>
        /// <param name="name">Наименование таблицы</param>
        /// <param name="measPoints">Массив измерительных точек</param>
        /// <param name="UnitCell">отознаечние едениц измерения в ячеках таблицы</param>
        /// <param name="isHorizantal">При значении <see cref="true" /> распологает ячейки горизотально</param>
        /// <returns></returns>
        protected virtual TableViewModel CreateTable(string name, IMeasPoint<Length>[] measPoints,
            SettingTableViewModel setting)
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
                if (setting.IsHorizontal)
                {
                    columnIndex++;
                    if (setting.Breaking == null) continue;
                    if (columnIndex % setting.Breaking == 0)
                    {
                        rowIndex++;
                        columnIndex = 0;
                    }
                }
                else
                {
                    rowIndex++;
                    if (setting.Breaking == null) continue;
                    if (rowIndex % setting.Breaking == 0)
                    {
                        columnIndex++;
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

        /// <param name="token"></param>
        /// <param name="token1"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationToken token)
        {
            IchBase = ((IControlPannelDevice)UserItemOperation.TestDevices.First().SelectedDevice).Device as IchBase;
            EndRange = (MeasPoint<Length>) IchBase.RangesFull.RealRangeStor.Max().Stop;
            base.InitWork(token);
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
    ///     Предоставляет реализацию определения измерительного усилия.
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
         
        }

        /// <param name="token"></param>
        /// <param name="token1"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationToken token)
        {
            base.InitWork(token);
            var operation = new MultiErrorMeasuringOperation<object>();


            var arrPoints = EndRange.GetArayMeasPointsInParcent(0, 50, 100).ToArray();

            var arrReversePoint = arrPoints.Reverse().ToArray();
            var arrstraightReversePoint = EndRange.GetArayMeasPointsInParcent(50, 50).ToArray();
            var fullPoints = arrPoints.Concat(arrReversePoint).Concat(arrstraightReversePoint).ToArray();
            var fullGettingPoints = Array.Empty<MeasPoint<Weight>>();


#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWork = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {
                var a = UserItemOperation.ServicePack.FreeWindow() as SelectionService;

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
                a.DocumentType = "MeasuringForceView";
                a.ViewModel = vm;
                a.Show();
                fullGettingPoints = vm.Content.Aggregate(fullGettingPoints,
                    (current, item) => current.Concat(Fill(item)).ToArray());
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
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.CompliteWork = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
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
    ///   Предоставляет реализацию  Определения изменений показаний индикатор при нажиме на измерительный стержень в направлении перпендикулярном его
    ///     оси.
    /// </summary>
    public sealed class PerpendicularPressure : MainIchProcedur<decimal>
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
                var dds = row as MeasuringOperation<decimal>;
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


        /// <param name="token"></param>
        /// <param name="token1"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationToken token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<decimal>();

            var arrPoints = EndRange.GetArayMeasPointsInParcent(50, 50, 50, 50)
                .ToArray();
            decimal[] arrGetting = null;

#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWork = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {
                var a = UserItemOperation.ServicePack.FreeWindow() as SelectionService;


                var vm = new PerpendicularPressureViewModel();
                vm.Data= CreateTable("Изменение показаний индикатора, делений шкалы", arrPoints,
                    new SettingTableViewModel { IsHorizontal = false });
                a.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                a.ViewModel = vm;
                a.DocumentType = "PerpendicularPressureView";
                a.Show();
                /*Получаем измерения*/
                arrGetting = vm.Data.Cells.Select(cell => ObjectToDecimal(cell.Value)).ToArray();
            };

            operation.BodyWorkAsync = () =>
            {
                for (var i = 0; i < arrGetting.Length; i++)
                {
                    if (i > 0) operation = (MeasuringOperation<decimal>) operation.Clone();
                    operation.Expected = arrPoints[i].MainPhysicalQuantity.Value;
                    operation.Getting = arrGetting[i];
                    if (i > 0) DataRow.Add(operation);
                }
            };
            operation.ErrorCalculation = (expected, getting) => DataRow.Max(q => q.Getting);
            //todo Указать погрешность  
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.CompliteWork = async () => operation.Error <= IchBase.PerpendicularPressureMax;
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод

            DataRow.Add(operation);
        }

        #endregion
    }

    /// <summary>
    ///     Предоставляет реализацию определение размаха показаний.
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

        /// <param name="token"></param>
        /// <param name="token1"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationToken token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<MeasPoint<Length>[]>();
            var arrPoints = EndRange.GetArayMeasPointsInParcent(GeneratenParcent(5, 0, 50, 100)).ToArray();
            MeasPoint<Length>[] arrGetting = null;

            int[] GeneratenParcent(int count, params int[] parcent)
            {
                var array = new List<int>(parcent.Length);

                foreach (var par in parcent)
                    for (var i = 0; i < count; i++)
                        array.Add(par);

                return array.ToArray();
            }

#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWork = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {
                var a = UserItemOperation.ServicePack.FreeWindow() as SelectionService;
                var setting = new SettingTableViewModel {Breaking = 5, CellFormat = "мкм"};

                var vm = CreateTable("Показания при арретировании", arrPoints, setting);
                a.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                a.ViewModel = vm;
                a.DocumentType = "RangeIcdicationView";
                a.Show();
                /*Получаем измерения*/
                arrGetting = vm.Cells.Select(cell => new MeasPoint<Length>(ObjectToDecimal(cell), UnitMultiplier.Micro))
                    .ToArray();
            };

            operation.BodyWorkAsync = () =>
            {
                for (var i = 0; i < 3; i++)
                {
                    if (i > 0) operation = (MeasuringOperation<MeasPoint<Length>[]>) operation.Clone();
                    operation.Expected = (MeasPoint<Length>[]) arrPoints[i * 5].Clone();
                    operation.Getting = arrGetting.Skip(i * 5).Take(5).ToArray();
                    if (i > 0) DataRow.Add(operation);
                }
            };

            operation.ErrorCalculation =
                (expected, getting) =>
                {
                    return new[]
                    {
                        expected.FirstOrDefault() -
                        getting.Max(q => Math.Abs(q.MainPhysicalQuantity.GetNoramalizeValueToSi()))
                    };
                };
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.CompliteWork = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
                operation.Error.FirstOrDefault().MainPhysicalQuantity.GetNoramalizeValueToSi() <=
                IchBase.PerpendicularPressureMax;

            DataRow.Add(operation);
        }

        #endregion
    }

    /// <summary>
    ///   Предоставляет реализацию  определение вариации показаний
    /// </summary>
    public sealed class VariationReading : MainIchProcedur<MeasPoint<Length>[]>
    {
        /// <inheritdoc />
        public VariationReading(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение вариации показаний";
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                //var dataRow = dataTable.NewRow();
                //var dds = row as MeasuringOperation<MeasPoint<Length>[]>;
                //// ReSharper disable once PossibleNullReferenceException
                //if (dds == null) continue;

                //dataRow[0] = dds.Expected.ToString();
                //for (var i = 0; i < 5; i++) dataRow[i + 1] = dds.Getting[i].ToString();

                //dataRow[5] = dds.Error;
                //if (dds.IsGood == null)
                //    dataRow[6] = ConstNotUsed;
                //else
                //    dataRow[6] = dds.IsGood() ? ConstGood : ConstBad;
                //dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Точка диапазона измерений индикатора", "Прямой ход",
                "Обратный ход", "Вариация показаний,",
                "Допустимая вариация"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <param name="token"></param>
        /// <param name="token1"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationToken token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<MeasPoint<Length>[]>();

            int[] GeneratenParcent(int count, params int[] parcent)
            {
                var array = new List<int>(parcent.Length);

                foreach (var par in parcent)
                    for (var i = 0; i < count; i++)
                        array.Add(par);

                return array.ToArray();
            }


            var arrPoints = EndRange.GetArayMeasPointsInParcent(GeneratenParcent(6, 0, 50, 100)).ToArray();
            MeasPoint<Length>[] arrGetting = { };
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWork = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {
                var a = UserItemOperation.ServicePack.FreeWindow() as SelectionService;
                var setting = new SettingTableViewModel {Breaking = 2, CellFormat = "мкм"};

              
                var rm1 = new WorkInPpiViewModel();
                var webCam = this.UserItemOperation.ControlDevices.FirstOrDefault(q => (q.SelectedDevice as WebCam) != null);
                //rm1.WebCam.WebCam.Source = new FilterInfo(webCam?.StringConnect);
                var ppi = this.UserItemOperation.ControlDevices.FirstOrDefault(q => (q.SelectedDevice as Ppi) != null);
                //rm1.Ppi.NubmerDevice = int.Parse(ppi?.StringConnect ?? string.Empty);
                rm1.Content = CreateTable("Определение вариации показаний", arrPoints, setting);
                a.ViewModel = rm1;
                a.DocumentType = "RangeIcdicationView";
                a.Show();
                /*Получаем измерения*/
                arrGetting = rm1.Content.Cells.Select(cell => new MeasPoint<Length>(ObjectToDecimal(cell.Value), UnitMultiplier.Micro))
                    .ToArray();
            };

            operation.BodyWorkAsync = () =>
            {
                for (var i = 0; i < 9; i++)
                {
                    if (i > 0) operation = (MeasuringOperation<MeasPoint<Length>[]>) operation.Clone();
                    operation.Expected = (MeasPoint<Length>[]) arrPoints[i * 3].Clone();
                    operation.Getting = arrGetting.Skip(i * 5).Take(5).ToArray();
                    if (i > 0) DataRow.Add(operation);
                }
            };

            operation.ErrorCalculation =
                (expected, getting) =>
                {
                    return new[]
                    {
                        expected.FirstOrDefault() -
                        getting.Max(q => Math.Abs(q.MainPhysicalQuantity.GetNoramalizeValueToSi()))
                    };
                };
            // ReSharper disable once PossibleNullReferenceException
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.CompliteWork = async () =>
                operation.Error.FirstOrDefault().MainPhysicalQuantity.GetNoramalizeValueToSi() <=
                IchBase.PerpendicularPressureMax;
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод

            DataRow.Add(operation);
        }
    }

    /// <summary>
    /// Предоставляет реализацию определение погрешности на участке 1 мм.
    /// </summary>
    public sealed class DeterminationError : MainIchProcedur<MeasPoint<Length>[]>
    {
        /// <inheritdoc />
        public DeterminationError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности на всем диапазоне и на участке 1 мм";
        }

        /// <param name="token"></param>
        /// <param name="token1"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationToken token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<MeasPoint<Length>[]>();
            MeasPoint<Length>[] arrPoints=null;
            var measpoint = new List<MeasPoint<Length>>();
            for (decimal i = 0.2m; i <= EndRange.MainPhysicalQuantity.Value; i+=0.2m)
            {
                measpoint.Add(new MeasPoint<Length>(i, UnitMultiplier.Mili));
                //if (i==Math.Truncate(i) && i!= EndRange.MainPhysicalQuantity.Value && i!=0)
                //{
                //    measpoint.Add(new MeasPoint<Length>(i));
                //}
            }
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWork = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {

                var freeWindow = UserItemOperation.ServicePack.FreeWindow() as SelectionService;
                var setting = new SettingTableViewModel();
                setting.Breaking = 5;
                setting.CellFormat = "мкм";
                var rm1 = new WorkInPpiViewModel();
                var webCam = this.UserItemOperation.ControlDevices.FirstOrDefault(q => (q.SelectedDevice as WebCam) != null);
                //rm1.WebCam.WebCam.Source = new FilterInfo(webCam?.StringConnect);
                var ppi = this.UserItemOperation.ControlDevices.FirstOrDefault(q => (q.SelectedDevice as Ppi)!=null);
                //rm1.Ppi.NubmerDevice = int.Parse(ppi?.StringConnect ?? string.Empty);
                rm1.Content =  CreateTable("Определение погрешности на всем диапазоне и на участке 1 мм", measpoint.ToArray(), setting); 
                freeWindow.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                freeWindow.ViewModel = rm1;
                freeWindow.DocumentType = "RangeIcdicationView";
                freeWindow.Show();
                arrPoints= rm1.Content.Cells.Select(cell => new MeasPoint<Length>(ObjectToDecimal(cell.Value), UnitMultiplier.Micro)).ToArray();
            };
            operation.BodyWorkAsync = () =>
            {
                for (var i = 0; i < EndRange.MainPhysicalQuantity.Value; i++)
                {
                    if (i > 0) operation = (MeasuringOperation<MeasPoint<Length>[]>) operation.Clone();
                    if (i == 0)
                    {
                        operation.Expected = new[] {new MeasPoint<Length>(0, UnitMultiplier.Mili)}
                            .Concat(arrPoints.Skip(i * 5).Take(5)).ToArray();
                        operation.Getting = new[] {new MeasPoint<Length>(0, UnitMultiplier.Mili)}
                            .Concat(measpoint.Skip(i * 5).Take(5)).ToArray();
                    }
                    else
                    {
                        operation.Expected = new[] {arrPoints.Skip(i * 5 - 1).Take(1).First()}
                            .Concat(arrPoints.Skip(i * 5).Take(5)).ToArray();
                        operation.Getting = new[] {measpoint.Skip(i * 5 - 1).Take(1).First()}
                            .Concat(measpoint.Skip(i * 5).Take(5)).ToArray();
                    }

                    if (i > 0) DataRow.Add(operation);
                }
            };
            operation.ErrorCalculation = (expected, getting) =>
            {
                var a = expected.Select((t, index) => t - getting[index]).ToArray();
                var res = (Length)(a.Max() -a.Min()).MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.Micro);
                return new[] {new MeasPoint<Length>(res)};
            };

            DataRow.Add(operation);
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return base.GenerateDataColumnTypeObject();
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            return base.FillData();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Accord.Video.DirectShow;
using AP.Extension;
using AP.Math;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Common.ViewModel;
using ASMC.Core.Model;
using ASMC.Core.UI;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.Devices.USB_Device.WebCam;
using ASMC.Devices.WithoutInterface.HourIndicator;
using ASMC.Devices.WithoutInterface.HourIndicator.IchGost577;
using DevExpress.Mvvm.UI;
using mp2192_92.DialIndicator.ViewModel;
using NLog;

namespace mp2192_92.DialIndicator
{
    /// <summary>
    ///     Придоставляет базувую реализацию для пунктов поверки индикаторов частового типа
    ///     <see cref="ASMC.Devices.WithoutInterface.HourIndicator.IchGost577" />
    /// </summary>
    /// <typeparam name="TOperation"></typeparam>
    public abstract class MainIchProcedur<TOperation> : ParagraphBase<TOperation>
    {
        #region Property

        /// <summary>
        ///     Предоставляет индикатор частового типа
        /// </summary>
        protected IchGost577 IchBase { get; private set; }

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
        protected MainIchProcedur(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

     

        #region Methods

        protected decimal RoundNoSigns(decimal value)
        {
            var val = value;
            MathStatistics.Round(ref val, 0);
            return val;
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name;
        }
        /// <summary>
        /// Возращает строковое значениеокругленное до значащах единиц погрешности и приведеное к множителю погрешности.
        /// </summary>
        /// <param name="measPoint"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        protected string GetValueNormalizeToError<T>(MeasPoint<T> measPoint,
            MeasPoint<T> error) where T : class, IPhysicalQuantity<T>, new()
        {
            var value = measPoint.MainPhysicalQuantity.ChangeMultiplier(error.MainPhysicalQuantity.Multiplier)
                .Value;
            return MathStatistics.Round(ref value, error.GetMainValue().ToString());
        }

        /// <param name="token"></param>
        /// <param name="token1"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            IchBase = ((IControlPannelDevice) UserItemOperation.TestDevices.First().SelectedDevice).Device as IchGost577;
            EndRange = (MeasPoint<Length>) IchBase.RangesFull.Ranges.Max().End;
            Service = UserItemOperation.ServicePack.FreeWindow() as SelectionService;
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

    #region Periodic

    /// <summary>
    /// Предоставляет реализацию внешнего осномотра.
    /// </summary>
    public sealed class VisualInspection : MainIchProcedur<bool>
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
                dataRow[0] = dds.Getting ? "соответствует требованиям МИ 2192-92" : dds.Comment;
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
            DataRow.Add(new DialogOperationHelp(this, "Внешний осмотр ГОСТ 577"));
        }
    }
    /// <summary>
    /// Предоставляет операцию опробывания.
    /// </summary>
    public sealed class Testing : MainIchProcedur<bool>
    {
        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.InsetrTextByMark.GetStringValue() + GetType().Name;
        }
        /// <inheritdoc />
        public Testing(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробывание";
        }
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, "Опробывание ГОСТ 577"));
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
                dataRow[0] = dds.Getting ? "соответствует требованиям МИ 2192-92" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }
    }

    /// <summary>
    ///     Предоставляет реализацию определения измерительного усилия.
    /// </summary>
    public sealed class MeasuringForce : MainIchProcedur<object>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
                dataRow[0] =((MeasPoint<Length>)dds.Expected)?.GetMainValue();
                dataRow[1] = ((MeasPoint<Weight>)dds.Getting)?.GetMainValue();
                dataRow[2] = GetValueNormalizeToError((MeasPoint<Force>)dds.Error[0], IchBase.MeasuringForce.ChangeCourse);
                dataRow[3] = GetValueNormalizeToError((MeasPoint<Force>)dds.Error[1], IchBase.MeasuringForce.StraightRun);
                dataRow[4] = GetValueNormalizeToError((MeasPoint<Force>)dds.Error[2], IchBase.MeasuringForce.Oscillatons);
                //todo Указать погрешность  
                  dataRow[5] = IchBase.MeasuringForce.ChangeCourse.GetMainValue();
                dataRow[7] = IchBase.MeasuringForce.StraightRun.GetMainValue();
                dataRow[6] = IchBase.MeasuringForce.Oscillatons.GetMainValue();
              
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
            };
        }

        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MultiErrorMeasuringOperation<object>();


            var arrPoints = EndRange.GetArayMeasPointsInParcent(0, 50, 100).ToArray();

            var arrReversePoint = arrPoints.Reverse().ToArray();
            var arrstraightReversePoint = EndRange.GetArayMeasPointsInParcent(50, 50).ToArray();
            var fullPoints = arrPoints.Concat(arrReversePoint).Concat(arrstraightReversePoint).ToArray();
            var fullGettingPoints = Array.Empty<MeasPoint<Weight>>();


#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {
               
                var nameCell = "г"; /*Форматирование ячеек в таблице*/
                var setting = new TableViewModel.SettingTableViewModel { CellFormat = nameCell };
                var first = TableViewModel.CreateTable("Прямой ход", arrPoints, setting);
                var too = TableViewModel.CreateTable("Обратный ход", arrReversePoint, setting);
                var tree = TableViewModel.CreateTable("Прямой/обратный ход", arrstraightReversePoint, setting);
                var vm = new MeasuringForceViewModel();
                vm.Content.Add(first);
                vm.Content.Add(too);
                vm.Content.Add(tree);
                Service.Title = Name;
                Service.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                Service.DocumentType = "MeasuringForceView";
                Service.ViewModel = vm;
                Service.Show();
                fullGettingPoints = vm.Content.Aggregate(fullGettingPoints,
                    (current, item) => current.Concat(Fill(item)).ToArray());

            };

            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < fullPoints.Length; i++)
                    {
                        if (i > 0) operation = (MultiErrorMeasuringOperation<object>)operation.Clone();
                        operation.Expected = fullPoints[i].Clone();
                        operation.Getting = fullGettingPoints[i].Clone();
                        Logger.Debug($@"Ожидаемое:{((IMeasPoint<Length>)operation.Expected).Description} измеренное {((IMeasPoint<Weight>)operation.Getting).Description}");
                        if (i == 0)
                        {
                            operation.IsGood = () =>
                            {
                                Logger.Debug($@"Максимальное усилие {(operation?.Error?.Take(1).First() as MeasPoint<Force>).Description }");
                                Logger.Debug($@"Прямой/обратный ход {(operation?.Error?.Skip(1).Take(1).First() as MeasPoint<Force>).Description }");
                                Logger.Debug($@"Изменение хода{(operation?.Error?.Skip(2).Take(1).First() as MeasPoint<Force>).Description }");
                                return operation?.Error?.Take(1).First() as MeasPoint<Force> <= IchBase.MeasuringForce.StraightRun
                                       &&
                                       operation?.Error?.Skip(1).Take(1).First() as MeasPoint<Force> <=
                                       IchBase.MeasuringForce.Oscillatons
                                       &&
                                       operation?.Error?.Skip(2).Take(1).First() as MeasPoint<Force> <=
                                       IchBase.MeasuringForce.ChangeCourse;
                            };
                        }
                        if (i > 0) DataRow.Add(operation);
                    }
                }, cancellationToken);
              
            };
            operation.ErrorCalculation = new Func<object, object, object>[]
            {
                (expected, getting) =>
                {
                    var fullMeasPoints = new List<MeasPoint<Weight>>(8);
                    fullMeasPoints.AddRange(DataRow.Select(t => (MeasPoint<Weight>) t.Getting));

                    var fullMeasPointForces = ConvertWeightToForce(fullMeasPoints.ToArray());

                    var array = fullMeasPointForces.Take(6).ToArray();
                    Logger.Debug($@"Полное: Максимум {array.Max().Description} и минимум {array.Min().Description}");
                    return array.Max() - array.Min();
                },
                (expected, getting) =>
                {
                    var fullMeasPoints = new List<MeasPoint<Weight>>(8);
                    fullMeasPoints.AddRange(DataRow.Select(t => (MeasPoint<Weight>) t.Getting));

                    var fullMeasPointForces = ConvertWeightToForce(fullMeasPoints.ToArray());

                    var array = fullMeasPointForces.Take(3).ToArray();

                    Logger.Debug($@"Прямое: Максимум {array.Max().Description} и минимум {array.Min().Description}");
                    return array.Max() - array.Min();
                },
                (expected, getting) =>
                {
                    var fullMeasPoints = new List<MeasPoint<Weight>>(8);
                    fullMeasPoints.AddRange(DataRow.Select(t => (MeasPoint<Weight>) t.Getting));

                    var fullMeasPointForces = ConvertWeightToForce(fullMeasPoints.ToArray());

                    var array = fullMeasPointForces.Skip(6).Take(2).ToArray();

                    Logger.Debug($@"прямой/обратный: Максимум {array.Max().Description} и минимум {array.Min().Description}");
                    return array.Max() - array.Min();
                }
            };
 
           
             

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
                        Unit = MeasureUnits.Weight,
                        Value = ObjectToDecimal(cell.Value)
                    });
            }
        }

        #endregion
    }

    /// <summary>
    ///     Предоставляет реализацию  Определения изменений показаний индикатор при нажиме на измерительный стержень в
    ///     направлении перпендикулярном его
    ///     оси.
    /// </summary>
    public sealed class PerpendicularPressure : MainIchProcedur<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <inheritdoc />
        public PerpendicularPressure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Изменение показания индикатора при нажиме с усилием.";
        }

        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as MeasuringOperation<decimal>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Expected.ToString();
                dataRow[1] = dds.Getting.ToString();
                dataRow[2] =IchBase.PerpendicularPressureMax;
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
            };
            ;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<decimal>();

            var arrPoints = EndRange.GetArayMeasPointsInParcent(50, 50, 50, 50)
                .ToArray();
            decimal[] arrGetting = null;

#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {


                var vm = new OneTableViewModel();
                vm.Data = TableViewModel.CreateTable("Изменение показаний индикатора, делений шкалы", arrPoints,
                    new TableViewModel.SettingTableViewModel { IsHorizontal = false, CellFormat = "дел."});
                Service.ViewLocator = new ViewLocator(vm.GetType().Assembly);
                Service.ViewModel = vm;
                Service.DocumentType = "OneTableView";
                Service.Show();
                /*Получаем измерения*/
                arrGetting = vm.Data.Cells.Select(cell => ObjectToDecimal(cell.Value)).ToArray();
            };

            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < arrGetting.Length; i++)
                    {
                        if (i > 0) operation = (MeasuringOperation<decimal>)operation.Clone();
                        operation.Expected = arrPoints[i].MainPhysicalQuantity.Value;
                        operation.Getting = arrGetting[i];
                        Logger.Debug($@"Ожидаемое:{(operation.Expected)} измеренное {operation.Getting}");
                        if (i > 0) DataRow.Add(operation);
                        if (i == 0)
                        {
                            operation.IsGood = () =>
                            {
                                Logger.Debug($@"Максимальное усиле {operation?.Error}");
                                return operation.Error <= IchBase.PerpendicularPressureMax;
                            };
                        }
                    }

                }, cancellationToken);
             

            };
            operation.ErrorCalculation = (expected, getting) => DataRow.Max(q => q.Getting);

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
            Name = "Определение размаха показаний.";
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

                dataRow[0] =dds.Expected[0].GetMainValue();
                for (var i = 0; i < dds.Getting.Length; i++) dataRow[i+1] = dds.Getting[i].GetMainValue();
                var a = dds.Error[0].MainPhysicalQuantity
                    .ChangeMultiplier(IchBase.Arresting.MainPhysicalQuantity.Multiplier).Value;
                dataRow[6] = MathStatistics.Round(ref a, IchBase.Arresting.GetMainValue().ToString());
                dataRow[7] = IchBase.Arresting.GetMainValue();
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
                "Размах показаний", "Допустимый размах"
            };
        }

        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<MeasPoint<Length>[]>();
            var arrPoints = EndRange.GetArayMeasPointsInParcent(GeneratenParcent(5, 0, 50, 100)).ToArray();
            MeasPoint<Length>[] arrGetting = null;

            double[] GeneratenParcent(int count, params int[] parcent)
            {
                var array = new List<double>(parcent.Length);

                foreach (var par in parcent)
                    for (var i = 0; i < count; i++)
                        array.Add(par);

                return array.ToArray();
            }

#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {
            
                var setting = new TableViewModel.SettingTableViewModel { Breaking = 5, CellFormat = "мкм" };
                
                var vm = new OneTableViewModel();
                vm.Data = TableViewModel.CreateTable("Показания при арретировании", arrPoints,
                    setting);
                Service.Title = Name;
                Service.ViewLocator = new ViewLocator(vm.GetType().Assembly);
                Service.ViewModel = vm;
                Service.DocumentType = "OneTableView";
                Service.Show();

                /*Получаем измерения*/
                arrGetting = vm.Data.Cells.Select(cell => new MeasPoint<Length>(ObjectToDecimal(cell.Value), UnitMultiplier.Micro))
                    .ToArray();
            };

            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < 3; i++)
                    {
                        if (i > 0) operation = (MeasuringOperation<MeasPoint<Length>[]>)operation.Clone();
                        operation.Expected = new[] { (MeasPoint<Length>)arrPoints[i * 5].Clone() };
                        operation.Getting = arrGetting.Skip(i * 5).Take(5).ToArray();
                        if (i > 0) DataRow.Add(operation);
                    }
                }, cancellationToken);
              
            };

            operation.ErrorCalculation =
                (getting,  expected) =>
                {
                    var res = new MeasPoint<Length>(getting.Select(q => expected.First() - q)
                        .Max(q => Math.Abs(q.MainPhysicalQuantity.GetNoramalizeValueToSi())));
                    res.MainPhysicalQuantity.ChangeMultiplier(getting.First().MainPhysicalQuantity.Multiplier);
                    return new[]
                    {
                        res
                    };
                };
             operation.IsGood = () =>  operation.Error.FirstOrDefault() <=
                                     IchBase.Arresting;

            DataRow.Add(operation);
        }

        #endregion
    }

    /// <summary>
    ///     Предоставляет реализацию  определение вариации показаний
    /// </summary>
    public sealed class VariationReading : MainIchProcedur<MeasPoint<Length>[]>
    {
        /// <inheritdoc />
        public VariationReading(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение вариации показаний.";
        }

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

                dataRow[0] = dds.Expected.First().GetMainValue();
                for (var i = 0; i < 2; i++) dataRow[i + 1] = dds.Getting[i].GetMainValue();
                dataRow[3] = GetValueNormalizeToError(dds.Error.First(), IchBase.Variation);
                dataRow[4] = IchBase.Variation.GetMainValue();
                dataTable.Rows.Add(dataRow);
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
            };
        }

        /// <param name="token"></param>
        /// <param name="token1"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<MeasPoint<Length>[]>();

            double[] GeneratenParcent(int count, params int[] parcent)
            {
                var array = new List<double>(parcent.Length);

                foreach (var par in parcent)
                    for (var i = 0; i < count; i++)
                        array.Add(par);

                return array.ToArray();
            }


            var arrPoints = EndRange.GetArayMeasPointsInParcent(GeneratenParcent(6, 0, 50, 100)).ToArray();
            MeasPoint<Length>[] arrGetting = { };
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {
                var service = UserItemOperation.ServicePack.FreeWindow() as SelectionService;
                var setting = new TableViewModel.SettingTableViewModel { Breaking = 2, CellFormat = "мкм" };


                var viewModel = new WorkInPpiViewModel();


                var webCam = UserItemOperation.ControlDevices.FirstOrDefault(q =>
                    (q.SelectedDevice as IControlPannelDevice)?.Device as WebCam != null);
                var ppi = UserItemOperation.ControlDevices.FirstOrDefault(q => q.SelectedDevice as Ppi != null);
                if (ppi != null)
                {
                    viewModel.Ppi.NubmerDevice = int.Parse(ppi?.StringConnect ?? "0");
                }
                if (webCam != null)
                {
                    viewModel.WebCam.WebCam = (webCam.SelectedDevice as IControlPannelDevice)?.Device as WebCam;
                    viewModel.WebCam.WebCam.Source = new FilterInfo(webCam?.StringConnect);
                }

                service.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                viewModel.Content = TableViewModel.CreateTable("Определение вариации показаний",  arrPoints, setting);
                service.Title = Name;
                service.ViewModel = viewModel;
                service.DocumentType = "RangeIcdicationView";
                service.Show();
                /*Получаем измерения*/
                arrGetting = viewModel.Content.Cells.Select(cell =>
                        new MeasPoint<Length>(ObjectToDecimal(cell.Value), UnitMultiplier.Micro))
                    .ToArray();
            };

            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < 9; i++)
                    {
                        if (i > 0) operation = (MeasuringOperation<MeasPoint<Length>[]>)operation.Clone();
                        operation.Expected = new[] { (MeasPoint<Length>)arrPoints[i * 2].Clone() };
                        operation.Getting = arrGetting.Skip(i * 2).Take(2).ToArray();
                        if (i > 0) DataRow.Add(operation);
                    }
                }, cancellationToken);
               
            };
             operation.IsGood = () => operation.Error.First() <= IchBase.Variation;
            operation.ErrorCalculation =
                (expected, getting) =>
                {
                    return new[]
                    {
                       new MeasPoint<Length>((Length) (expected.FirstOrDefault() -
                                                       getting.Max(q => (MeasPoint<Length>)q.Abs())).MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.Micro))  
                    };
                };
            // ReSharper disable once PossibleNullReferenceException
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.CompliteWorkAsync = async () =>
                operation.Error.FirstOrDefault().MainPhysicalQuantity.GetNoramalizeValueToSi() <=
                IchBase.PerpendicularPressureMax;
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод

            DataRow.Add(operation);
        }
    }

    /// <summary>
    ///     Предоставляет реализацию определение погрешности на участке 1 мм.
    /// </summary>
    public sealed class DeterminationError : MainIchProcedur<MeasPoint<Length>[]>
    {
        /// <inheritdoc />
        public DeterminationError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности на всем диапазоне и на участке 1 мм.";
        }

        /// <param name="token"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<MeasPoint<Length>[]>();
            MeasPoint<Length>[] arrPoints = null;
            var measpoint = new List<MeasPoint<Length>>();
            for (var i = 0.2m; i <= EndRange.MainPhysicalQuantity.Value; i += 0.2m)
                measpoint.Add(new MeasPoint<Length>(i, UnitMultiplier.Mili));
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {
                var freeWindow = UserItemOperation.ServicePack.FreeWindow() as SelectionService;
                var setting = new TableViewModel.SettingTableViewModel();
                setting.Breaking = 5;
                setting.CellFormat = "мкм";
                var viewModel = new WorkInPpiViewModel();

                var webCam = UserItemOperation.ControlDevices.FirstOrDefault(q =>
                    (q.SelectedDevice as IControlPannelDevice)?.Device as WebCam != null);
                var ppi = UserItemOperation.ControlDevices.FirstOrDefault(q => q.SelectedDevice as Ppi != null);
                if (ppi != null)
                {
                    viewModel.Ppi.NubmerDevice = int.Parse(ppi?.StringConnect ?? "0");
                }
                if (webCam != null)
                {
                    viewModel.WebCam.WebCam = (webCam.SelectedDevice as IControlPannelDevice)?.Device as WebCam;
                    viewModel.WebCam.WebCam.Source = new FilterInfo(webCam?.StringConnect);
                }

                viewModel.Content = TableViewModel.CreateTable("Определение погрешности на всем диапазоне и на участке 1 мм",
                     measpoint.ToArray(), setting);
                freeWindow.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                freeWindow.ViewModel = viewModel;
                freeWindow.DocumentType = "RangeIcdicationView";
                freeWindow.Show();

                arrPoints = viewModel.Content.Cells
                    .Select(cell => new MeasPoint<Length>(ObjectToDecimal(cell.Value), UnitMultiplier.Micro)).ToArray();
            };
            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < EndRange.MainPhysicalQuantity.Value; i++)
                    {
                        if (i > 0) operation = (MeasuringOperation<MeasPoint<Length>[]>)operation.Clone();
                        if (i == 0)
                        {
                            operation.Expected = new[] { new MeasPoint<Length>(0, UnitMultiplier.Mili) }
                                .Concat(measpoint.Skip(i * 5).Take(5)).ToArray();
                            operation.Getting = new[] { new MeasPoint<Length>(0, UnitMultiplier.Mili) }
                                .Concat(arrPoints.Skip(i * 5).Take(5)).ToArray();
                        }
                        else
                        {
                            operation.Expected = new[] { measpoint.Skip(i * 5 - 1).Take(1).First() }
                                .Concat(measpoint.Skip(i * 5).Take(5)).ToArray();
                            operation.Getting = new[] { arrPoints.Skip(i * 5 - 1).Take(1).First() }
                                .Concat(arrPoints.Skip(i * 5).Take(5)).ToArray();
                        }

                        if (i > 0) DataRow.Add(operation);
                    }
                }, cancellationToken);
              
            };
            operation.ErrorCalculation = (expected, getting) =>
            {
                var a = expected.Select((t, index) => t - getting[index]).ToArray();

                var res = (a.Max() - a.Min()).MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.Micro);
                return new[] { new MeasPoint<Length>((Length) res) };
            };
             operation.IsGood = () =>
            {
                var full = (DataRow.Max(q => (q as MeasuringOperation<MeasPoint<Length>[]>)?.Error[0]) -
                            DataRow.Min(q => (q as MeasuringOperation<MeasPoint<Length>[]>)?.Error[0]));
                var one = DataRow.Max(q => (q as MeasuringOperation<MeasPoint<Length>[]>)?.Error[0]);
                return full <= new MeasPoint<Length>((decimal) IchBase.RangesFull.AccuracyChatacteristic.Resolution) && one <= new MeasPoint<Length>((decimal)IchBase.RangesFull.Ranges[0].AccuracyChatacteristic.Resolution);

            };

            DataRow.Add(operation);
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Участок индикатора", "0", "20", "40", "60", "80", "100", "На любом участке 1 мм", "На всём диапазоне измерений", " Допуск на любом участке в 1 мм", "Допуск на всём диапазоне измерений" };
        }

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

                dataRow[0] = dds.Expected.First().MainPhysicalQuantity.Value + " – " +dds.Expected.Last().GetMainValue();
                for (var i = 0; i < 6; i++) dataRow[i + 1] = dds.Getting[i].GetMainValue();

                dataRow[7] = RoundNoSigns(DataRow.Max(q => (q as MeasuringOperation<MeasPoint<Length>[]>)?.Error[0]).GetMainValue());
                var a = DataRow.Max(q => (q as MeasuringOperation<MeasPoint<Length>[]>)?.Error[0]).MainPhysicalQuantity.Value;
                var b = DataRow.Min(q => (q as MeasuringOperation<MeasPoint<Length>[]>)?.Error[0]).MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.Micro).Value;
                    dataRow[8] = RoundNoSigns(a - b);
                    dataRow[9] =
                        new MeasPoint<Length>((decimal) IchBase.RangesFull.Ranges[0].AccuracyChatacteristic.Resolution).MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.Micro).ToString();

                    dataRow[10] = new MeasPoint<Length>((decimal)IchBase.RangesFull.AccuracyChatacteristic.Resolution).MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.Micro).ToString();
             
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }
    }


    #endregion

    #region First
    /// <summary>
    /// Предоставляет реализацию проверки присоеденительного диаметра
    /// </summary>  
    public sealed class ConnectionDiametr : MainIchProcedur<object>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <inheritdoc />
        public ConnectionDiametr(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Контроль присоединительного диаметра и отклонения от цилиндричности гильзы.";
        }

        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<object>();
            var arrPoints = new [] {"1-e", "2-e", "3-e", "4-e"};
            MeasPoint<Length>[] arrGettin = null;
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {

                var setting = new TableViewModel.SettingTableViewModel { Breaking = 1, CellFormat = "мкм" };
                var vm = new OneTableViewModel();
                    vm.Data = TableViewModel.CreateTable("Присоединительный диаметр", arrPoints, setting);
                    Service.ViewLocator = new ViewLocator(vm.GetType().Assembly);
                Service.ViewModel = vm;
                Service.DocumentType = "OneTableView";
                Service.Show();
                /*Получаем измерения*/
                arrGettin = vm.Data.Cells.Select(cell => new MeasPoint<Length>(ObjectToDecimal(cell.Value), UnitMultiplier.Micro))
                    .ToArray();
            };
            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < arrGettin.Length; i++)
                    {
                        if (i > 0) operation = (MeasuringOperation<object>)operation.Clone();
                        operation.Expected = arrPoints[i];
                        operation.Getting = arrGettin[i];
                        Logger.Debug($@"Измеренное {operation.Getting}");
                        if (i > 0) DataRow.Add(operation);
                        if (i == 0)
                        {
                            operation.IsGood = () =>
                            {
                                Logger.Debug($@"Отклонение от цилендричности{operation?.Error}");
                                return operation.Error as MeasPoint<Length> <= IchBase.ConnectDiametr.MaxDelta && DataRow.All(q => IchBase.ConnectDiametr.Range.IsPointBelong(q.Getting as MeasPoint<Length>));
                            };
                        }
                    }
                }, cancellationToken);
             
            };
            operation.ErrorCalculation = (exped, getting) =>
            {
                var res = (DataRow.Select(q => q.Getting as MeasPoint<Length>).Max() -
                           DataRow.Select(q => q.Getting as MeasPoint<Length>).Min());
                res.MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.Micro);
                return res;
            };
            DataRow.Add(operation);
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Измеряемое сечение", "Номинальный размер диаметра", "Присоединительный диаметр",
                "Отклонение от цилиндричности гильзы", "Допустимое отклонение"
            };

        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as MeasuringOperation<object>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Expected;
                dataRow[1] = IchBase.ConnectDiametr.Range.End.Description;
                dataRow[2] = (dds.Getting as MeasPoint<Length>).Description;
                dataRow[3] = (dds.Error as MeasPoint<Length>).Description;
                dataRow[4] = IchBase.ConnectDiametr.MaxDelta.Description;
                if (dds.IsGood == null)
                    dataRow[5] = ConstNotUsed;
                else
                    dataRow[5] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }
    } //todo готово
    /// <summary>
    /// Предоставляет реализацию проверку  шероховатости наружной поверхности гильзы.
    /// </summary>
    public sealed class LinerRoughness : MainIchProcedur<bool>
    {
        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.InsetrTextByMark.GetStringValue() + GetType().Name;
        }
        /// <inheritdoc />
        public LinerRoughness(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Контроль шероховатости наружной поверхности гильзы.";
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
                dataRow[0] = dds.Getting ? "Соответствует" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, ""));
        }
    }//todo готово
    /// <summary>
    /// Предотсавляет реализацию проерку  шероховатости поверхности измерительного наконечника.
    /// </summary>
    public sealed class TipRoughness : MainIchProcedur<bool>
    {
        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.InsetrTextByMark.GetStringValue() + GetType().Name;
        }
        /// <inheritdoc />
        public TipRoughness(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Контроль шероховатости рабочей поверхности измерительного наконечника.";
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
                dataRow[0] = dds.Getting ? "Соответствует" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, ""));
        }
    }//todo готово
    /// <summary>
    /// Предотсавляет реализацию определения ширины стрелки.
    /// </summary>
    public sealed class ArrowWidch : MainIchProcedur<MeasPoint<Length>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <inheritdoc />
        public ArrowWidch(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение ширины стрелки.";
        }
        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as MeasuringOperation<MeasPoint<Length>>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Getting.Description;
                dataRow[1] = IchBase.ArrowWidch.Start.Description + " - " +IchBase.ArrowWidch.End.Description;
                if (dds.IsGood == null)
                    dataRow[2] = ConstNotUsed;
                else
                    dataRow[2] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Ширина стрелки", "Допустимая ширина",
            };
            ;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<MeasPoint<Length>>();

            var arrPoints = EndRange.GetArayMeasPointsInParcent(50, 50, 50, 50)
                .ToArray();
            MeasPoint<Length> arrGetting = null;

#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {


                var vm = new OneTableViewModel();
                vm.Data = TableViewModel.CreateTable("Ширина стрелки", new []{"Ширина стрелки"}, new TableViewModel.SettingTableViewModel { CellFormat = "мм"});
                Service.ViewLocator = new ViewLocator(vm.GetType().Assembly);
                Service.ViewModel = vm;
                Service.DocumentType = "OneTableView";
                Service.Show();
                /*Получаем измерения*/
                arrGetting = new MeasPoint<Length>(vm.Data.Cells.Select(cell => ObjectToDecimal(cell.Value)).First(), UnitMultiplier.Mili);
            };

            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    operation.Getting = arrGetting;
                    Logger.Debug($@"Измеренное {operation.Getting}");
                }, cancellationToken);
               
                 

            };
             operation.IsGood = () => IchBase.ArrowWidch.IsPointBelong(operation.Getting);

            DataRow.Add(operation);
        }

        #endregion
    } //todo готово
    /// <summary>
    /// Предотсавляет реализацию определения ширины штриха.
    /// </summary>
    public sealed class StrokeWidch : MainIchProcedur<object>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <inheritdoc />
        public StrokeWidch(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение ширины штрихов.";
        }

        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<object>();
            var arrPoints = new[] { "0", "20", "40", "60", "80" };
            MeasPoint<Length>[] arrGettin = null;
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {

                var setting = new TableViewModel.SettingTableViewModel { Breaking = 1, CellFormat = "мкм" };
                var vm = new OneTableViewModel();
                vm.Data = TableViewModel.CreateTable("Определение ширины штрихов", arrPoints, setting);
                Service.ViewLocator = new ViewLocator(vm.GetType().Assembly);
                Service.ViewModel = vm;
                Service.DocumentType = "OneTableView";
                Service.Show();
                /*Получаем измерения*/
                arrGettin = vm.Data.Cells.Select(cell => new MeasPoint<Length>(ObjectToDecimal(cell.Value), UnitMultiplier.Micro))
                    .ToArray();
            };
            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < arrGettin.Length; i++)
                    {
                        if (i > 0) operation = (MeasuringOperation<object>)operation.Clone();
                        operation.Expected = arrPoints[i];
                        operation.Getting = arrGettin[i];
                        Logger.Debug($@"Измеренное {operation.Getting}");
                        if (i > 0) DataRow.Add(operation);
                        if (i == 0)
                        {
                            operation.IsGood = () =>
                            {
                                Logger.Debug($@"Отклонение от цилендричности{operation?.Error}");
                                return operation.Error as MeasPoint<Length> <= IchBase.StrokeWidch.MaxDelta && DataRow.All(q => IchBase.StrokeWidch.Range.IsPointBelong(q.Getting as MeasPoint<Length>));
                            };
                        }
                    }
                }, cancellationToken);
               
            };
            operation.ErrorCalculation = (exped, getting) =>
            {
                var res = (DataRow.Select(q => q.Getting as MeasPoint<Length>).Max() -
                           DataRow.Select(q => q.Getting as MeasPoint<Length>).Min());
                res.MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.Micro);
                return res;
            };
            DataRow.Add(operation);
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return  new []{ "Измерение штриха на отметках шкалы",
                "Ширина штриха",
                "Разность ширины отдельных штрихов",
                "Допустимая ширина штрихов", 
                "Допустимая разность"};
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as MeasuringOperation<object>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Expected;
                dataRow[1] = IchBase.StrokeWidch.Range.Start.Description +" - "+ IchBase.StrokeWidch.Range.End.Description;
                dataRow[2] = (dds.Getting as MeasPoint<Length>).Description;
                dataRow[3] = (dds.Error as MeasPoint<Length>).Description;
                dataRow[4] = IchBase.StrokeWidch.MaxDelta.Description;
                if (dds.IsGood == null)
                    dataRow[5] = ConstNotUsed;
                else
                    dataRow[5] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }
    }//todo готово
    /// <summary>
    /// Предоставляет реализацию определения длинны штриха.
    /// </summary>
    public sealed class StrokeLength : MainIchProcedur<MeasPoint<Length>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <inheritdoc />
        public StrokeLength(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение длины деления штрихов.";
        }
        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as MeasuringOperation<MeasPoint<Length>>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Getting.Description;
                dataRow[1] = IchBase.StrokeLength.Description;
                if (dds.IsGood == null)
                    dataRow[2] = ConstNotUsed;
                else
                    dataRow[2] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Ширина стрелки", "Допустимая ширина",
            };
            ;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<MeasPoint<Length>>();

            var arrPoints = EndRange.GetArayMeasPointsInParcent(50, 50, 50, 50)
                .ToArray();
            MeasPoint<Length> arrGetting = null;

#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {


                var vm = new OneTableViewModel();
                vm.Data = TableViewModel.CreateTable("Определение длины деления шкалы", new[] { "Длина деления" }, new TableViewModel.SettingTableViewModel { CellFormat = "мм" });
                Service.ViewLocator = new ViewLocator(vm.GetType().Assembly);
                Service.ViewModel = vm;
                Service.DocumentType = "OneTableView";
                Service.Show();
                /*Получаем измерения*/
                arrGetting = new MeasPoint<Length>(vm.Data.Cells.Select(cell => ObjectToDecimal(cell.Value)).First(), UnitMultiplier.Mili);
            };

            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    operation.Getting = arrGetting;
                    Logger.Debug($@"Измеренное {operation.Getting}");
                }, cancellationToken);

            };
             operation.IsGood = () => IchBase.StrokeLength<=operation.Getting;

            DataRow.Add(operation);
        }

        #endregion
    } //todo готово
    /// <summary>
    /// Предоставляет реализацию определения расстояния между концом стрелки и циферблатом.
    /// </summary>
    public sealed class BetweenArrowDial : MainIchProcedur<object>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <inheritdoc />
        public BetweenArrowDial(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение расстояния между концом стрелки и циферблатом.";
        }
        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<object>();
            var arrPoints = new[] { "1 – е", "2 – е", "3 – е", "4 – е" };
            MeasPoint<Length>[] arrGettin = null;
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {

                var setting = new TableViewModel.SettingTableViewModel { Breaking = 1, CellFormat = "мкм" };
                var vm = new OneTableViewModel();
                vm.Data = TableViewModel.CreateTable("пределение расстояния между концом стрелки и циферблатом.", arrPoints, setting);
                Service.ViewLocator = new ViewLocator(vm.GetType().Assembly);
                Service.ViewModel = vm;
                Service.DocumentType = "OneTableView";
                Service.Show();
                /*Получаем измерения*/
                arrGettin = vm.Data.Cells.Select(cell => new MeasPoint<Length>(ObjectToDecimal(cell.Value), UnitMultiplier.Micro))
                    .ToArray();
            };
            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < arrGettin.Length; i++)
                    {
                        if (i > 0) operation = (MeasuringOperation<object>)operation.Clone();
                        operation.Expected = arrPoints[i];
                        operation.Getting = arrGettin[i];
                        Logger.Debug($@"Измеренное {operation.Getting}");
                        if (i > 0) DataRow.Add(operation);
                        if (i == 0)
                        {
                            operation.IsGood = () =>
                            {
                                Logger.Debug($@"Отклонение от цилендричности{operation?.Error}");
                                return operation.Error as MeasPoint<Length> <= IchBase.BetweenArrowDial;
                            };
                        }
                    }
                }, cancellationToken);
               
            };
            operation.ErrorCalculation = (exped, getting) =>
            {
                var res = DataRow.Select(q => q.Getting as MeasPoint<Length>).Max();
                res.MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.Micro);
                return res;
            };
            DataRow.Add(operation);
        }
        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]{ "Измерение расстояния между концом стрелки и циферблатом",
                "Расстояние между концом стрелки и циферблатом",
                "Наибольшее расстояние между концом стрелки и циферблатом",
                "Максимальное допустимое расстояние",};
        }
        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as MeasuringOperation<object>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Expected;
                dataRow[1] = (dds.Getting as MeasPoint<Length>).Description;
                dataRow[2] = (dds.Error as MeasPoint<Length>).Description;
                dataRow[3] = IchBase.BetweenArrowDial.Description;
                if (dds.IsGood == null)
                    dataRow[4] = ConstNotUsed;
                else
                    dataRow[4] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }
    }//todo готово


    /// <summary>
    ///     Предоставляет реализацию определение погрешности на участке 1 мм.
    /// </summary>
    public sealed class Determination_01Error : MainIchProcedur<MeasPoint<Length>[]>
    {
        /// <inheritdoc />
        public Determination_01Error(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности на всем диапазоне и на участке 0,1 мм.";
        }

        /// <param name="token"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new MeasuringOperation<MeasPoint<Length>[]>();
            MeasPoint<Length>[] arrPoints = null;
            var first = (MeasPoint<Length>)EndRange.GetArayMeasPointsInParcent(10).First();
            var mid = ((MeasPoint<Length>)EndRange.GetArayMeasPointsInParcent(50).First())-0.05m;
            var last = (MeasPoint<Length>)EndRange.GetArayMeasPointsInParcent(90).First();

            List<MeasPoint<Length>> measpointarr = new List<MeasPoint<Length>>(15);
            FillMeasPoints(first, ref measpointarr);
            FillMeasPoints(mid, ref measpointarr);
            FillMeasPoints(last, ref measpointarr);
            void FillMeasPoints(MeasPoint<Length> start, ref List<MeasPoint<Length>> measpoint)
            {
                for (var i = start; i <= start + 0.1m; i += 0.02m)
                    measpoint.Add(i);
            }
           

#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            operation.InitWorkAsync = async () =>
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
            {
                var freeWindow = UserItemOperation.ServicePack.FreeWindow() as SelectionService;
                var setting = new TableViewModel.SettingTableViewModel();
                setting.Breaking = 6;
                setting.CellFormat = "мкм";
                var viewModel = new WorkInPpiViewModel();

                var webCam = UserItemOperation.ControlDevices.FirstOrDefault(q =>
                    (q.SelectedDevice as IControlPannelDevice)?.Device as WebCam != null);
              
              
                var ppi = UserItemOperation.ControlDevices.FirstOrDefault(q => q.SelectedDevice as Ppi != null);
                if (ppi != null)
                {
                    viewModel.Ppi.NubmerDevice = int.Parse(ppi?.StringConnect ?? "0");
                }
                if (webCam != null)
                {
                    viewModel.WebCam.WebCam = (webCam.SelectedDevice as IControlPannelDevice)?.Device as WebCam;
                    viewModel.WebCam.WebCam.Source = new FilterInfo(webCam?.StringConnect);
                }
                viewModel.Content = TableViewModel.CreateTable("Определение погрешности на всем диапазоне и на участке 1 мм",
                    measpointarr.ToArray(), setting);
                freeWindow.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                freeWindow.ViewModel = viewModel;
                freeWindow.DocumentType = "RangeIcdicationView";
                freeWindow.Show();

                arrPoints = viewModel.Content.Cells
                    .Select(cell => new MeasPoint<Length>(ObjectToDecimal(cell.Value), UnitMultiplier.Micro)).ToArray();
            };
            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < EndRange.MainPhysicalQuantity.Value; i++)
                    {
                        if (i > 0) operation = (MeasuringOperation<MeasPoint<Length>[]>)operation.Clone();
                        operation.Expected = new[] { new MeasPoint<Length>(0, UnitMultiplier.Mili) }
                            .Concat(arrPoints.Skip(i * 6).Take(6)).ToArray();
                        operation.Getting = new[] { new MeasPoint<Length>(0, UnitMultiplier.Mili) }
                            .Concat(measpointarr.Skip(i * 6).Take(6)).ToArray();
                        if (i > 0) DataRow.Add(operation);
                    }
                }, cancellationToken);
                
            };
            operation.ErrorCalculation = (expected, getting) =>
            {
                var a = expected.Select((t, index) => t - getting[index]).ToArray();

                var res = (a.Max() - a.Min()).MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.Micro);
                return new[] { new MeasPoint<Length>((Length) res) };
            };

            DataRow.Add(operation);
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new []{ "Участок индикатора", "0", "20", "40", "60", "80", "100", "На любом участке 0.1 мм", "На допуск любом участке в 0.1 мм"} 
                ;
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            
            return base.FillData();
        }
    }
    #endregion

}
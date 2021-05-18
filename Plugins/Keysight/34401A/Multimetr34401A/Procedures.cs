using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AP.Extension;
using AP.Math;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Interface.SourceAndMeter;
using DevExpress.Mvvm;
using Ivi.Visa;
using NLog;
using Current = ASMC.Data.Model.PhysicalQuantity.Current;

namespace Multimetr34401A
{
    /// <summary>
    ///     Предоставляет базовую реализацию для пунктов поверки
    /// </summary>
    /// <typeparam name="TOperation"></typeparam>
    public abstract class OperationBase<TOperation> : ParagraphBase<TOperation>
    {
        #region Property

        protected ICalibratorMultimeterFlukeBase Clalibrator { get; set; }

        protected BaseDigitalMultimetr344xx Multimetr { get; set; }

        #endregion

      
        /// <inheritdoc />
        protected OperationBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected (MeasPoint<TPhysicalQuantity>, IOTimeoutException) BodyWork<TPhysicalQuantity>(
            IMeterPhysicalQuantity<TPhysicalQuantity> metr, ISourceOutputControl sourse,
            Logger logger, CancellationTokenSource _token)
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            CatchException<IOTimeoutException>(() => sourse.OutputOn(), _token, logger);
            Thread.Sleep(1000);
            (MeasPoint<TPhysicalQuantity>, IOTimeoutException) result;
            try
            {
                result = CatchException<IOTimeoutException, MeasPoint<TPhysicalQuantity>>(
                    () => metr.GetValue(), _token, logger);
                result.Item1.Round(
                    MathStatistics.GetMantissa(metr.RangeStorage.SelectRange.AccuracyChatacteristic.Resolution));
            }
            finally
            {
                CatchException<IOTimeoutException>(() => sourse.OutputOff(), _token, logger);
            }

            return result;
        }

        protected IPhysicalRange<T> InitWork<T>(IMeterPhysicalQuantity<T> mert, ISourcePhysicalQuantity<T> sourse,
            MeasPoint<T> setPoint, Logger loger, CancellationTokenSource _token)
            where T : class, IPhysicalQuantity<T>, new()
        {
            mert.RangeStorage.SetRange(setPoint);
            mert.RangeStorage.IsAutoRange = false;
            CatchException<IOTimeoutException>(() => mert.Setting(), _token, loger);
            CatchException<IOTimeoutException>(() => sourse.SetValue(setPoint), _token, loger);
            return mert.RangeStorage.SelectRange;
        }

        protected Task<bool> CompliteWorkAsync<T>(IMeasuringOperation<T> operation)
        {
            if (operation.IsGood == null || operation.IsGood(operation.Getting))
                return Task.FromResult(operation.IsGood == null || operation.IsGood(operation.Getting));

            return ShowQuestionMessage(operation.ToString()) == MessageResult.No
                ? Task.FromResult(true)
                : Task.FromResult(operation.IsGood == null || operation.IsGood(operation.Getting));

            MessageResult ShowQuestionMessage(string message)
            {
                return UserItemOperation.ServicePack.MessageBox()
                    .Show($"Текущая точка {message} не проходит по допуску:\n" +
                          "Повторить измерение этой точки?",
                        "Информация по текущему измерению",
                        MessageButton.YesNo, MessageIcon.Question,
                        MessageResult.Yes);
            }
        }

        protected bool ChekedOperation<T>(IBasicOperationVerefication<MeasPoint<T>> operation)
            where T : class, IPhysicalQuantity<T>, new()
        {
           
                return operation.Getting <= operation.UpperTolerance &&
                       operation.Getting >= operation.LowerTolerance;
        }

        /// <summary>
        ///     Позволяет получить погрешность для указанной точки.
        /// </summary>
        /// <typeparam name="T">
        ///     Физическая величина <see cref="IPhysicalQuantity" /> для которой необходима получить погрешность.
        /// </typeparam>
        /// <param name="rangeStorage">Диапазон на котором определяется погрешность.</param>
        /// <param name="expected">Точка на диапазоне для которой определяется погрешность.</param>
        /// <returns></returns>
        protected MeasPoint<T> AllowableError<T>(IRangePhysicalQuantity<T> rangeStorage, MeasPoint<T> expected)
            where T : class, IPhysicalQuantity<T>, new()
        {
            rangeStorage.SetRange(expected);
            var toll= rangeStorage.SelectRange.AccuracyChatacteristic.GetAccuracy(
                expected.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                rangeStorage.SelectRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi());
            return new MeasPoint<T>(toll);
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Предел",
                "Ожидаемое значение",
                "Измеренное значение",
                "Минимальное допустимое значение",
                "Максимальное допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <summary>
        ///     Создает схему
        /// </summary>
        /// <param name="filename">Имя файла с расширением</param>
        /// <param name="number">Номер схемы</param>
        /// <returns></returns>
        protected SchemeImage ShemeGeneration(string filename, int number)
        {
            return new SchemeImage
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема. " + Name,
                Number = number,
                FileName = filename,
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name;
        }

        protected override void ConnectionToDevice()
        {
            Clalibrator = (ICalibratorMultimeterFlukeBase) GetSelectedDevice<ICalibratorMultimeterFlukeBase>();
            Clalibrator.StringConnection = GetStringConnect(Clalibrator);
            Clalibrator.Initialize();
            Multimetr = (Keysight34401A) GetSelectedDevice<Keysight34401A>();
            Multimetr.StringConnection = GetStringConnect(Multimetr);
            Multimetr.Initialize();
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = data.NewRow();
                var dds = row as BasicOperationVerefication<TOperation>;
                if (dds == null) continue;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Name?.ToString();
                dataRow[1] = dds.Expected?.ToString();
                dataRow[2] = dds.Getting?.ToString();
                dataRow[3] = dds.LowerTolerance?.ToString();
                dataRow[4] = dds.UpperTolerance?.ToString();
                dataRow[5] = string.IsNullOrWhiteSpace(dds.Comment) ? dds.IsGood(dds.Getting) ? ConstGood : ConstBad : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        #endregion Methods
    }

    public abstract class MultiPointOperation<T1, T2> : OperationBase<MeasPoint<T1, T2>>
        where T1 : class, IPhysicalQuantity<T1>, new() where T2 : class, IPhysicalQuantity<T2>, new()
    {
        /// <inheritdoc />
        protected MultiPointOperation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = base.FillData();
            data.Rows.Clear();

            foreach (var row in DataRow)
            {
                var dataRow = data.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint<T1, T2>>;
                if (dds == null) continue;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Name;
                dataRow[1] = dds.Expected?.Description;
                dataRow[2] = dds.Getting?.MainPhysicalQuantity.ToString();
                dataRow[3] = dds.LowerTolerance?.MainPhysicalQuantity.ToString();
                dataRow[4] = dds.UpperTolerance?.MainPhysicalQuantity.ToString();
                //dataRow[4] = string.IsNullOrWhiteSpace(dds.Comment) ? dds.IsGood() ? ConstGood : ConstBad : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        protected (MeasPoint<T1>, IOTimeoutException) BodyWork(
            IMeterPhysicalQuantity<T1, T2> mert, ISourceOutputControl sourse,
            Logger logger, CancellationTokenSource _token) 

        {
            CatchException<IOTimeoutException>(() => sourse.OutputOn(), _token, logger);
            (MeasPoint<T1>, IOTimeoutException) result;
            try
            {
                Thread.Sleep(2000);
                result = CatchException<IOTimeoutException, MeasPoint<T1>>(
                    () => mert.GetValue(), _token, logger);
            }
            finally
            {
                CatchException<IOTimeoutException>(() => sourse.OutputOff(), _token, logger);
            }

            return result;
        }
        protected (MeasPoint<T1>, IOTimeoutException) BodyWork(
            IMeterPhysicalQuantity<T1> metr, ISourceOutputControl sourse,
            Logger logger, CancellationTokenSource _token)

        {
            CatchException<IOTimeoutException>(() => sourse.OutputOn(), _token, logger);
            (MeasPoint<T1>, IOTimeoutException) result;
            try
            {
                Thread.Sleep(1000);
                result = CatchException<IOTimeoutException, MeasPoint<T1>>(
                    () => metr.GetValue(), _token, logger);
                result.Item1.Round(
                    MathStatistics.GetMantissa(metr.RangeStorage.SelectRange.AccuracyChatacteristic.Resolution));
            }
            finally
            {
                CatchException<IOTimeoutException>(() => sourse.OutputOff(), _token, logger);
            }

            return result;
        }

        protected IPhysicalRange<T1, T2> InitWork(IMeterPhysicalQuantity<T1, T2> multimetr,
            ISourcePhysicalQuantity<T1, T2> sourse,
            MeasPoint<T1, T2> setPoint, Logger loger, CancellationTokenSource _token, bool isAutoRange = false)
        {
            multimetr.RangeStorage.SetRange(setPoint);
            multimetr.RangeStorage.IsAutoRange = isAutoRange;
            if (multimetr is IAcFilter<T1,T2> cast)
            {
                cast.Filter.SetFilter(setPoint);
            }
            CatchException<IOTimeoutException>(() => multimetr.Setting(), _token, loger);
            CatchException<IOTimeoutException>(() => sourse.SetValue(setPoint), _token, loger);
            return multimetr.RangeStorage.SelectRange;
        }
      
        /// <summary>
        ///     Позволяет получить погрешность для указанной точки.
        /// </summary>
        /// <typeparam name="T">
        ///     Физическая величина для которой необходима получить погрешность <see cref="IPhysicalQuantity" />
        /// </typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="rangeStorage">Диапазон на котором определяется погрешность.</param>
        /// <param name="expected">Точка на диапазоне для которой определяется погрешность.</param>
        /// <returns></returns>
        protected MeasPoint<T1, T2> AllowableError(IRangePhysicalQuantity<T1, T2> rangeStorage,
            MeasPoint<T1, T2> expected) 
        {
            rangeStorage.SetRange(expected);
            var toll=rangeStorage.SelectRange.AccuracyChatacteristic.GetAccuracy(
                expected.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                rangeStorage.SelectRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi());
           return  ConvertMeasPoint(new MeasPoint<T1>(toll), expected);
        }

        protected bool ChekedOperation(IBasicOperationVerefication<MeasPoint<T1, T2>> operation)
        {
            return operation.Getting <= operation.UpperTolerance &&
                   operation.Getting >= operation.LowerTolerance;
        }

        protected MeasPoint<T1, T2> ConvertMeasPoint(MeasPoint<T1> gettingMeasPoint,
            MeasPoint<T1, T2> exepectedMeasPoint)
        {
            return new MeasPoint<T1, T2>(gettingMeasPoint.MainPhysicalQuantity,
                exepectedMeasPoint.AdditionalPhysicalQuantity);
        }
    }

    /// <summary>
    ///     Предоставляет реализацию внешнего осмотра.
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
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Результат внешнего осмотра"};
        }

        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, "Внешний осмотр"));
        }
    }

    /// <summary>
    ///     Предоставляет операцию опробования.
    /// </summary>
    public sealed class Testing : OperationBase<bool>
    {
        /// <inheritdoc />
        public Testing(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.InsetrTextByMark.GetStringValue() + GetType().Name;
        }

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Результат опробования"};
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, Test, "Внешний осмотр"));
        }

        private async Task<bool> Test()
        {
            try
            {
                return await Task.Factory.StartNew(() => Multimetr.SelfTest(), CancellationToken.None);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = base.FillData();

            var dataRow = data.NewRow();
            if (DataRow.Count != 1) return data;

            var dds = DataRow[0] as BasicOperation<bool>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting ? "соответствует требованиям" : dds.Comment;
            data.Rows.Add(dataRow);

            return data;
        }
    }

    public sealed class DcVoltageError : OperationBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public DcVoltageError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности DCV";
            Sheme = ShemeGeneration("34401A_Cal_Volt.jpg", 0);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef = new[]
            {
                new MeasPoint<Voltage>(0.1m), new MeasPoint<Voltage>(-0.1m), new MeasPoint<Voltage>(1),
                new MeasPoint<Voltage>(-1), new MeasPoint<Voltage>(10), new MeasPoint<Voltage>(-10),
                new MeasPoint<Voltage>(100), new MeasPoint<Voltage>(-100), new MeasPoint<Voltage>(1000),
                new MeasPoint<Voltage>(-1000)
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new ASMC.Data.Model.BasicOperationVerefication<MeasPoint<Voltage>>();

                operation.Expected = setPoint;
                operation.InitWorkAsync = () =>
                {
                    var range = InitWork(Multimetr.DcVoltage, Clalibrator.DcVoltage, setPoint, Logger, token);
                    operation.Name = range.End.Description;
                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        operation.Getting = BodyWork(Multimetr.DcVoltage, Clalibrator, Logger, token).Item1;
                    }, cancellationToken);
                  
                };
                operation.ErrorCalculation = (point, measPoint) => null;
                operation.LowerCalculation = expected => expected - AllowableError(Multimetr.DcVoltage.RangeStorage, (MeasPoint<Voltage>) expected.Abs());

                operation.UpperCalculation = expected => expected + AllowableError(Multimetr.DcVoltage.RangeStorage, (MeasPoint<Voltage>)expected.Abs());

                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);

                operation.IsGood= (getting) => ChekedOperation(operation);

                DataRow.Add(operation);
            }
        }
    }

    public sealed class FrequencyError : MultiPointOperation<Frequency,Voltage>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public FrequencyError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности Частоты";
            Sheme = ShemeGeneration("34401A_Cal_Volt.jpg", 0);
        }
        
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef = new[]
            {
                new MeasPoint<Frequency, Voltage>(100, UnitMultiplier.Kilo ,1),
                new MeasPoint<Frequency,Voltage>(100, UnitMultiplier.None,10, UnitMultiplier.Mili)
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Frequency, Voltage>> {Expected = setPoint};

                operation.InitWorkAsync = () =>
                {
                   var range=  InitWork(Multimetr.Frequency, Clalibrator.Frequency, setPoint, Logger, token, true);
                    operation.Name = range.End.Description;
                    return Task.CompletedTask;
                };

                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        var result = BodyWork(Multimetr.Frequency, Clalibrator, Logger, token).Item1;
                        operation.Getting = ConvertMeasPoint(result, operation.Expected);
                    }, cancellationToken);
                  
                };
                operation.ErrorCalculation = (point, measPoint) => null;
                operation.LowerCalculation = expected =>
                    expected - AllowableError(Multimetr.Frequency.RangeStorage, expected);
                operation.UpperCalculation = expected =>
                    expected + AllowableError(Multimetr.Frequency.RangeStorage, expected);
                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood= (getting) => ChekedOperation(operation);
                DataRow.Add(operation);
            }
        }
    }
    public sealed class DcCurrentError : OperationBase<MeasPoint<Current>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public DcCurrentError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности DCI";
            Sheme = ShemeGeneration("34401A_Cal_Current.jpg", 1);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef = new[]
            {
                new MeasPoint<Current>(10, UnitMultiplier.Mili), new MeasPoint<Current>(100, UnitMultiplier.Mili), new MeasPoint<Current>(1),
                new MeasPoint<Current>(2)
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Current>> {Expected = setPoint};

                operation.InitWorkAsync = () =>
                {
                    var range= InitWork(Multimetr.DcCurrent, Clalibrator.DcCurrent, setPoint, Logger, token);
                    operation.Name = range.End.Description;
                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        operation.Getting = BodyWork(Multimetr.DcCurrent, Clalibrator, Logger, token).Item1;
                    }, cancellationToken);
                  
                };
                operation.ErrorCalculation = (point, measPoint) => null;
                operation.LowerCalculation = expected =>
                    expected - AllowableError(Multimetr.DcCurrent.RangeStorage, expected);

                operation.UpperCalculation = expected =>
                    expected + AllowableError(Multimetr.DcCurrent.RangeStorage, expected);

                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);

                operation.IsGood= (getting) => ChekedOperation(operation);

                DataRow.Add(operation);
            }
        }
    }

    public sealed class Resistance4WError : OperationBase<MeasPoint<Resistance>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public Resistance4WError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности сопротивления 4W";
            Sheme = ShemeGeneration("34401A_Cal_4W.jpg", 2);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var cal4W = Clalibrator as IResistance4W;
            var voltRef = new[]
            {
                new MeasPoint<Resistance>(100), new MeasPoint<Resistance>(1, UnitMultiplier.Kilo), new MeasPoint<Resistance>(10, UnitMultiplier.Kilo),
                new MeasPoint<Resistance>(100, UnitMultiplier.Kilo) 
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Resistance>> {Expected = setPoint};

                operation.InitWorkAsync = () =>
                {
                    var range= InitWork(Multimetr.Resistance4W, cal4W.Resistance4W, setPoint, Logger, token);
                    operation.Name = range.End.Description;
                    return Task.CompletedTask;
                };
                operation.ErrorCalculation = (point, measPoint) => null;
                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        operation.Getting = BodyWork(Multimetr.Resistance4W, Clalibrator, Logger, token).Item1;
                    }, cancellationToken);
                  
                };

                operation.LowerCalculation = expected =>
                    expected - AllowableError(Multimetr.Resistance4W.RangeStorage, expected);
                operation.UpperCalculation = expected =>
                    expected + AllowableError(Multimetr.Resistance4W.RangeStorage, expected);

                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood= (getting) => ChekedOperation(operation);

                DataRow.Add(operation);
            }
        }
    }

    public sealed class Resistance2WError : OperationBase<MeasPoint<Resistance>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public Resistance2WError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности сопротивления";
            Sheme = ShemeGeneration("34401A_Cal_Volt.jpg", 0);
        }

        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef = new[]
            {
                new MeasPoint<Resistance>(1, UnitMultiplier.Mega), new MeasPoint<Resistance>(10, UnitMultiplier.Mega),new MeasPoint<Resistance>(100, UnitMultiplier.Mega)
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Resistance>>();

                operation.Expected = setPoint;
                operation.InitWorkAsync = () =>
                {
                   var range= InitWork(Multimetr.Resistance2W, Clalibrator.Resistance2W, setPoint, Logger, token);
                   operation.Name = range.End.Description;
                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        operation.Getting = BodyWork(Multimetr.Resistance2W, Clalibrator, Logger, token)
                            .Item1;
                    }, cancellationToken);
                   
                };
                operation.LowerCalculation = expected =>
                    expected - AllowableError(Multimetr.Resistance2W.RangeStorage, expected);
                operation.UpperCalculation = expected =>
                    expected + AllowableError(Multimetr.Resistance2W.RangeStorage, expected);
                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood = (getting) => ChekedOperation(operation);
                DataRow.Add(operation);
            }
        }
    }

    public sealed class AcVoltageError : MultiPointOperation<Voltage, Frequency>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public AcVoltageError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности ACV";
            Sheme = ShemeGeneration("34401A_Cal_Volt.jpg", 0);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef = new[]
            {
                new MeasPoint<Voltage, Frequency>(10, UnitMultiplier.Mili, 1, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100, UnitMultiplier.Mili, 1, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100, UnitMultiplier.Mili, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1, UnitMultiplier.None, 1, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1,UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(10,UnitMultiplier.None, 0.01m, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(10,UnitMultiplier.None, 1, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(10,UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100,UnitMultiplier.None, 1, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100,UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(750,UnitMultiplier.None, 1, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(750,UnitMultiplier.None, 50, UnitMultiplier.Kilo)
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage, Frequency>>();

                operation.Expected = setPoint;
                operation.InitWorkAsync = () =>
                {
                    var range= InitWork(Multimetr.AcVoltage, Clalibrator.AcVoltage, setPoint, Logger, token);
                    operation.Name = range.End.Description;
                    return Task.CompletedTask;
                };

                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        var result = BodyWork(Multimetr.AcVoltage, Clalibrator, Logger, token).Item1;
                        operation.Getting = ConvertMeasPoint(result, operation.Expected);
                    }, cancellationToken);
                  
                };
                operation.ErrorCalculation = (point, measPoint) => null;
                operation.LowerCalculation = expected =>
                    expected - AllowableError(Multimetr.AcVoltage.RangeStorage, expected);
                operation.UpperCalculation = expected =>
                    expected + AllowableError(Multimetr.AcVoltage.RangeStorage, expected);
                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood= (getting) => ChekedOperation(operation);
                DataRow.Add(operation);
            }
        }
    }

    public sealed class AcCurrentError : MultiPointOperation<Current, Frequency>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public AcCurrentError(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности ACI";
            Sheme = ShemeGeneration("34401A_Cal_Current.jpg", 1);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef = new[]
            {
                new MeasPoint<Current, Frequency>(1, UnitMultiplier.None, 1, UnitMultiplier.Kilo),
                new MeasPoint<Current, Frequency>(2, UnitMultiplier.None, 1,UnitMultiplier.Kilo)
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Current, Frequency>>();

                operation.Expected = setPoint;
                operation.InitWorkAsync = () =>
                {
                    var range= InitWork(Multimetr.AcCurrent, Clalibrator.AcCurrent, setPoint, Logger, token);
                    operation.Name = range.End.MainPhysicalQuantity.ToString();
                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        var result = BodyWork(Multimetr.AcCurrent, Clalibrator, Logger, token).Item1;
                        operation.Getting = ConvertMeasPoint(result, operation.Expected);
                    }, cancellationToken);
                 
                };
                operation.ErrorCalculation = (point, measPoint) => null;
                operation.LowerCalculation = expected =>
                    expected - AllowableError(Multimetr.AcCurrent.RangeStorage, expected);
                operation.UpperCalculation = expected =>
                    expected + AllowableError(Multimetr.AcCurrent.RangeStorage, expected);

                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood= (getting) => ChekedOperation(operation);
                DataRow.Add(operation);
            }
        }
    }
}
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using DevExpress.Mvvm;
using Ivi.Visa;
using NLog;
using Current = ASMC.Data.Model.PhysicalQuantity.Current;

namespace Multimetr34401A
{
    /// <summary>
    ///     Придоставляет базувую реализацию для пунктов поверки
    /// </summary>
    /// <typeparam name="TOperation"></typeparam>
    public abstract class OperationBase<TOperation> : ParagraphBase<TOperation>
    {
        #region Property

        protected ICalibratorMultimeterFlukeBase Clalibrator { get; set; }

        protected BaseDigitalMultimetr344xx Multimetr { get; set; }

        #endregion

        #region Field

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        /// <inheritdoc />
        protected OperationBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }


        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Ожидаемое значение",
                "Измеренное значение",
                "Минимальное допустимое значение",
                "Максимальное допустимое значение"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <summary>
        ///     Создает схему
        /// </summary>
        /// <param name="filename">Имя файла с разширением</param>
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
            Multimetr = (Keysight34401A) GetSelectedDevice<Keysight34401A>();
            Multimetr.StringConnection = GetStringConnect(Multimetr);
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
                dataRow[0] = dds.Getting.ToString();
                dataRow[1] = dds.Expected.ToString();
                dataRow[2] = dds.LowerTolerance.ToString();
                dataRow[3] = dds.UpperTolerance.ToString();
                dataRow[4] = string.IsNullOrWhiteSpace(dds.Comment) ? dds.IsGood() ? ConstGood : ConstBad : dds.Comment;
                data.Rows.Add(dataRow);
            }


            return data;
        }

        #endregion
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

            foreach (var row in DataRow)
            {
                var dataRow = data.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint<T1, T2>>;
                if (dds == null) continue;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Getting.ToString();
                dataRow[1] = dds.Expected.MainPhysicalQuantity.ToString();
                dataRow[2] = dds.LowerTolerance.MainPhysicalQuantity.ToString();
                dataRow[3] = dds.UpperTolerance.MainPhysicalQuantity.ToString();
                dataRow[4] = string.IsNullOrWhiteSpace(dds.Comment) ? dds.IsGood() ? ConstGood : ConstBad : dds.Comment;
                data.Rows.Add(dataRow);
            }


            return data;
        }

        protected MeasPoint<T1, T2> ConvertMeasPoint(MeasPoint<T1> gettingMeasPoint,
            MeasPoint<T1, T2> exepectedMeasPoint)
        {
            return new MeasPoint<T1, T2>(gettingMeasPoint.MainPhysicalQuantity,
                exepectedMeasPoint.AdditionalPhysicalQuantity);
        }
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
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Результат внешнего оснотра"};
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

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] {"Результат опробывания"};
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
            Name = "Определение погрешности DC";
            Sheme = ShemeGeneration("", 0);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef = new[]
            {
                new MeasPoint<Voltage>(0.1m), new MeasPoint<Voltage>(1), new MeasPoint<Voltage>(10),
                new MeasPoint<Voltage>(100), new MeasPoint<Voltage>(1000)
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();

                operation.Expected = setPoint;
                operation.InitWorkAsync = () =>
                {
                    Multimetr.DcVoltage.RangeStorage.SetRange(setPoint);
                    Multimetr.DcVoltage.RangeStorage.IsAutoRange = false;
                    CatchException<IOTimeoutException>(() => Multimetr.DcVoltage.Setting(), token, Logger);
                    CatchException<IOTimeoutException>(() => Clalibrator.DcVoltage.SetValue(setPoint), token, Logger);

                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    CatchException<IOTimeoutException>(() => Clalibrator.DcVoltage.OutputOn(), token, Logger);
                    (MeasPoint<Voltage>, IOTimeoutException) result;
                    try
                    {
                        result = CatchException<IOTimeoutException, MeasPoint<Voltage>>(
                            () => Multimetr.DcVoltage.GetValue(), token, Logger);
                    }
                    finally
                    {
                        CatchException<IOTimeoutException>(() => Clalibrator.DcVoltage.OutputOff(), token, Logger);
                    }

                    operation.Getting = result.Item1;
                };
                //operation.LowerCalculation = expected =>
                //{

                //};
                operation.CompliteWorkAsync = () =>
                {
                    if (operation.IsGood == null || operation.IsGood())
                        return Task.FromResult(operation.IsGood == null || operation.IsGood());
                    var answer =
                        UserItemOperation.ServicePack.MessageBox()
                            .Show($"Текущая точка {operation} не проходит по допуску:\n" +
                                  "Повторить измерение этой точки?",
                                "Информация по текущему измерению",
                                MessageButton.YesNo, MessageIcon.Question,
                                MessageResult.Yes);

                    return answer == MessageResult.No
                        ? Task.FromResult(true)
                        : Task.FromResult(operation.IsGood == null || operation.IsGood());
                };
                operation.IsGood = () => operation.Getting <= operation.UpperTolerance &&
                                         operation.Getting >= operation.LowerTolerance;
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
            Sheme = ShemeGeneration("", 0);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var cal4W = Clalibrator as IResistance4W;
            var voltRef = new[]
            {
                new MeasPoint<Resistance>(0.1m), new MeasPoint<Resistance>(1), new MeasPoint<Resistance>(10),
                new MeasPoint<Resistance>(100), new MeasPoint<Resistance>(1000)
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Resistance>>();

                operation.Expected = setPoint;
                operation.InitWorkAsync = () =>
                {
                    Multimetr.Resistance4W.RangeStorage.SetRange(setPoint);
                    Multimetr.Resistance4W.RangeStorage.IsAutoRange = false;
                    CatchException<IOTimeoutException>(() => Multimetr.Resistance4W.Setting(), token, Logger);
                    CatchException<IOTimeoutException>(() => cal4W.Resistance4W.SetValue(setPoint), token,
                        Logger);

                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    CatchException<IOTimeoutException>(() => cal4W.Resistance4W.OutputOn(), token, Logger);
                    (MeasPoint<Resistance>, IOTimeoutException) result;
                    try
                    {
                        result = CatchException<IOTimeoutException, MeasPoint<Resistance>>(
                            () => Multimetr.Resistance4W.GetValue(), token, Logger);
                    }
                    finally
                    {
                        CatchException<IOTimeoutException>(() => cal4W.Resistance4W.OutputOff(), token, Logger);
                    }

                    operation.Getting = result.Item1;
                };
                operation.CompliteWorkAsync = () =>
                {
                    if (operation.IsGood == null || operation.IsGood())
                        return Task.FromResult(operation.IsGood == null || operation.IsGood());
                    var answer =
                        UserItemOperation.ServicePack.MessageBox()
                            .Show($"Текущая точка {operation} не проходит по допуску:\n" +
                                  "Повторить измерение этой точки?",
                                "Информация по текущему измерению",
                                MessageButton.YesNo, MessageIcon.Question,
                                MessageResult.Yes);

                    return answer == MessageResult.No
                        ? Task.FromResult(true)
                        : Task.FromResult(operation.IsGood == null || operation.IsGood());
                };
                operation.IsGood = () => operation.Getting <= operation.UpperTolerance &&
                                         operation.Getting >= operation.LowerTolerance;
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
            Sheme = ShemeGeneration("", 0);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef = new[]
            {
                new MeasPoint<Resistance>(0.1m), new MeasPoint<Resistance>(1), new MeasPoint<Resistance>(10),
                new MeasPoint<Resistance>(100), new MeasPoint<Resistance>(1000)
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Resistance>>();

                operation.Expected = setPoint;
                operation.InitWorkAsync = () =>
                {
                    Multimetr.Resistance2W.RangeStorage.SetRange(setPoint);
                    Multimetr.Resistance2W.RangeStorage.IsAutoRange = false;
                    CatchException<IOTimeoutException>(() => Multimetr.Resistance2W.Setting(), token, Logger);
                    CatchException<IOTimeoutException>(() => Clalibrator.Resistance2W.SetValue(setPoint), token,
                        Logger);

                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    CatchException<IOTimeoutException>(() => Clalibrator.Resistance2W.OutputOn(), token, Logger);
                    (MeasPoint<Resistance>, IOTimeoutException) result;
                    try
                    {
                        result = CatchException<IOTimeoutException, MeasPoint<Resistance>>(
                            () => Multimetr.Resistance2W.GetValue(), token, Logger);
                    }
                    finally
                    {
                        CatchException<IOTimeoutException>(() => Clalibrator.Resistance2W.OutputOff(), token, Logger);
                    }

                    operation.Getting = result.Item1;
                };
                operation.CompliteWorkAsync = () =>
                {
                    if (operation.IsGood == null || operation.IsGood())
                        return Task.FromResult(operation.IsGood == null || operation.IsGood());
                    var answer =
                        UserItemOperation.ServicePack.MessageBox()
                            .Show($"Текущая точка {operation} не проходит по допуску:\n" +
                                  "Повторить измерение этой точки?",
                                "Информация по текущему измерению",
                                MessageButton.YesNo, MessageIcon.Question,
                                MessageResult.Yes);

                    return answer == MessageResult.No
                        ? Task.FromResult(true)
                        : Task.FromResult(operation.IsGood == null || operation.IsGood());
                };
                operation.IsGood = () => operation.Getting <= operation.UpperTolerance &&
                                         operation.Getting >= operation.LowerTolerance;
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
            Name = "Определение погрешности AC";
            Sheme = ShemeGeneration("", 0);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef = new[]
            {
                new MeasPoint<Voltage, Frequency>(10, UnitMultiplier.Mili, 1),
                new MeasPoint<Voltage, Frequency>(100, UnitMultiplier.Mili, 1),
                new MeasPoint<Voltage, Frequency>(100, UnitMultiplier.Mili, 50),
                new MeasPoint<Voltage, Frequency>(1, 1),
                new MeasPoint<Voltage, Frequency>(1, 50),
                new MeasPoint<Voltage, Frequency>(10, 0.01m),
                new MeasPoint<Voltage, Frequency>(10, 1),
                new MeasPoint<Voltage, Frequency>(10, 50),
                new MeasPoint<Voltage, Frequency>(100, 1),
                new MeasPoint<Voltage, Frequency>(100, 50),
                new MeasPoint<Voltage, Frequency>(750, 1),
                new MeasPoint<Voltage, Frequency>(750, 50)
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage, Frequency>>();

                operation.Expected = setPoint;
                operation.InitWorkAsync = () =>
                {
                    Multimetr.AcVoltage.RangeStorage.SetRange(setPoint);
                    Multimetr.AcVoltage.Filter.SetFilter(setPoint);
                    Multimetr.AcVoltage.RangeStorage.IsAutoRange = false;
                    CatchException<IOTimeoutException>(() => Multimetr.AcVoltage.Setting(), token, Logger);
                    CatchException<IOTimeoutException>(() => Clalibrator.AcVoltage.SetValue(setPoint), token, Logger);

                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    CatchException<IOTimeoutException>(() => Clalibrator.AcVoltage.OutputOn(), token, Logger);
                    (MeasPoint<Voltage>, IOTimeoutException) result;
                    try
                    {
                        result = CatchException<IOTimeoutException, MeasPoint<Voltage>>(
                            () => Multimetr.AcVoltage.GetValue(), token, Logger);
                    }
                    finally
                    {
                        CatchException<IOTimeoutException>(() => Clalibrator.AcVoltage.OutputOff(), token, Logger);
                    }

                    operation.Getting = ConvertMeasPoint(result.Item1, operation.Expected);
                };
                operation.CompliteWorkAsync = () =>
                {
                    if (operation.IsGood == null || operation.IsGood())
                        return Task.FromResult(operation.IsGood == null || operation.IsGood());
                    var answer =
                        UserItemOperation.ServicePack.MessageBox()
                            .Show($"Текущая точка {operation} не проходит по допуску:\n" +
                                  "Повторить измерение этой точки?",
                                "Информация по текущему измерению",
                                MessageButton.YesNo, MessageIcon.Question,
                                MessageResult.Yes);

                    return answer == MessageResult.No
                        ? Task.FromResult(true)
                        : Task.FromResult(operation.IsGood == null || operation.IsGood());
                };
                operation.IsGood = () => operation.Getting <= operation.UpperTolerance &&
                                         operation.Getting >= operation.LowerTolerance;
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
            Name = "Определение погрешности AC";
            Sheme = ShemeGeneration("", 0);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var voltRef = new[]
            {
                new MeasPoint<Current, Frequency>(10, UnitMultiplier.Mili, 1),
                new MeasPoint<Current, Frequency>(100, UnitMultiplier.Mili, 1),
            };
            foreach (var setPoint in voltRef)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Current, Frequency>>();

                operation.Expected = setPoint;
                operation.InitWorkAsync = () =>
                {
                    Multimetr.AcCurrent.RangeStorage.SetRange(setPoint);
                    Multimetr.AcCurrent.Filter.SetFilter(setPoint);
                    Multimetr.AcCurrent.RangeStorage.IsAutoRange = false;
                    CatchException<IOTimeoutException>(() => Multimetr.AcCurrent.Setting(), token, Logger);
                    CatchException<IOTimeoutException>(() => Clalibrator.AcCurrent.SetValue(setPoint), token, Logger);

                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    CatchException<IOTimeoutException>(() => Clalibrator.AcVoltage.OutputOn(), token, Logger);
                    (MeasPoint<Current>, IOTimeoutException) result;
                    try
                    {
                        result = CatchException<IOTimeoutException, MeasPoint<Current>>(
                            () => Multimetr.AcCurrent.GetValue(), token, Logger);
                    }
                    finally
                    {
                        CatchException<IOTimeoutException>(() => Clalibrator.AcCurrent.OutputOff(), token, Logger);
                    }

                    operation.Getting = ConvertMeasPoint(result.Item1, operation.Expected);
                };
                operation.CompliteWorkAsync = () =>
                {
                    if (operation.IsGood == null || operation.IsGood())
                        return Task.FromResult(operation.IsGood == null || operation.IsGood());
                    var answer =
                        UserItemOperation.ServicePack.MessageBox()
                            .Show($"Текущая точка {operation} не проходит по допуску:\n" +
                                  "Повторить измерение этой точки?",
                                "Информация по текущему измерению",
                                MessageButton.YesNo, MessageIcon.Question,
                                MessageResult.Yes);

                    return answer == MessageResult.No
                        ? Task.FromResult(true)
                        : Task.FromResult(operation.IsGood == null || operation.IsGood());
                };
                operation.IsGood = () => operation.Getting <= operation.UpperTolerance &&
                                         operation.Getting >= operation.LowerTolerance;
                DataRow.Add(operation);
            }
        }
    }
}
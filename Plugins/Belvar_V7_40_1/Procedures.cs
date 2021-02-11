using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using NLog;

namespace Belvar_V7_40_1
{
    /// <summary>
    /// Предоставляет реализацию внешнего осномотра.
    /// </summary>
    public sealed class VisualInspection : OperationBase<bool>
    {
        /// <inheritdoc />
        public VisualInspection(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
        }

        #region Methods

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
            DataRow.Add(new DialogOperationHelp(this, "V740_1_VisualTest.rtf"));
        }

        #endregion
    }

    /// <summary>
    /// Предоставляет операцию опробывания.
    /// </summary>
    public sealed class Testing : OperationBase<bool>
    {
        /// <inheritdoc />
        public Testing(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
        }

        #region Methods

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

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, "V740_1_Oprobovanie.rtf"));
        }

        #endregion
    }

    public sealed class DcvTest : OperationBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DcvTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного напряжения";
        }

        #region Methods

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            //поверяемые точки
            var testPoint = new[]
            {
                new MeasPoint<Voltage>(0.07M, UnitMultiplier.Mili),
                new MeasPoint<Voltage>(2, UnitMultiplier.Mili),
                new MeasPoint<Voltage>(50, UnitMultiplier.Mili),
                new MeasPoint<Voltage>(100, UnitMultiplier.Mili),
                new MeasPoint<Voltage>(150, UnitMultiplier.Mili),
                new MeasPoint<Voltage>(190, UnitMultiplier.Mili),
                new MeasPoint<Voltage>(0.2M),
                new MeasPoint<Voltage>(0.5M),
                new MeasPoint<Voltage>(1),
                new MeasPoint<Voltage>(1.5M),
                new MeasPoint<Voltage>(1.9M),
                new MeasPoint<Voltage>(2),
                new MeasPoint<Voltage>(10),
                new MeasPoint<Voltage>(19),
                new MeasPoint<Voltage>(20),
                new MeasPoint<Voltage>(100),
                new MeasPoint<Voltage>(190),
                new MeasPoint<Voltage>(200),
                new MeasPoint<Voltage>(500),
                new MeasPoint<Voltage>(1000)
            };

            foreach (var measPoint in testPoint)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();

                operation.Expected = measPoint;
                operation.InitWorkAsync = () =>
                {
                    InitWork(Multimetr.DcVoltage, Clalibrator.DcVoltage, measPoint, Logger, token);

                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    operation.Getting = BodyWork(Multimetr.DcVoltage, Clalibrator.DcVoltage, Logger, token).Item1;
                };
                operation.ErrorCalculation = (expected, getting) => null;
                operation.LowerCalculation = expected =>
                    expected - AllowableError(Multimetr.DcVoltage.RangeStorage, expected);

                operation.UpperCalculation = expected =>
                    expected + AllowableError(Multimetr.DcVoltage.RangeStorage, expected);

                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);

                operation.IsGood = () => ChekedOperation(operation);

                DataRow.Add(operation);
            }
        }

        #endregion
    }

    public sealed class AcvTest : OperationBase<MeasPoint<Voltage>>
    {
        public AcvTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения переменного напряжения";
        }

        #region Methods

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var testPoint = new[]
            {
                #region 200 мВ

                new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.Mili, 20),
                new MeasPoint<Voltage, Frequency>(50, UnitMultiplier.Mili, 20),
                new MeasPoint<Voltage, Frequency>(100, UnitMultiplier.Mili, 20),
                new MeasPoint<Voltage, Frequency>(150, UnitMultiplier.Mili, 20),
                new MeasPoint<Voltage, Frequency>(180, UnitMultiplier.Mili, 20),

                new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.Mili, 40),
                new MeasPoint<Voltage, Frequency>(50, UnitMultiplier.Mili, 40),
                new MeasPoint<Voltage, Frequency>(100, UnitMultiplier.Mili, 40),
                new MeasPoint<Voltage, Frequency>(150, UnitMultiplier.Mili, 40),
                new MeasPoint<Voltage, Frequency>(180, UnitMultiplier.Mili, 20),

                new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.Mili, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(50, UnitMultiplier.Mili, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100, UnitMultiplier.Mili, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(150, UnitMultiplier.Mili, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(180, UnitMultiplier.Mili, 10, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.Mili, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(50, UnitMultiplier.Mili, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100, UnitMultiplier.Mili, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(150, UnitMultiplier.Mili, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(180, UnitMultiplier.Mili, 20, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.Mili, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(50, UnitMultiplier.Mili, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100, UnitMultiplier.Mili, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(150, UnitMultiplier.Mili, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(180, UnitMultiplier.Mili, 50, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.Mili, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(50, UnitMultiplier.Mili, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100, UnitMultiplier.Mili, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(150, UnitMultiplier.Mili, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(180, UnitMultiplier.Mili, 100, UnitMultiplier.Kilo),

                #endregion 200 мВ

                #region 2 В

                new MeasPoint<Voltage, Frequency>(0.2M, 20),
                new MeasPoint<Voltage, Frequency>(0.5M, 20),
                new MeasPoint<Voltage, Frequency>(1.0M, 20),
                new MeasPoint<Voltage, Frequency>(1.5M, 20),
                new MeasPoint<Voltage, Frequency>(1.8M, 20),

                new MeasPoint<Voltage, Frequency>(0.2M, 40),
                new MeasPoint<Voltage, Frequency>(0.5M, 40),
                new MeasPoint<Voltage, Frequency>(1.0M, 40),
                new MeasPoint<Voltage, Frequency>(1.5M, 40),
                new MeasPoint<Voltage, Frequency>(1.8M, 20),

                new MeasPoint<Voltage, Frequency>(0.2M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(0.5M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.0M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.5M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.8M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(0.2M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(0.5M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.0M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.5M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.8M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(0.2M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(0.5M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.0M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.5M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.8M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(0.2M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(0.5M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.0M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.5M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(1.8M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),

                #endregion 2 В

                #region 20 В

                new MeasPoint<Voltage, Frequency>(2M, 20),
                new MeasPoint<Voltage, Frequency>(10M, 20),
                new MeasPoint<Voltage, Frequency>(18M, 20),

                new MeasPoint<Voltage, Frequency>(2M, 40),
                new MeasPoint<Voltage, Frequency>(10M, 40),
                new MeasPoint<Voltage, Frequency>(18M, 40),

                new MeasPoint<Voltage, Frequency>(2M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(10M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(18M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(2M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(10M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(18M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(2M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(10M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(18M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(2M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(10M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(18M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),

                #endregion 20 В

                #region 200 В

                new MeasPoint<Voltage, Frequency>(20M, 20),
                new MeasPoint<Voltage, Frequency>(100M, 20),
                new MeasPoint<Voltage, Frequency>(180M, 20),

                new MeasPoint<Voltage, Frequency>(20M, 40),
                new MeasPoint<Voltage, Frequency>(100M, 40),
                new MeasPoint<Voltage, Frequency>(180M, 40),

                new MeasPoint<Voltage, Frequency>(20M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(180M, UnitMultiplier.None, 10, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(20M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(180M, UnitMultiplier.None, 20, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(20M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(180M, UnitMultiplier.None, 50, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(20M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(100M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(180M, UnitMultiplier.None, 100, UnitMultiplier.Kilo),

                #endregion 200 В

                #region 2000 В

                //без делителя
                new MeasPoint<Voltage, Frequency>(200M, 40),
                new MeasPoint<Voltage, Frequency>(500M, 40),

                new MeasPoint<Voltage, Frequency>(200M, 500),
                new MeasPoint<Voltage, Frequency>(500M, 500),

                new MeasPoint<Voltage, Frequency>(200M, UnitMultiplier.None, 1, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(500M, UnitMultiplier.None, 1, UnitMultiplier.Kilo),

                new MeasPoint<Voltage, Frequency>(200M, UnitMultiplier.None, 5, UnitMultiplier.Kilo),
                new MeasPoint<Voltage, Frequency>(500M, UnitMultiplier.None, 5, UnitMultiplier.Kilo),

                #endregion 2000 В
            };
        }

        #endregion
    }

    public sealed class DciTest : OperationBase<MeasPoint<Current>>
    {
        public DciTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного тока";
        }

        #region Methods

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var testPoint = new[]
            {
                new MeasPoint<Current>(0.05M),
                new MeasPoint<Current>(2.0M),
                new MeasPoint<Current>(20.0M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(150M),
                new MeasPoint<Current>(190M),
                new MeasPoint<Current>(0.2M),
                new MeasPoint<Current>(1.0M),
                new MeasPoint<Current>(1.9M),
                new MeasPoint<Current>(2.0M),
                new MeasPoint<Current>(10.0M),
                new MeasPoint<Current>(19.0M),
                new MeasPoint<Current>(20M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(190M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(1000M),
                new MeasPoint<Current>(1900M),
                new MeasPoint<Current>(0.05M),
                new MeasPoint<Current>(2.0M),
                new MeasPoint<Current>(20.0M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(150M),
                new MeasPoint<Current>(190M),
                new MeasPoint<Current>(0.2M),
                new MeasPoint<Current>(1.0M),
                new MeasPoint<Current>(1.9M),
                new MeasPoint<Current>(2.0M),
                new MeasPoint<Current>(10.0M),
                new MeasPoint<Current>(19.0M),
                new MeasPoint<Current>(20M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(190M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(1000M),
                new MeasPoint<Current>(1900M),
                new MeasPoint<Current>(0.05M),
                new MeasPoint<Current>(2.0M),
                new MeasPoint<Current>(20.0M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(150M),
                new MeasPoint<Current>(190M),
                new MeasPoint<Current>(0.2M),
                new MeasPoint<Current>(1.0M),
                new MeasPoint<Current>(1.9M),
                new MeasPoint<Current>(2.0M),
                new MeasPoint<Current>(10.0M),
                new MeasPoint<Current>(19.0M),
                new MeasPoint<Current>(20M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(190M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(1000M),
                new MeasPoint<Current>(1900M),
                new MeasPoint<Current>(0.05M),
                new MeasPoint<Current>(2.0M),
                new MeasPoint<Current>(20.0M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(150M),
                new MeasPoint<Current>(190M),
                new MeasPoint<Current>(0.2M),
                new MeasPoint<Current>(1.0M),
                new MeasPoint<Current>(1.9M),
                new MeasPoint<Current>(2.0M),
                new MeasPoint<Current>(10.0M),
                new MeasPoint<Current>(19.0M),
                new MeasPoint<Current>(20M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(190M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(1000M),
                new MeasPoint<Current>(1900M),
                new MeasPoint<Current>(0.05M),
                new MeasPoint<Current>(2.0M),
                new MeasPoint<Current>(20.0M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(150M),
                new MeasPoint<Current>(190M),
                new MeasPoint<Current>(0.2M),
                new MeasPoint<Current>(1.0M),
                new MeasPoint<Current>(1.9M),
                new MeasPoint<Current>(2.0M),
                new MeasPoint<Current>(10.0M),
                new MeasPoint<Current>(19.0M),
                new MeasPoint<Current>(20M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(190M),
                new MeasPoint<Current>(100M),
                new MeasPoint<Current>(1000M),
                new MeasPoint<Current>(1900M)
            };
        }

        #endregion
    }
}
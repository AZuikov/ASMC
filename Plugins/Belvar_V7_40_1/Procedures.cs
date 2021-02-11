using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.Extension;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;
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

            var dic = new Dictionary<int, double[]>();
            dic.Add(0, new[] { 0.035, 1, 25, 50, 75, 95 });
            dic.Add(1, new[] { 5.0, 25, 50, 75, 95 });
            dic.Add(2, new[] { 10.0, 50, 95 });
            dic.Add(3, new[] { 10.0, 50, 95 });
            dic.Add(4, new[] { 10.0, 50, 95 });


            MeasPoint<Voltage>[] testPoint =  (MeasPoint<Voltage>[]) GetTestPoints(Multimetr.DcVoltage, dic);




            foreach (var measPoint in testPoint)
            {
                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();

                operation.Expected = (MeasPoint<Voltage>) measPoint;
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
                new MeasPoint<Current>(0.05M, UnitMultiplier.Micro ),
                new MeasPoint<Current>(2.0M , UnitMultiplier.Micro),
                new MeasPoint<Current>(20.0M, UnitMultiplier.Micro),
                new MeasPoint<Current>(100M , UnitMultiplier.Micro),
                new MeasPoint<Current>(150M , UnitMultiplier.Micro),
                new MeasPoint<Current>(190M , UnitMultiplier.Micro),
                
                new MeasPoint<Current>(0.2M, UnitMultiplier.Mili),
                new MeasPoint<Current>(1.0M, UnitMultiplier.Mili),
                new MeasPoint<Current>(1.9M, UnitMultiplier.Mili),
                
                new MeasPoint<Current>(2.0M, UnitMultiplier.Mili),
                new MeasPoint<Current>(10.0M, UnitMultiplier.Mili),
                new MeasPoint<Current>(19.0M, UnitMultiplier.Mili),
                
                new MeasPoint<Current>(20M, UnitMultiplier.Mili),
                new MeasPoint<Current>(100M, UnitMultiplier.Mili),
                new MeasPoint<Current>(190M, UnitMultiplier.Mili),
                
                new MeasPoint<Current>(100M, UnitMultiplier.Mili),
                new MeasPoint<Current>(1000M, UnitMultiplier.Mili),
                new MeasPoint<Current>(1900M, UnitMultiplier.Mili),
                
            };
        }

        #endregion
    }

    public sealed class AciTest : OperationBase<MeasPoint<Current,Frequency>>
    {
        public AciTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного тока";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            MeasPoint<Current,Frequency> testPoints = new MeasPoint<Current, Frequency>(0,0);

            var arr = new MeasPoint<Current,Frequency>().GetArayMeasPointsInParcent(new MeasPoint<Current,Frequency>(200))
        }
    }
}
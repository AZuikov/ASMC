﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.Extension;
using AP.Math;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;
using DevExpress.Mvvm;
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

    [TestMeasPointAttribute("Operation1: DCV", typeof(MeasPoint<Voltage>))]
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
            ConnectionToDevice();
            for (int row=0; row< TestMeasPoints.GetUpperBound(0)+1;row++)
            {
                var testingMeasureValue = TestMeasPoints[row, 1];
                var rangeToSetOnDmm = TestMeasPoints[row, 0];

                //установим пределы измерения мултиметра и воспроизведения калибратора
                Multimetr.DcVoltage.RangeStorage.SetRange(rangeToSetOnDmm);
                Calibrator.DcVoltage.RangeStorage.SetRange(testingMeasureValue);

                //если у какого-то из устройств нет подходящего диапазона?
                if (!RangeIsSet(Calibrator.DcVoltage.RangeStorage) ||
                    !RangeIsSet(Multimetr.DcVoltage.RangeStorage))
                {
                    ShowNotSupportedMeasurePointMeessage(Multimetr.DcVoltage, Calibrator.DcVoltage, rangeToSetOnDmm,
                                                         testingMeasureValue);

                    //эту точку не будем поверять, не добавляем ее в протокол и не подаем значение на приборы
                    continue;
                }

                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();


                operation.Expected =  testingMeasureValue;
                operation.InitWorkAsync = () =>
                {
                    InitWork(Multimetr.DcVoltage, Calibrator.DcVoltage, rangeToSetOnDmm,testingMeasureValue, Logger,token);

                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    operation.Getting = BodyWork(Multimetr.DcVoltage, Calibrator.DcVoltage, Logger, token).Item1;
                };
                operation.ErrorCalculation = (expected, getting) => null;
                operation.LowerCalculation = (expected) =>
                {
                    var result =expected - AllowableError(Multimetr.DcVoltage.RangeStorage, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.UpperCalculation = (expected) =>
                {
                    var result = expected + AllowableError(Multimetr.DcVoltage.RangeStorage, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood = () => ChekedOperation(operation);

                DataRow.Add(operation);
            }
        }

        #endregion
    }

    [TestMeasPointAttribute("Operation1: ACV", typeof(MeasPoint<Voltage,Frequency>))]
    public sealed class AcvTest : MultiOperationBase<Voltage,Frequency>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AcvTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения переменного напряжения";
        }

        #region Methods

        protected override void InitWork(CancellationTokenSource token)
        {
            ConnectionToDevice();
            for (int row = 0; row < TestMeasPoints.GetUpperBound(0) + 1; row++)
            {
                var testingMeasureValue = TestMeasPoints[row, 1];
                var rangeToSetOnDmm = TestMeasPoints[row, 0];
                
                //установим пределы измерения мултиметра и воспроизведения калибратора
                Multimetr.AcVoltage.RangeStorage.SetRange(rangeToSetOnDmm);
                Calibrator.AcVoltage.RangeStorage.SetRange(testingMeasureValue);

                //если у какого-то из устройств нет подходящего диапазона?
                if (!RangeIsSet(Calibrator.AcVoltage.RangeStorage) ||
                    !RangeIsSet(Multimetr.AcVoltage.RangeStorage))
                {
                    ShowNotSupportedMeasurePointMeessage(Multimetr.AcVoltage, Calibrator.AcVoltage, rangeToSetOnDmm,
                                                         testingMeasureValue);

                    //эту точку не будем поверять, не добавляем ее в протокол и не подаем значение на приборы
                    continue;
                }

                //если  калибратор и вольтметр имеют подходящие диапазоны, то можно произвести измерение
                var operation = new BasicOperationVerefication<MeasPoint<Voltage, Frequency>>();
                operation.Expected = testingMeasureValue;

                //operation.InitWorkAsync = () =>
                //{
                //    InitWork(Multimetr.AcVoltage, Calibrator.AcVoltage, rangeToSetOnDmm, testingMeasureValue, Logger, token);
                //    return Task.CompletedTask;
                //};
                //operation.BodyWorkAsync = () =>
                //{
                //    var result = BodyWork(Multimetr.AcVoltage, Calibrator.AcVoltage, Logger, token).Item1;
                //    operation.Getting = ConvertMeasPoint(result, operation.Expected);
                //};
                //operation.ErrorCalculation = (expected, getting) => null;
                //operation.LowerCalculation = (expected) =>
                //{
                //    var result = expected - AllowableError(Multimetr.AcVoltage.RangeStorage, expected);
                //    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                //    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                //    return result;
                //};
                //operation.UpperCalculation = (expected) =>
                //{
                //    var result = expected + AllowableError(Multimetr.AcVoltage.RangeStorage, expected);
                //    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                //    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                //    return result;
                //};
                //operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                //operation.IsGood = () => ChekedOperation(operation);

                DataRow.Add(operation);
            }



        }

        #endregion

       
    }

    [TestMeasPointAttribute("Operation1: Resistance 2W", typeof(MeasPoint<Resistance>))]
    public sealed class Resist2WTest : OperationBase<MeasPoint<Resistance>>
    {
        public Resist2WTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения Электрического сопротивления";
        }
    }

    [TestMeasPointAttribute("Operation1: DCI", typeof(MeasPoint<Current>))]
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

    [TestMeasPointAttribute("Operation1: ACI", typeof(MeasPoint<Current, Frequency>))]
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

           // var arr = new MeasPoint<Current,Frequency>().GetArayMeasPointsInParcent(new MeasPoint<Current,Frequency>(200))
        }
    }
}
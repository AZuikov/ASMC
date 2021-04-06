using AP.Extension;
using AP.Math;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using NLog;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;

namespace Belvar_V7_40_1
{
    /// <summary>
    /// Предоставляет реализацию внешнего осномотра.
    /// </summary>
    public sealed class VisualTest : OperationBase<bool>
    {
        /// <inheritdoc />
        public VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
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
            DataRow.Add(new DialogOperationHelp(this, "V740_1_VisualTest"));
        }

        #endregion Methods
    }

    /// <summary>
    /// Предоставляет операцию опробывания.
    /// </summary>
    public sealed class Oprobovanie : OperationBase<bool>
    {
        /// <inheritdoc />
        public Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
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
            DataRow.Add(new DialogOperationHelp(this, "V740_1_Oprobovanie"));
        }

        #endregion Methods
    }

    public sealed class IsolationTest1 : OperationBase<bool>
    {
        /// <inheritdoc />
        public IsolationTest1(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Проверка электрической прочности изоляции вольтметра";
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
            DataRow.Add(new DialogOperationHelp(this, "V740_1_IsolationTest1"));
        }

        #endregion Methods
    }

    public sealed class IsolationTest2 : OperationBase<bool>
    {
        /// <inheritdoc />
        public IsolationTest2(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Проверка электрической прочности изоляции высоковольтного делителя постоянного напряжения";
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
            DataRow.Add(new DialogOperationHelp(this, "V740_1_IsolationTest2"));
        }

        #endregion Methods
    }



    [TestMeasPointAttribute("Operation1: DCV", typeof(MeasPoint<Voltage>))]
    public sealed class DcvTest : OperationBase<MeasPoint<Voltage>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DcvTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "DCV Определение погрешности измерения постоянного напряжения";
            Sheme = ShemeGeneration("V7-40_U_R.jpg", 1);
        }

        #region Methods

        protected override void InitWork(CancellationTokenSource token)
        {
            ConnectionToDevice();
            DataRow.Clear();
            for (int row = 0; row < TestMeasPoints.GetUpperBound(0) + 1; row++)
            {
                var testingMeasureValue = TestMeasPoints[row, 1];
                var rangeToSetOnDmm = TestMeasPoints[row, 0];

                var operation = new BasicOperationVerefication<MeasPoint<Voltage>>();
                operation.Name = rangeToSetOnDmm.Description;

                operation.Expected = testingMeasureValue;
                operation.ErrorCalculation = (expected, getting) => null;
                operation.LowerCalculation = (expected) =>
                {
                    var result = expected - AllowableError(Multimetr.DcVoltage.RangeStorage, rangeToSetOnDmm, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.UpperCalculation = (expected) =>
                {
                    var result = expected + AllowableError(Multimetr.DcVoltage.RangeStorage, rangeToSetOnDmm, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood = () => ChekedOperation(operation);

                //Проверяем можем ли мы установить подходящий предел измерения на мультиметре и воспроизвести значение физ. величины на эталоне.
                if (!CheckAndSetPhisicalValuesIsSuccess(Multimetr.DcVoltage.RangeStorage,
                                                        Calibrator.DcVoltage.RangeStorage, rangeToSetOnDmm,
                                                        testingMeasureValue, operation ))
                {
                    DataRow.Add(operation);
                    continue;//если что-то не можем, тогда информируем пользователя и эту точку не добавляем в протокол
                }
                
                operation.InitWorkAsync = () =>
                {
                    InitWork(Multimetr.DcVoltage, Calibrator.DcVoltage, rangeToSetOnDmm, testingMeasureValue, Logger, token);

                    return Task.CompletedTask;
                };

                operation.BodyWorkAsync = () =>
                {
                    try
                    {
                        operation.Getting = BodyWork(Multimetr.DcVoltage, Calibrator.DcVoltage, Logger, token, 2000).Item1;
                        operation.Getting.MainPhysicalQuantity.ChangeMultiplier(operation.Expected.MainPhysicalQuantity
                                                                                         .Multiplier);
                    }
                    catch (NullReferenceException e)
                    {
                        Logger.Error($"Не удалось получить измеренное значение с В7-40/1 в точке {testingMeasureValue}");
                    }
                };

               

                DataRow.Add(operation);
            }
        }

        #endregion Methods
    }

    [TestMeasPointAttribute("Operation1: ACV", typeof(MeasPoint<Voltage, Frequency>))]
    public sealed class AcvTest : MultiOperationBase<Voltage, Frequency>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AcvTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "ACV Определение погрешности измерения переменного напряжения";
            Sheme = ShemeGeneration("V7-40_U_R.jpg", 1);
        }

        #region Methods

        protected override void InitWork(CancellationTokenSource token)
        {
            ConnectionToDevice();
            DataRow.Clear();
            for (int row = 0; row < TestMeasPoints.GetUpperBound(0) + 1; row++)
            {
                var testingMeasureValue = TestMeasPoints[row, 1];
                var rangeToSetOnDmm = TestMeasPoints[row, 0];

                var operation = new BasicOperationVerefication<MeasPoint<Voltage, Frequency>>();
                operation.Name = rangeToSetOnDmm.MainPhysicalQuantity.ToString();
                operation.Expected = testingMeasureValue;
                operation.ErrorCalculation = (expected, getting) => null;
                operation.LowerCalculation = (expected) =>
                {
                    var result = expected - AllowableError(Multimetr.AcVoltage.RangeStorage,  rangeToSetOnDmm,expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.UpperCalculation = (expected) =>
                {
                    var result = expected + AllowableError(Multimetr.AcVoltage.RangeStorage, rangeToSetOnDmm, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood = () => ChekedOperation(operation);

                //Проверяем можем ли мы установить подходящий предел измерения на мультиметре и воспроизвести значение физ. величины на эталоне.
                //если  калибратор и вольтметр имеют подходящие диапазоны, то можно произвести измерение
                if (!CheckAndSetPhisicalValuesIsSuccess(Multimetr.AcVoltage.RangeStorage, Calibrator.AcVoltage.RangeStorage, rangeToSetOnDmm, testingMeasureValue, operation))
                {
                    //записываем сгенерированное измерение в результирующую таблицу
                    DataRow.Add(operation);
                    continue;//если что-то не можем, тогда информируем пользователя и эту точку не добавляем в протокол
                }

                operation.InitWorkAsync = () =>
                {
                    InitWork(Multimetr.AcVoltage, Calibrator.AcVoltage, rangeToSetOnDmm, testingMeasureValue, Logger, token);
                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    try
                    {
                        var result = BodyWork(Multimetr.AcVoltage, Calibrator.AcVoltage, Logger, token).Item1;
                        operation.Getting = ConvertMeasPoint(result, operation.Expected);
                        operation.Getting.MainPhysicalQuantity.ChangeMultiplier(operation.Expected.MainPhysicalQuantity
                                                                                         .Multiplier);
                    }
                    catch (NullReferenceException e)
                    {
                        Logger.Error($"Не удалось получить значение с В7-40/1 в точке {testingMeasureValue}");
                    }
                };
               

                DataRow.Add(operation);
            }
        }

        #endregion Methods
    }

    [TestMeasPointAttribute("Operation1: Resistance 2W", typeof(MeasPoint<Resistance>))]
    public sealed class Resist2WTest : OperationBase<MeasPoint<Resistance>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Resist2WTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Resist Определение погрешности измерения Электрического сопротивления";
            Sheme = ShemeGeneration("V7-40_U_R.jpg", 1);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            ConnectionToDevice();
            DataRow.Clear();
            for (int row = 0; row < TestMeasPoints.GetUpperBound(0) + 1; row++)
            {
                var testingMeasureValue = TestMeasPoints[row, 1];
                var rangeToSetOnDmm = TestMeasPoints[row, 0];

                var operation = new BasicOperationVerefication<MeasPoint<Resistance>>();
                operation.Name = rangeToSetOnDmm.Description;

                operation.Expected = testingMeasureValue;
                operation.ErrorCalculation = (expected, getting) => null;
                operation.LowerCalculation = (expected) =>
                {
                    var result = expected - AllowableError(Multimetr.Resistance2W.RangeStorage, rangeToSetOnDmm, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.UpperCalculation = (expected) =>
                {
                    var result = expected + AllowableError(Multimetr.Resistance2W.RangeStorage, rangeToSetOnDmm, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood = () => ChekedOperation(operation);

                //Проверяем можем ли мы установить подходящий предел измерения на мультиметре и воспроизвести значение физ. величины на эталоне.
                if (!CheckAndSetPhisicalValuesIsSuccess(Multimetr.Resistance2W.RangeStorage, Calibrator.Resistance2W.RangeStorage, rangeToSetOnDmm, testingMeasureValue, operation))
                    continue;//если что-то не можем, тогда информируем пользователя и эту точку не добавляем в протокол

               
                operation.InitWorkAsync = () =>
                {
                    InitWork(Multimetr.Resistance2W, Calibrator.Resistance2W, rangeToSetOnDmm ,testingMeasureValue, Logger, token);

                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    try
                    {
                        MeasPoint<Resistance> nullPointResistance = new MeasPoint<Resistance>(0); // сопротивление проводов
                        int timeOut = 700;//таймаут для измерения
                        if (Multimetr.Resistance2W.RangeStorage.SelectRange.End.MainPhysicalQuantity
                                     .GetNoramalizeValueToSi() == 199.999M) //если предел измерения 200 Ом, то нужно учитывать сопротивление проводов
                        {
                            //зададим 0 Ом и считвем сопротивление проводов
                            InitWork(Multimetr.Resistance2W, Calibrator.Resistance2W, rangeToSetOnDmm, nullPointResistance, Logger, token);
                            nullPointResistance = BodyWork(Multimetr.Resistance2W, Calibrator.Resistance2W, Logger, token).Item1;
                            nullPointResistance.MainPhysicalQuantity.Multiplier = UnitMultiplier.Kilo;
                        }
                        else
                        {
                            timeOut = 4000;//на других пределах нужно дольше измерять и не учитывать провода
                        }
                        InitWork(Multimetr.Resistance2W, Calibrator.Resistance2W, rangeToSetOnDmm, testingMeasureValue, Logger, token);

                        operation.Getting = BodyWork(Multimetr.Resistance2W, Calibrator.Resistance2W, Logger, token, timeOut).Item1;
                        operation.Getting.MainPhysicalQuantity.Multiplier = UnitMultiplier.Kilo;
                        //если сопротивление проводов измерено, то его нужно учесть
                        if (nullPointResistance.MainPhysicalQuantity.GetNoramalizeValueToSi() > 0)
                        {
                            operation.Getting = operation.Getting - nullPointResistance;
                        }

                        operation.Getting.MainPhysicalQuantity.ChangeMultiplier(operation.Expected.MainPhysicalQuantity
                                                                                         .Multiplier);
                    }
                    catch (NullReferenceException e)
                    {
                        Logger.Error($"Не удалось считать показания с {Multimetr.UserType} в точке {testingMeasureValue}");
                    }
                };
               

                DataRow.Add(operation);
            }
        }
    }

    [TestMeasPointAttribute("Operation1: DCI", typeof(MeasPoint<Current>))]
    public sealed class DciTest : OperationBase<MeasPoint<Current>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DciTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "DCI Определение погрешности измерения постоянного тока";
            Sheme = ShemeGeneration("V7-40_Curr.jpg", 2);
        }

        #region Methods

        protected override void InitWork(CancellationTokenSource token)
        {
            ConnectionToDevice();
            DataRow.Clear();
            for (int row = 0; row < TestMeasPoints.GetUpperBound(0) + 1; row++)
            {
                var testingMeasureValue = TestMeasPoints[row, 1];
                var rangeToSetOnDmm = TestMeasPoints[row, 0];

                var operation = new BasicOperationVerefication<MeasPoint<Current>>();
                operation.Name = rangeToSetOnDmm.Description;

                operation.Expected = testingMeasureValue;
                operation.ErrorCalculation = (expected, getting) => null;
                operation.LowerCalculation = (expected) =>
                {
                    var result = expected - AllowableError(Multimetr.DcCurrent.RangeStorage, rangeToSetOnDmm, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.UpperCalculation = (expected) =>
                {
                    var result = expected + AllowableError(Multimetr.DcCurrent.RangeStorage, rangeToSetOnDmm, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood = () => ChekedOperation(operation);

                //Проверяем можем ли мы установить подходящий предел измерения на мультиметре и воспроизвести значение физ. величины на эталоне.
                if (!CheckAndSetPhisicalValuesIsSuccess(Multimetr.DcCurrent.RangeStorage, Calibrator.DcCurrent.RangeStorage, rangeToSetOnDmm, testingMeasureValue, operation))
                    continue;//если что-то не можем, тогда информируем пользователя и эту точку не добавляем в протокол

                
                operation.InitWorkAsync = () =>
                {
                    InitWork(Multimetr.DcCurrent, Calibrator.DcCurrent, rangeToSetOnDmm, testingMeasureValue, Logger, token);

                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    try
                    {
                        operation.Getting = BodyWork(Multimetr.DcCurrent, Calibrator.DcCurrent, Logger, token).Item1;
                        operation.Getting.MainPhysicalQuantity.Multiplier = UnitMultiplier.Mili;
                        operation.Getting.MainPhysicalQuantity.ChangeMultiplier(operation.Expected.MainPhysicalQuantity.Multiplier);
                    }
                    catch (NullReferenceException e)
                    {
                        Logger.Error($"Не удалось считать показания с {Multimetr.UserType} в точке {testingMeasureValue}");
                    }
                };

                

                DataRow.Add(operation);
            }
        }

        #endregion Methods
    }

    [TestMeasPointAttribute("Operation1: ACI", typeof(MeasPoint<Current, Frequency>))]
    public sealed class AciTest : MultiOperationBase<Current, Frequency>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AciTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "ACI Определение погрешности измерения переменного тока";
            Sheme = ShemeGeneration("V7-40_Curr.jpg", 2);
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            ConnectionToDevice();
            for (int row = 0; row < TestMeasPoints.GetUpperBound(0) + 1; row++)
            {
                var testingMeasureValue = TestMeasPoints[row, 1];
                var rangeToSetOnDmm = TestMeasPoints[row, 0];

                var operation = new BasicOperationVerefication<MeasPoint<Current, Frequency>>();
                operation.Name = rangeToSetOnDmm.MainPhysicalQuantity.ToString();
                operation.Expected = testingMeasureValue;
                operation.ErrorCalculation = (expected, getting) => null;
                operation.LowerCalculation = (expected) =>
                {
                    var result = expected - AllowableError(Multimetr.AcCurrent.RangeStorage, rangeToSetOnDmm, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.UpperCalculation = (expected) =>
                {
                    var result = expected + AllowableError(Multimetr.AcCurrent.RangeStorage, rangeToSetOnDmm, expected);
                    result.MainPhysicalQuantity.ChangeMultiplier(expected.MainPhysicalQuantity.Multiplier);
                    result.Round(MathStatistics.GetMantissa(expected.MainPhysicalQuantity.Value));
                    return result;
                };
                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood = () => ChekedOperation(operation);

                //Проверяем можем ли мы установить подходящий предел измерения на мультиметре и воспроизвести значение физ. величины на эталоне.
                if (!CheckAndSetPhisicalValuesIsSuccess(Multimetr.AcCurrent.RangeStorage, Calibrator.AcCurrent.RangeStorage, rangeToSetOnDmm, testingMeasureValue, operation))
                {
                    //записываем сгенерированное измерение в таблицу
                    
                    DataRow.Add(operation);
                    continue;//если что-то не можем, тогда информируем пользователя и эту точку не добавляем в протокол
                    
                }

                //если  калибратор и вольтметр имеют подходящие диапазоны, то можно произвести измерение
               

                operation.InitWorkAsync = () =>
                {
                    InitWork(Multimetr.AcCurrent, Calibrator.AcCurrent, rangeToSetOnDmm, testingMeasureValue, Logger, token);
                    return Task.CompletedTask;
                };
                operation.BodyWorkAsync = () =>
                {
                    try
                    {
                        var result = BodyWork(Multimetr.AcCurrent, Calibrator.AcCurrent, Logger, token).Item1;
                        result.MainPhysicalQuantity.Multiplier = UnitMultiplier.Mili;
                        operation.Getting = ConvertMeasPoint(result, operation.Expected);
                        operation.Getting.MainPhysicalQuantity.ChangeMultiplier(operation.Expected.MainPhysicalQuantity
                                                                                         .Multiplier);
                    }
                    catch (NullReferenceException e)
                    {
                        Logger.Error($"Не удалось считать показания с {Multimetr.UserType} в точке {testingMeasureValue}");
                    }
                };
               

                DataRow.Add(operation);
            }
        }
    }


    //==========  Процедуры для ЗИПа ===================

    /// <summary>
    /// Определение погрешности измерения постоянного напряжения с высоковольтным делителем.
    /// </summary>

    public sealed class ZIP_DCI_10A : Zip_DcvTest_DNV<MeasPoint<Current>>
    {
        public ZIP_DCI_10A(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "ЗИП DCI постоянный ток с шунтом 10 А";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            //base.InitWork(token);
            MeasPoint<Current>[][] refPoints = new[]
            {//                            предел                                     точка                     погрешность
                new  [] {new MeasPoint<Current>(10),  new MeasPoint<Current>(2.000M),  new MeasPoint<Current>(0.010M)},
                new  [] {new MeasPoint<Current>(10),  new MeasPoint<Current>(5.000M),  new MeasPoint<Current>(0.021M)},
                new  [] {new MeasPoint<Current>(10),  new MeasPoint<Current>(10.000M), new MeasPoint<Current>(0.040M)},
                
            };

            DataRow.Clear();

            foreach (var VARIABLE in refPoints)
            {
                DataRow.Add(CalcValues(VARIABLE));
            }




        }

    }

    public sealed class DCV_DNV_ZIP : Zip_DcvTest_DNV<MeasPoint<Voltage>>
    {
        public DCV_DNV_ZIP(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "ЗИП DCV с ДНВ";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            //base.InitWork(token);
            MeasPoint<Voltage>[][] refPoints = new[]
            {//                                                        точка в киловольтах           точка в вольтах                    погрешность в вольтах
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(1, UnitMultiplier.Kilo),     new MeasPoint<Voltage>(1.0000M),  new MeasPoint<Voltage>(0.0044M)},
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(1.5M, UnitMultiplier.Kilo),  new MeasPoint<Voltage>(1.5000M),  new MeasPoint<Voltage>(0.0063M)},
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(1.9M, UnitMultiplier.Kilo),  new MeasPoint<Voltage>(1.9000M),  new MeasPoint<Voltage>(0.0076M)},
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(5M, UnitMultiplier.Kilo),    new MeasPoint<Voltage>(5.000M),   new MeasPoint<Voltage>(0.026M)},
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(10M, UnitMultiplier.Kilo),   new MeasPoint<Voltage>(10.000M),  new MeasPoint<Voltage>(0.044M)},
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(15M, UnitMultiplier.Kilo),   new MeasPoint<Voltage>(15.000M),  new MeasPoint<Voltage>(0.063M)},
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(19M, UnitMultiplier.Kilo),   new MeasPoint<Voltage>(19.000M),  new MeasPoint<Voltage>(0.076M)},
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(20M, UnitMultiplier.Kilo),   new MeasPoint<Voltage>(20.00M),   new MeasPoint<Voltage>(0.15M)},
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(30M, UnitMultiplier.Kilo),   new MeasPoint<Voltage>(30.00M),   new MeasPoint<Voltage>(0.19M)},
            };

            DataRow.Clear();

            foreach (var VARIABLE in refPoints)
            {
                DataRow.Add(CalcValues(VARIABLE));
            }

            


        }

    }

    public sealed class DCV_DNV_K2_ZIP : Zip_DcvTest_DNV<MeasPoint<Voltage>>
    {
        public DCV_DNV_K2_ZIP(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "ЗИП DCV Измерение постоянного напряжения с ДНВ и шунтом К2";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            //base.InitWork(token);
            MeasPoint<Voltage>[][] refPoints = new[]
            {//                                                        точка в киловольтах           точка в вольтах                    погрешность в вольтах
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(4, UnitMultiplier.Kilo),     new MeasPoint<Voltage>(2.000M),  new MeasPoint<Voltage>(0.016M)  },
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(30M, UnitMultiplier.Kilo),  new MeasPoint<Voltage>(15.000M),  new MeasPoint<Voltage>(0.063M)  },
               
            };

            DataRow.Clear();

            foreach (var VARIABLE in refPoints)
            {
                DataRow.Add(CalcValues(VARIABLE));
            }




        }

    }

    public  sealed class DCV_DNV_K3_ZIP : Zip_DcvTest_DNV<MeasPoint<Voltage>>
    {
        public DCV_DNV_K3_ZIP(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "ЗИП DCV Измерение постоянного напряжения с ДНВ и шунтом К3";
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            //base.InitWork(token);
            MeasPoint<Voltage>[][] refPoints = new[]
            {//                                                        точка в киловольтах           точка в вольтах                    погрешность в вольтах
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(0.8M, UnitMultiplier.Kilo), new MeasPoint<Voltage>(0.8000M),  new MeasPoint<Voltage>(0.0037M)  },
                new MeasPoint<Voltage> [] {new MeasPoint<Voltage>(30M, UnitMultiplier.Kilo),  new MeasPoint<Voltage>(6.000M),  new MeasPoint<Voltage>(0.030M)  },

            };

            DataRow.Clear();

            foreach (var VARIABLE in refPoints)
            {
                DataRow.Add(CalcValues(VARIABLE));
            }




        }

    }


   public abstract class Zip_DcvTest_DNV<TOperation> : OperationBase<TOperation>
    {
        protected Zip_DcvTest_DNV(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
           
        }
        
        /// <summary>
        /// Вычисляет и заполняет измеренные значения.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="refPoints"></param>
        protected BasicOperationVerefication<MeasPoint<T>> CalcValues<T>(MeasPoint<T>[] points) where T : class, IPhysicalQuantity<T>, new()
        {
            
                var operation = new BasicOperationVerefication<MeasPoint<T>>();
                operation.Name = points[0].Description;
                operation.Expected = points[1];
                operation.ErrorCalculation = (expected, getting) => null;
                operation.LowerCalculation = expected => expected - points[2];
                operation.UpperCalculation = expected => expected + points[2];
                operation.Getting = new MeasPoint<T>();
                operation.Getting.MainPhysicalQuantity.Value =
                    MathStatistics.RandomToRange(operation.LowerTolerance.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                                                 operation.UpperTolerance.MainPhysicalQuantity.GetNoramalizeValueToSi());
                operation.Getting.MainPhysicalQuantity.Multiplier = operation.Expected.MainPhysicalQuantity.Multiplier;
                int mantissa = MathStatistics.GetMantissa(operation.Expected.MainPhysicalQuantity.Value);
                operation.Getting.MainPhysicalQuantity.Value =
                    Math.Round(operation.Getting.MainPhysicalQuantity.Value, mantissa);
                operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
                operation.IsGood = () => ChekedOperation(operation);
            return operation;
        }
    }

   public class Zip_AcvTest_DN: Zip_ACTest<Voltage, Frequency>
   {
       public Zip_AcvTest_DN(IUserItemOperation userItemOperation) : base(userItemOperation)
       {
           Name = "ЗИП ACV Определение погрешности измерения переменного напряжения с делителем переменного напряжения";
       }

       protected override void InitWork(CancellationTokenSource token)
       {
           MeasPoint<Voltage, Frequency>[][] refPoint = new[]
           {//                                Предел измерения вольтметра                   Поверяемая точка                                           Погрешность         
               new []{new MeasPoint<Voltage, Frequency>(2,20),     new MeasPoint<Voltage, Frequency>(0.5000M,20),     new MeasPoint<Voltage, Frequency>(0.0065M, 20)}, 
               new []{new MeasPoint<Voltage, Frequency>(2,40),     new MeasPoint<Voltage, Frequency>(0.5000M,40),     new MeasPoint<Voltage, Frequency>(0.0045M, 40)}, 
               new []{new MeasPoint<Voltage, Frequency>(2,500),    new MeasPoint<Voltage, Frequency>(0.5000M,500),    new MeasPoint<Voltage, Frequency>(0.0045M, 500)}, 
               new []{new MeasPoint<Voltage, Frequency>(2,1000),   new MeasPoint<Voltage, Frequency>(0.5000M,1000),   new MeasPoint<Voltage, Frequency>(0.0045M, 1000)},

               new []{new MeasPoint<Voltage, Frequency>(2,20),     new MeasPoint<Voltage, Frequency>(0.7500M,20),    new MeasPoint<Voltage, Frequency>(0.0087M, 20)},
               new []{new MeasPoint<Voltage, Frequency>(2,40),     new MeasPoint<Voltage, Frequency>(0.7500M,40),    new MeasPoint<Voltage, Frequency>(0.0057M, 40)},
               new []{new MeasPoint<Voltage, Frequency>(2,500),    new MeasPoint<Voltage, Frequency>(0.7500M,500),   new MeasPoint<Voltage, Frequency>(0.0057M, 500)},
               new []{new MeasPoint<Voltage, Frequency>(2,1000),   new MeasPoint<Voltage, Frequency>(0.7500M,1000),  new MeasPoint<Voltage, Frequency>(0.0057M, 1000)},

               new []{new MeasPoint<Voltage, Frequency>(2,20),     new MeasPoint<Voltage, Frequency>(1.0000M,20),        new MeasPoint<Voltage, Frequency>(0.0110M, 20)},
               new []{new MeasPoint<Voltage, Frequency>(2,40),     new MeasPoint<Voltage, Frequency>(1.0000M,40),        new MeasPoint<Voltage, Frequency>(0.0070M, 40)},
               new []{new MeasPoint<Voltage, Frequency>(2,500),    new MeasPoint<Voltage, Frequency>(1.0000M,500),       new MeasPoint<Voltage, Frequency>(0.0070M, 500)},
               new []{new MeasPoint<Voltage, Frequency>(2,1000),   new MeasPoint<Voltage, Frequency>(1.0000M,1000),      new MeasPoint<Voltage, Frequency>(0.0070M, 1000)},
           };
            DataRow.Clear();

           foreach (var measPoints in refPoint)
           {
              DataRow.Add(CalcValues(measPoints));   
           }
       }
   }

    public class Zip_AcvTest_HighFreqProbe : Zip_ACTest<Voltage, Frequency>
    {
        public Zip_AcvTest_HighFreqProbe(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "ЗИП ACV Определение погрешности измерения переменного напряжения с высокочастотным пробником";
        }

       

        protected override void InitWork(CancellationTokenSource token)
        {
            MeasPoint<Voltage, Frequency>[][] refPoint = new[]
            {//                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,50, UnitMultiplier.Kilo),    new MeasPoint<Voltage, Frequency>(0.1000M, UnitMultiplier.None,50, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(0.0094M, UnitMultiplier.None,50, UnitMultiplier.Kilo)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,50, UnitMultiplier.Kilo),    new MeasPoint<Voltage, Frequency>(0.5000M, UnitMultiplier.None,50, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(0.1370M, UnitMultiplier.None,50, UnitMultiplier.Kilo)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,50, UnitMultiplier.Kilo),    new MeasPoint<Voltage, Frequency>(1.0000M, UnitMultiplier.None,50, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(0.1840M, UnitMultiplier.None,50, UnitMultiplier.Kilo)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,50, UnitMultiplier.Kilo),    new MeasPoint<Voltage, Frequency>(5.0000M,   UnitMultiplier.None,50, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(0.5600M, UnitMultiplier.None,50, UnitMultiplier.Kilo)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,50, UnitMultiplier.Kilo),    new MeasPoint<Voltage, Frequency>(10.0000M,  UnitMultiplier.None,50, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(1.0300M, UnitMultiplier.None,50, UnitMultiplier.Kilo)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,50, UnitMultiplier.Kilo),    new MeasPoint<Voltage, Frequency>(15.0000M,  UnitMultiplier.None,50, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(1.5000M, UnitMultiplier.None,50, UnitMultiplier.Kilo)},
                  //                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,100, UnitMultiplier.Kilo), new MeasPoint<Voltage, Frequency>(0.1000M,  UnitMultiplier.None,100, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(0.0094M, UnitMultiplier.None,100, UnitMultiplier.Kilo)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,100, UnitMultiplier.Kilo), new MeasPoint<Voltage, Frequency>(0.5000M,  UnitMultiplier.None,100, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(0.1370M, UnitMultiplier.None,100, UnitMultiplier.Kilo)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,100, UnitMultiplier.Kilo), new MeasPoint<Voltage, Frequency>(1.0000M,  UnitMultiplier.None,100, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(0.1840M, UnitMultiplier.None,100, UnitMultiplier.Kilo)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,100, UnitMultiplier.Kilo), new MeasPoint<Voltage, Frequency>(5.0000M,  UnitMultiplier.None,100, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(0.5600M, UnitMultiplier.None,100, UnitMultiplier.Kilo)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,100, UnitMultiplier.Kilo), new MeasPoint<Voltage, Frequency>(10.0000M, UnitMultiplier.None,100, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(1.0300M, UnitMultiplier.None,100, UnitMultiplier.Kilo)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,100, UnitMultiplier.Kilo), new MeasPoint<Voltage, Frequency>(15.0000M, UnitMultiplier.None,100, UnitMultiplier.Kilo),   new MeasPoint<Voltage, Frequency>(1.5000M, UnitMultiplier.None,100, UnitMultiplier.Kilo)},
                  //                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,30, UnitMultiplier.Mega), new MeasPoint<Voltage, Frequency>(0.1000M, UnitMultiplier.None,30, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.0094M, UnitMultiplier.None,30, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,30, UnitMultiplier.Mega), new MeasPoint<Voltage, Frequency>(0.3000M, UnitMultiplier.None,30, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1182M, UnitMultiplier.None,30, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,30, UnitMultiplier.Mega), new MeasPoint<Voltage, Frequency>(1.0000M, UnitMultiplier.None,30, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1840M, UnitMultiplier.None,30, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None, 30, UnitMultiplier.Mega), new MeasPoint<Voltage, Frequency>(3.0000M,   UnitMultiplier.None,30, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.3720M, UnitMultiplier.None, 30, UnitMultiplier.Mega) },
                  //                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,50, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.1000M, UnitMultiplier.None,50, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.0094M, UnitMultiplier.None,50, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,50, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.3000M, UnitMultiplier.None,50, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1182M, UnitMultiplier.None,50, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,50, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(1.0000M, UnitMultiplier.None,50, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1840M, UnitMultiplier.None,50, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,50, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(3.0000M,   UnitMultiplier.None,50, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.3720M, UnitMultiplier.None,50, UnitMultiplier.Mega) },
                  //                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,100, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.1000M, UnitMultiplier.None,100, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.0970M, UnitMultiplier.None,100, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,100, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.3000M, UnitMultiplier.None,100, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1110M, UnitMultiplier.None,100, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,100, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(1.0000M, UnitMultiplier.None,100, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1600M, UnitMultiplier.None,100, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,100, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(3.0000M,   UnitMultiplier.None,100, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.300M,  UnitMultiplier.None,100, UnitMultiplier.Mega) },
                  //                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,150, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.1000M, UnitMultiplier.None,150, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.0970M, UnitMultiplier.None,150, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,150, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.3000M, UnitMultiplier.None,150, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1110M, UnitMultiplier.None,150, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,150, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(1.0000M, UnitMultiplier.None,150, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1600M, UnitMultiplier.None,150, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,150, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(3.0000M,   UnitMultiplier.None,150, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.300M,  UnitMultiplier.None,150, UnitMultiplier.Mega) },
                  //                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,300, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.1000M, UnitMultiplier.None,300, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.0970M, UnitMultiplier.None,300, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,300, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.3000M, UnitMultiplier.None,300, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1110M, UnitMultiplier.None,300, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,300, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(1.0000M, UnitMultiplier.None,300, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1600M, UnitMultiplier.None,300, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,300, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(3.0000M,   UnitMultiplier.None,300, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.300M,  UnitMultiplier.None,300, UnitMultiplier.Mega)},
                  //                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,600, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.1000M, UnitMultiplier.None,600, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.0780M, UnitMultiplier.None,600, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,600, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.3000M, UnitMultiplier.None,600, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1140M, UnitMultiplier.None,600, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,600, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(1.0000M, UnitMultiplier.None,600, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.2400M, UnitMultiplier.None,600, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,600, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(3.0000M,   UnitMultiplier.None,600, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.6000M,  UnitMultiplier.None,600, UnitMultiplier.Mega) },
                  //                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,700, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.1000M, UnitMultiplier.None,700, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.0780M, UnitMultiplier.None,700, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,700, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.3000M, UnitMultiplier.None,700, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1140M, UnitMultiplier.None,700, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,700, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(1.0000M, UnitMultiplier.None,700, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.2400M, UnitMultiplier.None,700, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,700, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(3.0000M,   UnitMultiplier.None,700, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.6000M, UnitMultiplier.None,700, UnitMultiplier.Mega)},
                  //                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{  new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,800, UnitMultiplier.Mega), new MeasPoint<Voltage, Frequency>(0.1000M, UnitMultiplier.None,800, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.0780M, UnitMultiplier.None,800, UnitMultiplier.Mega)},
                  new[]{  new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,800, UnitMultiplier.Mega), new MeasPoint<Voltage, Frequency>(0.3000M, UnitMultiplier.None,800, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1140M, UnitMultiplier.None,800, UnitMultiplier.Mega)},
                  new[]{  new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,800, UnitMultiplier.Mega), new MeasPoint<Voltage, Frequency>(1.0000M, UnitMultiplier.None,800, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.2400M, UnitMultiplier.None,800, UnitMultiplier.Mega)},
                  new[]{  new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,800, UnitMultiplier.Mega), new MeasPoint<Voltage, Frequency>(3.0000M,   UnitMultiplier.None,800, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.6000M, UnitMultiplier.None,800, UnitMultiplier.Mega)},
                  //                  Предел измерения вольтметра                                                                                                      Поверяемая точка                                                                                                                      Погрешность         
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,1000, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.1000M, UnitMultiplier.None,1000, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.0880M, UnitMultiplier.None,1000, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,1000, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(0.3000M, UnitMultiplier.None,1000, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.1140M, UnitMultiplier.None,1000, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,1000, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(1.0000M, UnitMultiplier.None,1000, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.3400M, UnitMultiplier.None,1000, UnitMultiplier.Mega)},
                  new[]{ new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None,1000, UnitMultiplier.Mega),  new MeasPoint<Voltage, Frequency>(3.0000M,   UnitMultiplier.None,1000, UnitMultiplier.Mega),   new MeasPoint<Voltage, Frequency>(0.9000M, UnitMultiplier.None,1000, UnitMultiplier.Mega)},

           };

            DataRow.Clear();
            foreach (var measPoints in refPoint)
            {
                DataRow.Add(CalcValues(measPoints));
            }
        }
    }

    public abstract class Zip_ACTest<T1, T2> : MultiOperationBase<T1, T2>
       where T1 : class, IPhysicalQuantity<T1>, new() where T2 : class, IPhysicalQuantity<T2>, new()
   {
       protected Zip_ACTest(IUserItemOperation userItemOperation) : base(userItemOperation)
       {
       }

       protected virtual BasicOperationVerefication<MeasPoint<T1, T2>> CalcValues(MeasPoint<T1, T2>[] points)
       {

           var operation = new BasicOperationVerefication<MeasPoint<T1, T2>>();
           operation.Name = points[0].MainPhysicalQuantity.ToString();
           operation.Expected = points[1];
           operation.ErrorCalculation = (expected, getting) => null;
           operation.LowerCalculation = expected => expected - points[2];
           operation.UpperCalculation = expected => expected + points[2];
           operation.Getting = new MeasPoint<T1, T2>();
           operation.Getting.MainPhysicalQuantity.Value =
               MathStatistics.RandomToRange(operation.LowerTolerance.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                                            operation.UpperTolerance.MainPhysicalQuantity.GetNoramalizeValueToSi());
           operation.Getting.MainPhysicalQuantity.Multiplier = operation.Expected.MainPhysicalQuantity.Multiplier;
           operation.Getting.AdditionalPhysicalQuantity = operation.Expected.AdditionalPhysicalQuantity;
           int mantissa = MathStatistics.GetMantissa(operation.Expected.MainPhysicalQuantity.Value);
           operation.Getting.MainPhysicalQuantity.Value =
               Math.Round(operation.Getting.MainPhysicalQuantity.Value, mantissa);
           operation.CompliteWorkAsync = () => CompliteWorkAsync(operation);
           operation.IsGood = () => ChekedOperation(operation);
           return operation;
       }

   }

}
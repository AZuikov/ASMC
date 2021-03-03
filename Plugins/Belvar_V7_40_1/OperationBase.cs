using System.Threading;
using System.Threading.Tasks;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Interface.SourceAndMeter;
using DevExpress.Mvvm;
using NLog;
using Ivi.Visa;


namespace Belvar_V7_40_1
{
    /// <summary>
    /// Придоставляет базувую реализацию для пунктов поверки
    /// </summary>
    /// <typeparam name = "TOperation"></typeparam>
    public abstract class OperationBase<TOperation> : ParagraphBase<TOperation>
    {
        protected ICalibratorMultimeterFlukeBase Calibrator { get; set; }

        protected ASMC.Devices.IEEE.Belvar_V7_40_1 Multimetr { get; set; }

        /// <inheritdoc />
        protected OperationBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

       
        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name;
        }

        /// <param name = "token"></param>
        /// <inheritdoc />
        protected void InitWork<T>(IMeterPhysicalQuantity<T> multimetr, ISourcePhysicalQuantity<T> sourse, MeasPoint<T> rangeMeasureValue, MeasPoint<T> controlValue,
             Logger loger, CancellationTokenSource _token)
            where T : class, IPhysicalQuantity<T>, new()
        {
            multimetr.RangeStorage.SetRange(rangeMeasureValue);
            multimetr.RangeStorage.IsAutoRange = false;
            CatchException<IOTimeoutException>(() => multimetr.Setting(), _token, loger);
            CatchException<IOTimeoutException>(() => sourse.SetValue(controlValue), _token, loger);
        }

        protected (MeasPoint<TPhysicalQuantity>, IOTimeoutException) BodyWork<TPhysicalQuantity>(
            IMeterPhysicalQuantity<TPhysicalQuantity> multimetr, ISourcePhysicalQuantity<TPhysicalQuantity> sourse,
            Logger logger, CancellationTokenSource _token)
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            CatchException<IOTimeoutException>(() => sourse.OutputOn(), _token, logger);
            //todo 5522 должен отрабатывать обратную связь на завершение всех переходных процессов
            (MeasPoint<TPhysicalQuantity>, IOTimeoutException) result;
            try
            {
                result = CatchException<IOTimeoutException, MeasPoint<TPhysicalQuantity>>(
                                                                                          () => multimetr.GetValue(), _token, logger);
            }
            finally
            {
                CatchException<IOTimeoutException>(() => sourse.OutputOff(), _token, logger);
            }

            return result;
        }

        /// <summary>
        ///     Позволяет получить погрешность для указаной точки.
        /// </summary>
        /// <typeparam name="T">
        ///     Физическая велечина для которой необходима получить погрешность <see cref="IPhysicalQuantity" />
        /// </typeparam>
        /// <param name="rangeStorage">Диапазон на котором определяется погрешность.</param>
        /// <param name="expected">Точка на диапазоне для которой определяется погрешность.</param>
        /// <returns></returns>
        protected MeasPoint<T> AllowableError<T>(IRangePhysicalQuantity<T> rangeStorage, MeasPoint<T> expected)
            where T : class, IPhysicalQuantity<T>, new()
        {
            rangeStorage.SetRange(expected);
            var toll = rangeStorage.SelectRange.AccuracyChatacteristic.GetAccuracy(
                                                                                   expected.MainPhysicalQuantity.GetNoramalizeValueToSi(),
                                                                                   rangeStorage.SelectRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi());
            return new MeasPoint<T>(toll);
        }

        protected Task<bool> CompliteWorkAsync<T>(IMeasuringOperation<T> operation)
        {
            if (operation.IsGood == null || operation.IsGood())
                return Task.FromResult(operation.IsGood == null || operation.IsGood());

            return ShowQuestionMessage(operation.ToString()) == MessageResult.No
                ? Task.FromResult(true)
                : Task.FromResult(operation.IsGood == null || operation.IsGood());

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

        protected override void ConnectionToDevice()
        {
            Calibrator = (ICalibratorMultimeterFlukeBase)GetSelectedDevice<ICalibratorMultimeterFlukeBase>();
            Calibrator.StringConnection = GetStringConnect(Calibrator);
            Calibrator.InitializeAsync();

            Multimetr = (ASMC.Devices.IEEE.Belvar_V7_40_1)GetSelectedDevice<ASMC.Devices.IEEE.Belvar_V7_40_1>();
            Multimetr.StringConnection = GetStringConnect(Multimetr);
            Multimetr.InitializeAsync();
        }

        #endregion
    }

    public abstract class MultiOperationBase<T1, T2> : OperationBase<MeasPoint<T1, T2>>
        where T1 : class, IPhysicalQuantity<T1>, new() where T2 : class, IPhysicalQuantity<T2>, new()
    {
        protected MultiOperationBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

       
    }
}
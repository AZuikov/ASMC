using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Common.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Keysight.Generator;
using System.Data;
using System.Threading;
using ASMC.Devices.IEEE.PENDULUM;
using ASMC.Devices.Interface;
using NLog;

namespace CNT_90
{
    /// <summary>
    ///     Предоставляет базовую реализацию для пунктов поверки.
    /// </summary>
    /// <typeparam name="TOperation"></typeparam>

    public abstract class OperationBase<TOperation> : ParagraphBase<TOperation>
    {
        protected ASMC.Devices.IEEE.PENDULUM.Pendulum_CNT_90 Counter { get; set; }
        protected ISignalGenerator Generator { get; set; }

        protected OperationBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name;
        }

        /// <param name="token"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            /*Сдесь должна быть инициализация*/
            base.InitWork(token);
        }

        protected override void ConnectionToDevice()
        {
            Generator = (ISignalGenerator)GetSelectedDevice<ISignalGenerator>();
            Generator.StringConnection = GetStringConnect(Generator);
            

            Counter = (Pendulum_CNT_90)GetSelectedDevice<Pendulum_CNT_90>();
            Counter.StringConnection = GetStringConnect(Counter);
            
        }

        #endregion Methods
    }

    public abstract class OperationDeviceBase<T1, T2> : OperationBase<MeasPoint<T1, T2>> 
        where T1 : class, IPhysicalQuantity<T1>, new() 
        where T2 : class, IPhysicalQuantity<T2>, new()
    {
        protected OperationDeviceBase(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
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
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, "VisualTest_CNT90"));
            GeneratorOfSignals_81160A generator = new GeneratorOfSignals_81160A();
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

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            DataRow.Add(new DialogOperationHelp(this, "Опробование"));
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
    }

    public sealed class FrequencyMeasureCNT90 : OperationBase<Frequency>
    {
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public FrequencyMeasureCNT90(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение диапазона измеряемых частот, чувствительности и относительной погрешности измерения частоты сигнала";
            //Sheme
            
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            //base.InitWork(token);
            ConnectionToDevice();

            Counter.InputA.SettingSlope.SetInputSlopePositive();

            Generator.OUT1.SineSignal.SetValue(new MeasPoint<Voltage, Frequency>(0.1M, 500));
            Generator.OUT1.SineSignal.Setting();
            Generator.OUT1.SineSignal.OutputOn();

            var value = Counter.InputA.MeasureStandart.MeasFrequency.GetValue();

        }
        
    }
}
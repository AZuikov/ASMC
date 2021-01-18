using System.Linq;
using AP.Reports.Utils;
using AP.Utils.Data;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes;
using NLog;
using Current = ASMC.Data.Model.PhysicalQuantity.Current;

namespace E364xAPlugin
{
    public abstract class BasePowerSupplyWithDigitMult<T> : BasePowerSupplyProcedure<T>
    {
        #region Property

        protected IDigitalMultimetr344xx digitalMult { get; set; }

        #endregion

        protected BasePowerSupplyWithDigitMult(IUserItemOperation userItemOperation, E364xChanels inChanel,
            E364xRanges inVoltRange) : base(userItemOperation, inChanel, inVoltRange)
        {
        }

        #region Methods

        protected override void ConnectionToDevice()
        {
            base.ConnectionToDevice();
            digitalMult = UserItemOperation
                         .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IDigitalMultimetr344xx != null)
                         .SelectedDevice as IDigitalMultimetr344xx;
            ((IeeeBase) digitalMult).StringConnection = GetStringConnect(digitalMult);
        }

        #endregion
    }

    public abstract class BasePowerSupplyProcedure<T> : ParagraphBase<T>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        protected readonly E364xChanels _chanel;
        protected readonly E364xRanges _voltRangeMode;
        protected MeasPoint<Voltage, Current> _voltRange;

        #endregion

        #region Property

        protected IElectronicLoad ElectonicLoad { get; set; }
        protected E364xADevice powerSupply { get; set; }

        #endregion

        protected BasePowerSupplyProcedure(IUserItemOperation userItemOperation, E364xChanels inChanel,
            E364xRanges inVoltRange) :
            base(userItemOperation)
        {
            _chanel = inChanel;

            _voltRangeMode = inVoltRange;
        }

        #region Methods

        /// <summary>
        /// Подключение выбранных приборов для дальнейшей работы с устройствами.
        /// </summary>
        protected virtual void ConnectionToDevice()
        {
            ElectonicLoad = UserItemOperation
                           .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IElectronicLoad != null)
                           .SelectedDevice as IElectronicLoad;

            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xADevice != null)
                                           .SelectedDevice as E364xADevice;

            powerSupply.StringConnection = GetStringConnect(powerSupply);
            ((IeeeBase) ElectonicLoad).StringConnection = GetStringConnect((IProtocolStringLine) ElectonicLoad);

            _voltRange = powerSupply.Ranges[(int) _voltRangeMode];
        }

        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name + _chanel;
        }

        #endregion
    }
}
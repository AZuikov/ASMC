using System.Data;
using System.Linq;
using System.Reflection;
using AP.Extension;
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
        where T : PhysicalQuantity<T>, new()
    {
        #region Property

        protected IDigitalMultimetr344xx digitalMult { get; set; }

        #endregion

        protected BasePowerSupplyWithDigitMult(IUserItemOperation userItemOperation, E364xChanels inChanel) :
            base(userItemOperation, inChanel)
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

    public abstract class BasePowerSupplyProcedure<T> : ParagraphBase<MeasPoint<T>> where T : PhysicalQuantity<T>, new()
    {
        protected const string ConstBad = "Брак";

        protected const string ConstGood = "Годен";
        protected const string ConstNotUsed = "Не выполнено";
        
        protected bool isSpeedOperation = false;

        #region Fields

        protected readonly E364xChanels _chanel;

        #endregion

        #region Property

        protected IElectronicLoad ElectonicLoad { get; set; }
        protected E36xxA_Device powerSupply { get; set; }

        #endregion

        protected BasePowerSupplyProcedure(IUserItemOperation userItemOperation, E364xChanels inChanel) :
            base(userItemOperation)
        {
            _chanel = inChanel;
            Sheme = new SchemeImage
            {
                AssemblyLocalName = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Измерительная схема",
                Number = (int)_chanel,
                FileName = $"E364xA_{_chanel}_N3300A_34401A.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
                
            };
            
        }

        #region Methods

        /// <summary>
        /// Метод заполняет строку таблицы в соответствии с её структуров.
        /// </summary>
        /// <param name = "dataRow"></param>
        /// <param name = "dds"></param>
        public virtual void DefaultFillingRowTable(DataRow dataRow, BasicOperationVerefication<MeasPoint<T>> dds)
        {
            dataRow[0] = dds?.Comment;
            dataRow[1] = dds.Getting?.Description;
            dataRow[2] = dds?.LowerTolerance?.Description;
            dataRow[3] = dds?.UpperTolerance?.Description;
        }

        /// <summary>
        /// Подключение выбранных приборов для дальнейшей работы с устройствами.
        /// </summary>
        protected virtual void ConnectionToDevice()
        {
            ElectonicLoad = UserItemOperation
                           .ControlDevices.FirstOrDefault(q => q.SelectedDevice as IElectronicLoad != null)
                           .SelectedDevice as IElectronicLoad;

            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E36xxA_Device != null)
                                           .SelectedDevice as E36xxA_Device;

            powerSupply.StringConnection = GetStringConnect(powerSupply);
            ((IeeeBase) ElectonicLoad).StringConnection = GetStringConnect((IProtocolStringLine) ElectonicLoad);
        }

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint<T>>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                DefaultFillingRowTable(dataRow, dds);
                if (dds.IsGood == null)
                    dataRow[dataTable.Columns.Count - 1] = ConstNotUsed;
                else
                    dataRow[dataTable.Columns.Count - 1] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <summary>
        /// Метод, который определяем формулу расчета погрешности.
        /// </summary>
        /// <param name="inVal"></param>
        /// <returns></returns>
        protected abstract MeasPoint<T> ErrorCalc(MeasPoint<T> inVal);

        /// <summary>
        /// Типовая реализация расчета границ допуска, логики принятия решения о годности или браке для данной МП.
        /// </summary>
        /// <param name="inOperation"></param>
        protected void SetErrorCalculationUpperLowerCalcAndIsGood(BasicOperationVerefication<MeasPoint<T>> inOperation)
        {
            inOperation.ErrorCalculation = (point, measPoint) =>
            {
                MeasPoint<T> result = point - measPoint;
                result.Round(4);
                return result;
            };
            inOperation.UpperCalculation = (expected) => { return ErrorCalc(expected); };
            inOperation.LowerCalculation = (expected) => ErrorCalc(expected) * -1;

            inOperation.IsGood = () =>
            {
                if (inOperation.Getting == null || inOperation.Expected == null ||
                    inOperation.UpperTolerance == null || inOperation.LowerTolerance == null) return false;

                return inOperation.Error < inOperation.UpperTolerance &&
                       inOperation.Error > inOperation.LowerTolerance;
            };
        }


        protected void SetDefaultErrorCalculationUpperLowerCalcAndIsGood(BasicOperationVerefication<MeasPoint<T>> inOperation)
        {
            inOperation.ErrorCalculation = (point, measPoint) =>
            {
                MeasPoint<T> result = point - measPoint;
                result.Round(4);
                return result;
            };
            inOperation.LowerCalculation = (expected) =>
            {
                MeasPoint<T> result = expected - ErrorCalc(expected);
                result.Round(4);
                return result;
            };
            inOperation.UpperCalculation = (expected) =>
            {
                MeasPoint<T> result = expected + ErrorCalc(expected);
                result.Round(4);
                return result;
            };

            inOperation.IsGood = () =>
            {
                if (inOperation.Getting == null || inOperation.Expected == null ||
                    inOperation.UpperTolerance == null || inOperation.LowerTolerance == null) return false;
                return (inOperation.Getting < inOperation.UpperTolerance) &
                       (inOperation.Getting > inOperation.LowerTolerance);
            };
        }

        protected void SetDevicesForVoltageMode(E36xxA_Ranges rangePowerSupply)
        {
            powerSupply.ActiveE364XChanels = _chanel;
            var _voltRange = powerSupply.Ranges[(int)rangePowerSupply];
            powerSupply.SetRange(rangePowerSupply);
            powerSupply.SetVoltageLevel(new MeasPoint<Voltage>(_voltRange.MainPhysicalQuantity));
            powerSupply.SetCurrentLevel(new MeasPoint<Current>(_voltRange.AdditionalPhysicalQuantity));

            var resistToLoad =
                new MeasPoint<Resistance>(_voltRange.MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                          _voltRange.AdditionalPhysicalQuantity
                                                    .GetNoramalizeValueToSi());
            resistToLoad.Round(4);

            ElectonicLoad.SetThisModuleAsWorking();
            ElectonicLoad.SetResistanceMode();
            ElectonicLoad.SetResistanceLevel(resistToLoad);
        }

        protected void SetDevicesForCurrentMode(BasicOperationVerefication<MeasPoint<T>> inOperation, E36xxA_Ranges rangePowerSupply)
        {
            powerSupply.ActiveE364XChanels = _chanel;
            powerSupply.SetRange(rangePowerSupply);
            var _voltRange = powerSupply.Ranges[(int)rangePowerSupply];
            inOperation.Comment = _voltRange.Description;

            powerSupply.SetVoltageLevel(new MeasPoint<Voltage>(_voltRange.MainPhysicalQuantity));
            powerSupply.SetCurrentLevel(new MeasPoint<Current>(_voltRange
                                                                  .AdditionalPhysicalQuantity));
            // расчитаем значение для электронной нагрузки
            var resistToLoad =
                new MeasPoint<Resistance>(0.85M * _voltRange.MainPhysicalQuantity.GetNoramalizeValueToSi() /
                                          _voltRange
                                             .AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
            resistToLoad.Round(4);

            ElectonicLoad.SetThisModuleAsWorking();
            ElectonicLoad.SetResistanceMode();
            ElectonicLoad.SetResistanceLevel(resistToLoad);
        }


        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name + _chanel;
        }

        #endregion
    }
}
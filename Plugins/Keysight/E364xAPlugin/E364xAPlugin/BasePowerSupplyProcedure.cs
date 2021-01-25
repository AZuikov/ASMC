using System.Data;
using System.Linq;
using System.Reflection;
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
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected bool isSpeedOperation = false;

        #region Fields

        protected readonly E364xChanels _chanel;

        #endregion

        #region Property

        protected IElectronicLoad ElectonicLoad { get; set; }
        protected E364xADevice powerSupply { get; set; }

        #endregion

        protected BasePowerSupplyProcedure(IUserItemOperation userItemOperation, E364xChanels inChanel) :
            base(userItemOperation)
        {
            _chanel = inChanel;
            Sheme = new ShemeImage
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

            powerSupply = UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice as E364xADevice != null)
                                           .SelectedDevice as E364xADevice;

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

        protected override string GetReportTableName()
        {
            return MarkReportEnum.FillTableByMark.GetStringValue() + GetType().Name + _chanel;
        }

        #endregion
    }
}
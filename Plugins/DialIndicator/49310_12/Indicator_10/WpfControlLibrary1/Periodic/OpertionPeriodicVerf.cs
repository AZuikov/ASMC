using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Common.UI;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.MVision;

namespace Indicator_10.Periodic
{
    public class OpertionPeriodicVerf : Operation
    {
        public OpertionPeriodicVerf(ServicePack servicePac):base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new IUserType[] {new Ppi()}},
                new Device {Devices = new IUserType[] {new WebCam()}}
            };
            //var a = new Operation1(this);
            //a.Nodes.Add(new Operation2(this));

            //a.Nodes.Add(new Operation1(this));
            //this.UserItemOperation = new IUserItemOperationBase[] { new Operation1(this), new Operation1(this), a };
            Accessories = new[]
            {
                "Весы настольные циферблатные РН-3Ц13У",
                "Приспособление для определения измерительного усилия и его колебаний мод. 253",
                "Граммометр часового типа Г 3,0",
                "Прибор для поверки индикаторов ППИ-50"
            };
        }

        /// <inheritdoc />
        public override void RefreshDevice()
        {
            AddresDevice = ASMC.Devices.USB_Device.SiliconLabs.UsbExpressWrapper.FindAllDevice.Select(q => q.Number.ToString()).Concat(WebCam.GetVideoInputDevice.Select(q => q.MonikerString)).ToArray();
        }

        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }

    public class ConnectionDiametr : IndicatorParagraphBase<MeasPoint>
    {
        /// <inheritdoc />
        public ConnectionDiametr(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение присоединительного диаметра";
            ColumnName = new[]
            {
                "Присоединительный диаметр", "Минимальный диаметр гильзы", "Максимальный диаметр гильзы"
            };
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {

            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds.Expected?.Description;
                dataRow[1] = dds.Getting?.Description;
                dataRow[2] = dds.LowerTolerance?.Description;
                dataRow[3] = dds.UpperTolerance?.Description;
                if (dds.IsGood == null)
                    dataRow[5] = ConstNotUsed;
                else
                    dataRow[5] = dds.IsGood() ? ConstGood : ConstBad;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <inheritdoc />
        protected override void InitWork()
        {
            base.InitWork();
            var operation = new BasicOperation<MeasPoint>();
            operation.InitWork= () =>
            {
                var a = this.UserItemOperation.ServicePack.FreeWindow as WindowService;
                var vm = new 
                a.Show();
            }
            DataRow.Add(operation);
        }
    }
}
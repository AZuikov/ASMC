using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;

namespace Plugins.Test
{
 
    public class Device  : Program
    {
        /// <inheritdoc />
        public Device(ServicePack service) : base(service)
        {
            this.Type = "Тест";
            Operation = new Verefication(service);
        }
    }

    public class Verefication : OperationMetrControlBase
    {

        public Verefication(ServicePack servicePac)
        {
            this.UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePac);
        }
    }

    public class OpertionFirsVerf : Operation
    {
        /// <inheritdoc />
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            ControlDevices = new IDeviceUi[]
            {
                new DeviceInterface {Name = new[] {"34401A"}, Description = "Мультиметр"},
                new DeviceInterface {Name = new[] {"В3"}, Description = "Микровольтметр", IsCanStringConnect = false}
            };
            this.UserItemOperation = new IUserItemOperationBase[] {new Operation1(this)};
                Accessories = new[]
            {
                "Мультиметр цифровой Agilent/Keysight 34401A",
                "Кабель banana"
            };
        }

        /// <inheritdoc />
        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        /// <inheritdoc />
        public override void FindDivice()
        {
            throw new NotImplementedException();
        }
    }
    public class DeviceInterface :  IDeviceUi
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public bool IsCanStringConnect { get; set; }

        /// <inheritdoc />
        public bool? IsConnect { get; }

        /// <inheritdoc />
        public string[] Name { get; set; }

        /// <inheritdoc />
        public string SelectedName { get; set; }

        /// <inheritdoc />
        public string StringConnect { get; set; }
    }
    public class Operation1 : ParagraphBase, IUserItemOperation<double>
    {
        /// <inheritdoc />
        public Operation1(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<double>>();
        }


        /// <inheritdoc />
        protected override DataTable FillData()
        {
            return null;
        }

        /// <inheritdoc />
        protected override void InitWork()
        {
         return;
        }

        /// <inheritdoc />
        public List<IBasicOperation<double>> DataRow { get; set; }
    }

}

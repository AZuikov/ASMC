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
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using DevExpress.Mvvm;
using MessageButton = DevExpress.Mvvm.MessageButton;
using MessageIcon = DevExpress.Mvvm.MessageIcon;

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
            this.UserItemOperationPeriodicVerf= UserItemOperationPrimaryVerf;
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
            var a = new Operation1(this);
            a.Nodes.Add(new Operation1(this));

            a.Nodes.Add(new Operation1(this));
            this.UserItemOperation = new IUserItemOperationBase[] {new Operation1(this), new Operation1(this) , a };
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

            DataRow.Clear();

            for (int i = 0; i < 1; i++)
            {
                var operation = new BasicOperation<double>();
                operation.InitWork = () =>
                {
                    var a = this.UserItemOperation.ServicePack.MessageBox;
                    a.Show(this.Name, "", MessageButton.OK, MessageIcon.None, MessageResult.Cancel);
                    return Task.CompletedTask;
                };
                operation.BodyWork = () =>
                {

                    Thread.Sleep(10000);
                    operation.Expected = new Random().NextDouble();
                    operation.Getting = new Random().NextDouble();
                    operation.IsGood = () => false;
                };


                DataRow.Add(operation);
            }

        }

        /// <inheritdoc />
        public List<IBasicOperation<double>> DataRow { get; set; }
    }


}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Common.ViewModel;
using ASMC.Core.Model;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.MVision;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using MessageButton = DevExpress.Mvvm.MessageButton;
using MessageIcon = DevExpress.Mvvm.MessageIcon;
using WindowService = ASMC.Common.UI.WindowService;

namespace Plugins.Test
{
 
    public class Device  : Program<Verefication>
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
                new DeviceInterface {Devices = new IDeviceBase[] {new Ppi()}, Description = "Прибор ППИ"}
            };
            var a = new Operation1(this);
            a.Nodes.Add(new Operation2(this));

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
            AddresDevice= ASMC.Devices.USB_Device.SiliconLabs.UsbExpressWrapper.FindAllDevice.Select(q => q.Number.ToString()).Concat(WebCam.GetVideoInputDevice.Select(q=>q.MonikerString)).ToArray();
        }

        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }

 
    }
    public class DeviceInterface :  IDeviceUi
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public bool IsCanStringConnect { get; set; } = true;

        /// <inheritdoc />
        public bool? IsConnect { get; }

        /// <inheritdoc />
        public IUserType[] Devices { get; set; }

        /// <inheritdoc />
        public IUserType SelectedDevice { get; set; }

        /// <inheritdoc />
        public string StringConnect { get; set; }
    }
    public class Operation1 : ParagraphBase, IUserItemOperation<double>
    {
        /// <inheritdoc />
        public Operation1(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр1";
            DataRow = new List<IBasicOperation<double>>();
            Sheme = new ShemeImage
            {
                Number = 1,
                FileName = @"appa_10XN_A_Aux_5522A.jpg"
            };
        }


        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dt = new DataTable("fdsfs");
            dt.Columns.Add("Expected");
            dt.Columns.Add("Getting");
            foreach (var r in DataRow)
            {
                var row = dt.NewRow();
                row[0] = r.Expected;
                row[1] = r.Getting;
                dt.Rows.Add(row);
            }
            
            return dt;
        }

        /// <inheritdoc />
        protected override void InitWork()
        {

            DataRow.Clear();

            for (int i = 0; i <1; i++)
            {
                var operation = new BasicOperation<double>();
                operation.InitWork = () =>
                {
                    var wind = this.UserItemOperation.ServicePack.FreeWindow as WindowService;
                    var a = new WebCamViewModel();
                    wind.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                    wind.Show("WebView", a);
                    return Task.CompletedTask;
                };
                operation.BodyWork = () =>
                {
                    Thread.Sleep(50);
                    throw new Exception("СПЕЦАЛЬНО!");
                    operation.Expected = new Random().NextDouble();
                    operation.Getting = new Random().NextDouble();
                    operation.IsGood = () => true;
                };
                DataRow.Add(operation);
            }

        }

        /// <inheritdoc />
        public List<IBasicOperation<double>> DataRow { get; set; }
    }

    public class Operation2 : ParagraphBase, IUserItemOperation<double>
    {
        /// <inheritdoc />
        public Operation2(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр2";
            DataRow = new List<IBasicOperation<double>>();
            Sheme = new ShemeImage
            {
                Number = 2,
                FileName = @"appa_10XN_ma_5522A.jpg"
            };
        }


        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dt = new DataTable("fdsfs");
            dt.Columns.Add("Expected");
            dt.Columns.Add("Getting");
            foreach (var r in DataRow)
            {
                var row = dt.NewRow();
                row[0] = r.Expected;
                row[1] = r.Getting;
                dt.Rows.Add(row);
            }

            return dt;
        }

        /// <inheritdoc />
        protected override void InitWork()
        {
            base.InitWork();
            for (int i = 0; i < 1; i++)
            {
                var operation = new BasicOperation<double>();
                operation.InitWork = () =>
                {
                    var wind = this.UserItemOperation.ServicePack.FreeWindow as WindowService;
                    var a = new TableViewModel();
                    wind.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                    wind.Show("Table", a);
                    return Task.CompletedTask;
                };
                operation.BodyWork = () =>
                {

                    Thread.Sleep(50);
                    operation.Expected = new Random().NextDouble();
                    operation.Getting = new Random().NextDouble();
                    operation.IsGood = () => true;
                };


                DataRow.Add(operation);
            }

        }

        /// <inheritdoc />
        public List<IBasicOperation<double>> DataRow { get; set; }
    }
}

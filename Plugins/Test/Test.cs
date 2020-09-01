﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using MessageButton = DevExpress.Mvvm.MessageButton;
using MessageIcon = DevExpress.Mvvm.MessageIcon;
using WindowService = ASMC.Common.UI.WindowService;

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

            for (int i = 0; i <20; i++)
            {
                var operation = new BasicOperation<double>();
                operation.InitWork = () =>
                {
                    var wind = this.UserItemOperation.ServicePack.FreeWindow as WindowService;
                    var a = new TableVm();
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

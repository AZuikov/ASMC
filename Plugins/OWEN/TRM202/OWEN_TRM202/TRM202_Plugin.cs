using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AP.Math;
using AP.Utils.Data;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.IZ_Tech;
using ASMC.Devices.Port.ZipNu4Pribor;


namespace OWEN_TRM202
{
    public class TRM202_Plugin<T> : Program<T> where T : OperationMetrControlBase
    {
        public TRM202_Plugin(ServicePack service) : base(service)
        {
            Grsi = "32478-06";
            Type = "ТРМ202";

        }


        public  class Operation : OperationMetrControlBase
        {
            //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.
            public Operation()
            {
                //это операция первичной поверки
                //UserItemOperationPrimaryVerf = new OpertionFirsVerf();
                //здесь периодическая поверка, но набор операций такой же
                UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
            }



        }

        public  class OpertionFirsVerf : ASMC.Core.Model.Operation
        {
            protected OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
            {

                //Необходимые устройства
                ControlDevices = new IDeviceUi[]
                {
                    new Device {Devices = new IDeviceBase[] {new Km300P()}, Description = "компаратор-калибратор универсальный"},
                    new Device {Devices = new IDeviceBase[] {new MIT_8()}, Description = "измеритель температуры прецизионный"}
                };
                TestDevices = new IDeviceUi[]
                {
                    new Device {Devices = new IDeviceBase[] {new MIT_8()}, Description = "измеритель температуры прецизионный"}
                };
                DocumentName = "APPA_107N_109N";
            }

            public override void RefreshDevice()
            {
                AddresDevice = IeeeBase.AllStringConnect;
            }

            public override void FindDevice()
            {
                throw new NotImplementedException();
            }
        }

        public class Oper1VisualTest : ParagraphBase<bool>
        {
            public Oper1VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
            {
                Name = "Внешний осмотр";
                DataRow = new List<IBasicOperation<bool>>();
            }

            protected override string GetReportTableName()
            {
                throw new NotImplementedException();
            }

            protected override DataTable FillData()
            {
                var data = base.FillData();
                var dataRow = data.NewRow();
                if (DataRow.Count == 1)
                {
                    var dds = DataRow[0] as BasicOperation<bool>;
                    dataRow[0] = dds.Getting ? "Соответствует" : dds.Comment;
                    data.Rows.Add(dataRow);
                }

                return data;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.Generator;
using ASMC.Devices.IEEE.Keysight.NoiseFigureAnalyzer;

namespace N8957APlugin
{
    public class N8975A_Plugin<T> : Program<T> where T : OperationMetrControlBase
    {


        protected N8975A_Plugin(ServicePack service) : base(service)
        {
            Type = "N8975A";
            Grsi = "37178-08";
            Range = "no range";
            Accuracy = "none";
        }
    }

    public class Operation : OperationMetrControlBase
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.

        public Operation(ServicePack servicePack)
        {
            UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePack);
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public  class OpertionFirsVerf : ASMC.Core.Model.Operation
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            DocumentName = "N8975A_8_3_4";

            ControlDevices = new IDeviceUi[]
                {new Device { Devices = new IDeviceBase[] { new E8257D() }, Description = "Генератор сигналов"}};
            TestDevices = new IDeviceUi[]
                {new Device  {Devices = new IDeviceBase[]{new N8975A()}, Description  = "Анализатор шума"} };
            

            //Необходимые аксесуары
            Accessories = new[]
            {
                "Стандарт частоты PENDULUM 6689",
                "Генератор сигналов Keysight/Agilent E8257D",

                "Преобразователь интерфесов National Instruments GPIB-USB",
                "Кабелли:\n 1) для подключения стандарта частоты к генератору\n  2) для подключения выхода генератора ко входу анализатора шума"
            };

            UserItemOperation = new[] {new Oper8_3_4FreqInSintezatorFrequency(this)};
        }



        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }
    }

    public class Oper8_3_4FreqInSintezatorFrequency : ParagraphBase, IUserItemOperation<MeasPoint>
    {
        public List<IBasicOperation<MeasPoint>> DataRow { get; set; }
        public Oper8_3_4FreqInSintezatorFrequency(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности установки частоты внутреннего синтезатора";
            DataRow = new List<IBasicOperation<MeasPoint>>();
        }

        protected override DataTable FillData()
        {
           var dataTable = new DataTable{TableName = "FillTabBmSintezatorFrequency" };
           dataTable.Columns.Add("fН");
           dataTable.Columns.Add("fЦ");
           dataTable.Columns.Add("Δf");
           dataTable.Columns.Add("Максимально допустимое значение Δf");
           foreach (var row in DataRow)
           {
               var dataRow = dataTable.NewRow();
               var dds = row as BasicOperationVerefication<MeasPoint>;
               if (dds == null) continue;
               dataRow[0] = dds?.Expected.Description ;
               dataRow[1] = dds?.Getting.Description;
               MeasPoint tol = new MeasPoint(dds.Expected.Units, dds.Expected.UnitMultipliersUnit, dds.Expected.Value - dds.Getting.Value);
               dataRow[2] = tol.Description;
               dataRow[3] = dds?.Error.Description;
           }

           return dataTable;
        }

        protected override void InitWork()
        {
            throw new NotImplementedException();
        }

        
    }
}

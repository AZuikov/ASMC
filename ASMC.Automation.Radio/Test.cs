﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;

namespace ASMC.Automation.Radio
{
 
    public class Device  : IProg
    {
        public string Type { get; set; }
        public string Grsi { get; set; }

        public string Range{ get; set; }

        public string Accuracy
        {
            get; set;
        }

        public AbstraktOperation AbstraktOperation { get; }


        public Device()
        {
            AbstraktOperation = new Operation();
        }

    }
    public interface IProg
    {
        string Type { get; set; }
        string Grsi { get; set; }
        string Range    {    get;   }

         string Accuracy
        {
            get; 
        }
        AbstraktOperation AbstraktOperation { get; }

    }

    public class Operation : AbstraktOperation
    {
        public Operation()
        {
            this.UserItemOperationPrimaryVerf = new OpertionFirsVerf() ;
        } 
    }

    public class OpertionFirsVerf : IUserItemOperation
    {
        private string[] _StringConnectArray;
        public void RefreshDevice()
        {
            _StringConnectArray = new[] { "COM5", "COM6" }; 
        }

        public IDevice[] Device { get; }
        public IUserItemOperationBase[] UserItemOperation { get; }
        public string[] Accessories { get; }

        public OpertionFirsVerf()
        {
            Device = new[]
            {
                new DeviceInterface{ Name = new []{"344010A"}, StringConnect = "COM1", Description = "Мультиметр"},
                new DeviceInterface{ Name = new []{"APPA-106", "APPA-107"}, StringConnect = "COM12" } ,
                new DeviceInterface{ Name = new []{"APP32A-106", "APPA-107"}, StringConnect = "COM12" },
                new DeviceInterface{ Name = new []{"AP54545PA-106", "APPA-107"}, StringConnect = "COM12" }
            };
            Accessories = new[] {"Мультиметр 344010A", "Набор проводов"};
      
            var opertio3 = new ItemOperation2 {Name = "Измерение напряжения"};
            opertio3.Nodes.Add(new ItemOperation3 {Name = "Измерение постоянного тока"});   
            UserItemOperation = new IUserItemOperationBase[] { new ItemOperation1 { Name = "Опробывание" }, opertio3 };
        }
    }
    public class ItemOperation1  : AbstractUserItemOperationBase, IUserItemOperation<string>
    {

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public List<IBasicOperation<string>> DataRow { get; set; }

        public override void StartWork()
        {
            //throw new NotImplementedException();
        }

        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Измеренное значение");
            data.Columns.Add("Ожидаемое значение");
            foreach(var row in DataRow)
            {
                var dataRow = data.NewRow();
                var dds = row as BasicOperationVerefication<double>;
                dataRow[0] = dds.Getting;
                dataRow[1] = dds.Expected;
                data.Rows.Add(dataRow);
            }

            return data;
        }
    }
    public class ItemOperation2 : AbstractUserItemOperationBase, IUserItemOperation<double?>
    {
        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        } 
        
        public List<IBasicOperation<double?>> DataRow { get; set; }
    
       
        public override void StartWork()
        {
            throw new NotImplementedException();
        }

        public ItemOperation2()
        {
            DataRow = new List<IBasicOperation<double?>>
            {
                new BasicOperationVerefication<double?>{Getting = 512.3, Expected = 512.0},
                new BasicOperationVerefication<double?> {Getting = 5.001, Expected = 5.005},  
                new BasicOperationVerefication<double?> {Getting = null , Expected = 5.005},
            };
        }
        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Измеренное значение");
            data.Columns.Add("Ожидаемое значение");
            foreach(var row in DataRow)
            {
                var dataRow = data.NewRow();
                var dds = row as BasicOperationVerefication<double?>;
                dataRow[0] = dds.Getting;
                dataRow[1] = dds.Expected;
                data.Rows.Add(dataRow);
            }

            return data;
        }
       
    }

    public class ItemOperation3 : AbstractUserItemOperationBase, IUserItemOperation<double?>
    {
        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            throw new NotImplementedException();
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public List<IBasicOperation<double?>> DataRow { get; set; }
    }
}

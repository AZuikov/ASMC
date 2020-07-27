﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.APPA;
using DevExpress.Mvvm;


namespace APPA_107N_109N
{
    public class Appa107N109NBasePlugin : AbstractProgram

    {
        public Appa107N109NBasePlugin()
        {
            
            Grsi = "20085-11";
            
        }

        public string Type { get; protected set; }
        public string Grsi { get; }
        public string Range { get; protected set; }
        public string Accuracy { get; protected set; }
        public IMessageBoxService TaskMessageService { get; set; }
        public OperationBase Operation { get; }
    }

    public class Operation : OperationBase
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

    public class UsedDevices : IDevice
    {
        public bool IsCanStringConnect { get; set; }
        public string Description { get; set; }
        public string[] Name { get; set; }
        public string SelectedName { get; set; }
        public string StringConnect { get; set; }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public bool? IsConnect { get; }
    }

    public abstract class OpertionFirsVerf : IUserItemOperation
    {
        public string[] Accessories { get; }
        public string[] AddresDivece { get; set; }
        public IDevice[] ControlDevices { get; set; }
        public IDevice[] TestDevices { get; set; }
        public IDevice[] Device { get; }
        public IUserItemOperationBase[] UserItemOperation { get; set; }

        public OpertionFirsVerf()
        {
            //Необходимые устройства
            ControlDevices = new IDevice[] { new UsedDevices {Name = new []{"5522A"}, Description = "Многофунциональный калибратор"}};
            TestDevices = new IDevice[]{ new UsedDevices { Name = new[] { "Appa-107N" }, Description = "Цифровой портативный мультиметр" } };


            Accessories = new[]
            {
                "Интерфейсный кабель для клибратора (GPIB или COM порт)",
                "Кабель banana - banana 2 шт.",
                "Интерфейсный кабель для прибора APPA-107N/APPA-109N USB-COM инфракрасный."
            };

            
        }

        public void RefreshDevice()
        {
            AddresDivece = new IeeeBase().GetAllDevace.ToArray();

        }

        public void FindDivice()
        {
            throw new NotImplementedException();
        }
    }
 

    public abstract class Oper1VisualTest : AbstractUserItemOperationBase, IUserItemOperation<bool>
    {
        public List<IBasicOperation<bool>> DataRow { get; set; }

        public Oper1VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

       

        public override async Task StartWork(CancellationTokenSource token)
        {
           
        }

        
    }

    public abstract class Oper2Oprobovanie : AbstractUserItemOperationBase, IUserItemOperation<bool>
    {
        public List<IBasicOperation<bool>> DataRow { get; set; }

        public Oper2Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }

        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Результат опробования");
            var dataRow = data.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<bool>;
            dataRow[0] = dds.Getting;
            data.Rows.Add(dataRow);
            return data;
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationTokenSource token)
       
        {
            var bo = new BasicOperation<bool> { Expected = true };
            //bo.IsGood = s => bo.Getting;

            DataRow.Add(bo);
        }

        
    }

    public abstract class Oper3DcvMeasure : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        #region Fields
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //список точек из методики поверки
        readonly decimal[] _pointsArr = { (decimal)0.004, (decimal)0.008, (decimal)0.012, (decimal)0.016, (decimal)0.018, (decimal)-0.018 };
        //множитель, соответсвующий пределу измерения
        readonly decimal[] _multArr = { 1, 10, 100, 1000, 10000};
        //эталон
        protected Calib5522A flkCalib5522A;
        //контрлируемый прибор
        protected Mult107_109N appa107N;


        #endregion


        public Oper3DcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
           
        }


        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationTokenSource token)
       
        {
            
            flkCalib5522A.SetStringconection(this.UserItemOperation.ControlDevices.First().StringConnect);
            //тут нужно проверять, если прибор не подключен, тогда прекращаем работу
            if (flkCalib5522A.Open())
            {
                
                return;
            }
            flkCalib5522A.Close();


            
            
            appa107N.StringConnection = GetStringConnect(appa107N);
            appa107N.Open();

            

            decimal[] multiplaisArr = { (decimal)0.1, 1, 10, (decimal)0.001, (decimal)0.01 };
            var ranges = new[]
                {Mult107_109N.Range.Range1Manual, Mult107_109N.Range.Range2Manual, Mult107_109N.Range.Range3Manual};

            decimal DcvMeas(int point)
            {
                var countMeas = 10;
                decimal resultVal = 0;
                flkCalib5522A.Out.Set.Voltage.Dc.SetValue(0).Out.SetOutput(CalibrMain.COut.State.On);  

                foreach (var t in _pointsArr)
                {
                    flkCalib5522A.Out.Set.Voltage.Dc.SetValue(t * multiplaisArr[point]);


                    var valuesMeasure = new List<decimal>();
                    do
                    {
                        for (int j = 0; j < countMeas; )
                            valuesMeasure.Add((decimal)appa107N.Value);

                    } while (!AP.Math.MathStatistics.IntoTreeSigma(valuesMeasure.ToArray())); // пока показания не стабилизировались будут проводиться измерения
                    //Теперь уберем выбросы
                    var array = valuesMeasure.ToArray();
                    AP.Math.MathStatistics.Grubbs(ref array);
                    //вычисляем среднее значение и округляем
                    resultVal = AP.Math.MathStatistics.GetArithmeticalMean(array);
                    AP.Math.MathStatistics.Round(ref resultVal, 4);
                }

                return resultVal;
            }

            for (int i = 0; i < 3; i++)
            {
                do
                {
                    //вывод окна сообщения о включении предела измерения  
                } while (appa107N.GetRange != ranges[i]);
                DcvMeas(i);
            }










        }

        
    }

    public abstract class Oper4AcvMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
    {
        public Oper4AcvMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationTokenSource token)
       
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper5DcIMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
    {
        public Oper5DcIMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationTokenSource token)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper6AcIMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
    {
        public Oper6AcIMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationTokenSource token)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper7FreqMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
    {
        public Oper7FreqMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationTokenSource token)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper8OhmMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
    {
        public Oper8OhmMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationTokenSource token)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper9FarMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
    {
        public Oper9FarMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationTokenSource token)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper10TemperatureMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
    {
        public Oper10TemperatureMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationTokenSource token)
        
        {
            throw new NotImplementedException();
        }
    }


}

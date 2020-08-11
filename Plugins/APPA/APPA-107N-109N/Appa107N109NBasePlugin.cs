using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.APPA;
using DevExpress.Mvvm;


namespace APPA_107N_109N
{
    public class Appa107N109NBasePlugin : Program

    {
        public Appa107N109NBasePlugin(ServicePack service) : base(service)
        {
            
            Grsi = "20085-11";
            
        }

       
    }


    public class Operation : OperationMetrControlBase
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

    

    public abstract class OpertionFirsVerf : ASMC.Data.Model.Operation
    {
        
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            //Необходимые устройства
            ControlDevices = new IDevice[] { new Device {Name = new []{"5522A"}, Description = "Многофунциональный калибратор"}};
            TestDevices = new IDevice[]{ new Device { Name = new[] { "Appa-107N" }, Description = "Цифровой портативный мультиметр" } };


            Accessories = new[]
            {
                "Интерфейсный кабель для клибратора (GPIB или COM порт)",
                "Кабель banana - banana 2 шт.",
                "Интерфейсный кабель для прибора APPA-107N/APPA-109N USB-COM инфракрасный."
            };

            this.DocumentName = "appa";
        }

        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;

        }

        public override void FindDivice()
        {
            throw new NotImplementedException();
        }
    }
 

    public abstract class Oper1VisualTest : ParagraphBase, IUserItemOperation<bool>
    {
        public List<IBasicOperation<bool>> DataRow { get; set; }

        public Oper1VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }

        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Результат внешнего осмотра");
            var dataRow = data.NewRow();
            var dds = DataRow[0] as BasicOperation<bool>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting;
            data.Rows.Add(dataRow);
            return data;
        }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }



        public override async Task StartWork(CancellationToken cancellationToken)
        {
           
        }

        
    }

    public abstract class Oper2Oprobovanie : ParagraphBase, IUserItemOperation<bool>
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

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
       
        {
            var bo = new BasicOperation<bool> { Expected = true };
            //bo.IsGood = s => bo.Getting;

            DataRow.Add(bo);
        }

        
    }

    public abstract class Oper3DcvMeasure : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //базовые точки
        static decimal[] basePoint = { (decimal)0.004, (decimal)0.008, (decimal)0.012, (decimal)0.016, (decimal)0.018, (decimal)-0.018 };
        decimal[] dopPoint1000V = { 100, 200, 400, 700, 900, -900 };
        //множители для пределов
        decimal[] baseMultipliers = { 1, 10, 100, 1000, 10000 };
        //итоговая таблица с точками
        decimal[,] points = new decimal[basePoint.Length, basePoint.Length]; // это двумерный квадратный массив

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

        protected override void InitWork()
        {
            DataRow.Clear();
            //создаем таблицу точек
            //точки для пределов в вольтах
            //от 20 мВ до 200 В
            for (int i = 0; i < baseMultipliers.Length; i++)
            for (int j = 0; j < basePoint.Length; j++)
                points[i, j] = basePoint[j] * baseMultipliers[i];
            //дописываем точки на предел 1000 В
            for (int i = 0; i < dopPoint1000V.Length; i++)
                points[5, i] = dopPoint1000V[i];

            //теперь бежим по точкам
            for(int i =0; i < basePoint.Length;i++)
                for (int j = 0; j < basePoint.Length; j++)
                {
                    var operation = new BasicOperationVerefication<decimal>();
                    operation.InitWork = async () =>
                    {
                        try
                        {
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                        }
                    };
                }

        }


        protected override DataTable FillData()
        {
            throw new NotImplementedException();
            
        }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
       
        {
            
            
            flkCalib5522A.StringConnection = GetStringConnect(flkCalib5522A);
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
                            valuesMeasure.Add((decimal)appa107N.GetValue());

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

    public abstract class Oper4AcvMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper4AcvMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
       
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper5DcIMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper5DcIMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper6AcIMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper6AcIMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper7FreqMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper7FreqMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper8OhmMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper8OhmMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper9FarMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper9FarMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Oper10TemperatureMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper10TemperatureMeasure(IUserItemOperation userItemOperation):base(userItemOperation)
        {
            
        }
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        
        {
            throw new NotImplementedException();
        }
    }


}

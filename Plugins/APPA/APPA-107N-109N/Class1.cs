using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.APPA;
using DevExpress.Mvvm;

namespace APPA_107N_109N
{
    public class APPA_107N : IProgram

    {
        public APPA_107N()
        {
            Type = "Мультиметр цифровой";
            Grsi = "20085-11";
            Range = "Пост. напр. 0 - 1000 В, пер. напр. 0 - 750 В,\n" +
                    " пост./пер. ток 0 - 10 А, изм. частоты до 1 МГц,\n" +
                    " эл. сопр. 0 - 2 ГОм, эл. ёмкость до 40 мФ.";
            Accuracy = "DCV 0.06%, ACV 1%, DCI 0.2%, ACI 1.2%,\n" +
                       " FREQ 0.01%, OHM 5%, FAR 1.5%";
        }

        public string Type { get; }
        public string Grsi { get; }
        public string Range { get; }
        public string Accuracy { get; }
        public IMessageBoxService TaskMessageService { get; set; }
        public AbstraktOperation AbstraktOperation { get; }
    }

    public class Operation : AbstraktOperation
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.
        public Operation()
        {
            //это операция первичной поверки
            UserItemOperationPrimaryVerf = new OpertionFirsVerf();
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public class UsedDevices : IDevice
    {
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

    public class OpertionFirsVerf : IUserItemOperation
    {
        public string[] Accessories { get; }
        public string[] AddresDivece { get; set; }
        public IDevice[] ControlDevices { get; }
        public IDevice[] TestDevices { get; }
        public IDevice[] Device { get; }
        public IUserItemOperationBase[] UserItemOperation { get; }

        public OpertionFirsVerf()
        {
            //Необходимые устройства
            ControlDevices = new IDevice[] { new UsedDevices {Name = new []{"5522A"}, Description = "Многофунциональный калибратор"}};
            TestDevices = new IDevice[]{ new UsedDevices { Name = new[] { "Appa-107N" }, Description = "Цифровой портативный мультиметр" } };


            Accessories = new[]
            {
                "Интерфейсный кабель для клибратора (GPIB или COM порт)",
                "Кабель banana - banana 2 шт.",
                "Интерфейсный кабель для прибора APPA-10N USB-COM инфракрасный."
            };

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this),
                new Oper3DcvMeasure(this),
                new Oper4AcvMeasure(this),
                new Oper5DcIMeasure(this),
                new Oper6AcIMeasure(this),
                new Oper7FreqMeasure(this),
                new Oper8OhmMeasure(this),
                new Oper9FarMeasure(this),
                new Oper10TemperatureMeasure(this),
            };
        }

        public void RefreshDevice()
        {
            AddresDivece = new IeeeBase().GetAllDevace().ToArray();

        }

        public void FindDivice()
        {
            throw new NotImplementedException();
        }
    }
 

    public class Oper1VisualTest : AbstractUserItemOperationBase, IUserItemOperation<bool>
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

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            var bo = new BasicOperation<bool> { Expected = true };
            bo.IsGood = s => bo.Getting;

            DataRow.Add(bo);
        }

        
    }

    public class Oper2Oprobovanie : AbstractUserItemOperationBase, IUserItemOperation<bool>
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

        public override void StartWork()
        {
            var bo = new BasicOperation<bool> { Expected = true };
            bo.IsGood = s => bo.Getting;

            DataRow.Add(bo);
        }

        
    }

    public class Oper3DcvMeasure : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //список точек из методики поверки
        readonly decimal[] _pointsArr = { 4, 8, 12, 16, 18, -18 };

        public Oper3DcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения постоянного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "ShemePicture/5522A_NORMAL_DMM.jpg"

            };
        }


        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            Calib_5522A flkCalib5522A = new Calib_5522A();
            flkCalib5522A.SetStringconection(this.UserItemOperation.ControlDevices.First().StringConnect);
            //тут нужно проверять, если прибор не подключен, тогда прекращаем работу
            if (flkCalib5522A.Open())
            {
                
                return;
            }
            flkCalib5522A.Close();


            string serialPortName = this.UserItemOperation.ControlDevices.First().StringConnect;
            Mult107_109N appa107N = new Mult107_109N(serialPortName);
            appa107N.Open();

            

            decimal[] multiplaisArr = { (decimal)0.1, 1, 10, (decimal)0.001, (decimal)0.01 };
            var ranges = new[]
                {Mult107_109N.Range.Range1Manual, Mult107_109N.Range.Range2Manual, Mult107_109N.Range.Range3Manual};

            decimal DcvMeas(int point)
            {
                var countMeas = 10;
                decimal resultVal = 0;
               
                flkCalib5522A.WriteLine(Calib_5522A.Out.Set.Voltage.DC.SetValue(0, Calib_5522A.Multipliers.SI));
                flkCalib5522A.WriteLine(Calib_5522A.Out.State.ON.GetStringValue());

                for (int i = 0; i < _pointsArr.Length; i++)
                {
                    //поверяемая точка
                    flkCalib5522A.WriteLine(Calib_5522A.Out.Set.Voltage.DC.SetValue(_pointsArr[i] * multiplaisArr[point], Calib_5522A.Multipliers.SI));


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

    public class Oper4AcvMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
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

        public override void StartWork()
        {
            throw new NotImplementedException();
        }
    }

    public class Oper5DcIMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
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

        public override void StartWork()
        {
            throw new NotImplementedException();
        }
    }

    public class Oper6AcIMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
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

        public override void StartWork()
        {
            throw new NotImplementedException();
        }
    }

    public class Oper7FreqMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
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

        public override void StartWork()
        {
            throw new NotImplementedException();
        }
    }

    public class Oper8OhmMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
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

        public override void StartWork()
        {
            throw new NotImplementedException();
        }
    }

    public class Oper9FarMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
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

        public override void StartWork()
        {
            throw new NotImplementedException();
        }
    }

    public class Oper10TemperatureMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
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

        public override void StartWork()
        {
            throw new NotImplementedException();
        }
    }


}

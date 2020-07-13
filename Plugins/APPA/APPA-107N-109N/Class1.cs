using System;
using System.Collections.Generic;
using System.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE.Fluke.Calibrator;

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
        public IDevice[] Device { get; }
        public IUserItemOperationBase[] UserItemOperation { get; }

        public OpertionFirsVerf()
        {
            //Необходимые устройства
            Device = new IDevice[]
            {
                new UsedDevices {Name = new []{"5522A"}, Description = "Многофунциональный калибратор"},
                new UsedDevices {Name = new[]{"Appa-107N"}, Description = "Цифровой портативный мультиметр"}

            };

            Accessories = new[]
            {
                "Интерфейсный кабель для клибратора (GPIB или COM порт)",
                "Кабель banana - banana 2 шт.",
                "Интерфейсный кабель для прибора APPA-10N USB-COM инфракрасный."
            };

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(),
                new Oper2Oprobovanie(),
                new Oper3DcvMeasure(),
                new Oper4AcvMeasure(),
                new Oper5DcIMeasure(),
                new Oper6AcIMeasure(),
                new Oper7FreqMeasure(),
                new Oper8OhmMeasure(),
                new Oper9FarMeasure(),
                new Oper10TemperatureMeasure(),
            };
        }

        public void RefreshDevice()
        {
            throw new NotImplementedException();
        }

    }
 

    public class Oper1VisualTest : AbstractUserItemOperationBase, IUserItemOperation<bool>
    {
        public List<IBasicOperation<bool>> DataRow { get; set; }

        public Oper1VisualTest()
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

        public Oper2Oprobovanie()
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

        public Oper3DcvMeasure()
        {
            Name = "Определение погрешности измерения постоянног напряжения";
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

        public override void StartWork()
        {
            Calib_5522A flkCalib5522A = new Calib_5522A();
            flkCalib5522A.Devace();
            flkCalib5522A.WriteLine(Calib_5522A.Out.Set.Voltage.DC.SetValue((decimal)5));

        }

        
    }

    public class Oper4AcvMeasure : AbstractUserItemOperationBase, IUserItemOperationBase
    {
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

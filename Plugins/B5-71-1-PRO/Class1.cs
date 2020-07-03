using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devises.IEEE.Keysight.ElectronicLoad;
using ASMC.Devises.IEEE.Keysight.Multimeter;
using ASMC.Devises.Port.Profigrupp;


namespace B5_71_PRO
{
    public class B5_71_1PRO : IProrgam
    {
        public B5_71_1PRO()
        {
            AbstraktOperation = new Operation();
            Type = "Б5-71/1-ПРО";
            Grsi = "42467-09";
            Range = "0 - 30 В; 0 - 10 А";
            Accuracy = "Напряжение ПГ ±(0,002 * U + 0.1), ток ПГ ±(0,01 * I + 0,05)";
        }
        public string Type { get; }
        public string Grsi { get; }
        public string Range { get; }
        public string Accuracy { get; }
        public AbstraktOperation AbstraktOperation { get; }
    }

    public class Operation : AbstraktOperation
    {
      
        public Operation()
        {
            this.UserItemOperationPrimaryVerf = new OpertionFirsVerf();


        }
    }



    public class StandartDevices : IDevice
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
        public IDevice[] Device { get; }
        public IUserItemOperationBase[] UserItemOperation { get; }
        public string[] Accessories { get; }

        public OpertionFirsVerf()
        {
            //Необходимые эталоны
            Device = new []
            {
                new StandartDevices { Name = new []{"N3300A"},  Description = "Электронная нагрузка"},
                new StandartDevices{ Name = new []{"344010A"},  Description = "Мультиметр"}, 
                new StandartDevices{ Name = new []{"В3-57"}, Description = "Микровольтметр"} 

            };

            //Необходимые аксесуары
            Accessories = new[]
            {
                "Преобразователь интерфесов National Instruments GPIB-USB",
                "Преобразователь интерфесов USB - RS-232 + нульмодемный кабебель",
                "Кабель banana - banana 6 шт.",
                "Кабель BNC - banan для В3-57"
            };

            //Перечень операций поверки
            UserItemOperation = new IUserItemOperationBase[]{new ItemOperation1(),
                new ItemOperation2(),
                new ItemOperation3(),
                new ItemOperation4(),
                new ItemOperation5(), 
                new ItemOperation6(), 
                new ItemOperation7(), 
                new ItemOperation8(), 
                new ItemOperation9()
            };

        }

      public void RefreshDevice()
        {
            foreach (var dev in Device)
            {

            }
        }   
    }


    /// <summary>
    /// Проведение опробования
    /// </summary>
    public class ItemOperation1 : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public ItemOperation1()
        {
            Name = "Опробование";
        }

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

        public List<IBasicOperation<string>> DataRow { get; set; }
    }

    /// <summary>
    /// Воспроизведение постоянного напряжения
    /// </summary>
    public class ItemOperation2 : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public List<IBasicOperation<string>> DataRow { get; set; }

        public ItemOperation2()
        {
            this.Name = "Определение погрешности установки выходного напряжения";
            Sheme = new ShemeImage
            {
            Number = 1,
            Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {

            N3306A n3306a = new N3306A(1);
            n3306a.Devace();
            n3306a.Connection();

            //массив всех установленных модулей
            string[] InstalledMod = n3306a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3306a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3306a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3306a.SetWorkingChanel();
            n3306a.OffOutput();
            n3306a.Close();


        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        
    }


    /// <summary>
    /// Измерение постоянного напряжения 
    /// </summary>
    public class ItemOperation3 : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public ItemOperation3()
        {
            Name = "Определение погрешности измерения выходного напряжения";
        }

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

        public List<IBasicOperation<string>> DataRow { get; set; }
    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public class ItemOperation4 : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public ItemOperation4()
        {
            Name = "Определение нестабильности выходного напряжения";
        }

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

        public List<IBasicOperation<string>> DataRow { get; set; }
    }

    /// <summary>
    /// Опрделение уровня пульсаций
    /// </summary>
    public class ItemOperation5 : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public ItemOperation5()
        {
            Name = "Определение уровня пульсаций по напряжению";
        }

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

        public List<IBasicOperation<string>> DataRow { get; set; }
    }



    /// <summary>
    /// Определение погрешности установки выходного тока
    /// </summary>
    public class ItemOperation6 : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public ItemOperation6()
        {
            Name = "Определение погрешности установки выходного тока";
        }

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

        public List<IBasicOperation<string>> DataRow { get; set; }
    }


    /// <summary>
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public class ItemOperation7 : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public ItemOperation7()
        {
            Name = "Определение погрешности измерения выходного тока";
        }

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

        public List<IBasicOperation<string>> DataRow { get; set; }
    }


    /// <summary>
    /// Определение нестабильности выходного тока
    /// </summary>
    public class ItemOperation8 : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public ItemOperation8()
        {
            Name = "Определение нестабильности выходного тока";
        }

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

        public List<IBasicOperation<string>> DataRow { get; set; }
    }



    /// <summary>
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public class ItemOperation9 : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public ItemOperation9()
        {
            Name = "Определение уровня пульсаций постоянного тока";
        }

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

        public List<IBasicOperation<string>> DataRow { get; set; }
    }

}



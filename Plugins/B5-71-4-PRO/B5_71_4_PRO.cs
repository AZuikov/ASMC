using ASMC.Data.Model;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Port.Profigrupp;
using B5_71_PRO_Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace B5_71_4_PRO
{
    public class B5_71_4PRO : AbstractB571ProPlugin
    {
        public B5_71_4PRO(ServicePack service) : base(service)
        {
            this.Range = "0 - 75 В, 0 - 4 А";
            this.Type = "Б5-71/4-ПРО";
            Operation = new Operation(service);
        }
    }

    public class Operation : OperationMetrControlBase
    {
        public Operation(ServicePack servicePack)
        {
            this.UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePack);
            //здесь периодическая поверка, но набор операций такой же
            this.UserItemOperationPeriodicVerf = this.UserItemOperationPrimaryVerf;
        }
    }

    public class OpertionFirsVerf : B5_71_PRO_Abstract.OpertionFirsVerf
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            ControlDevices = new IDevice[]
            {
                new Device { Name = new []{"N3300A"},  Description = "Электронная нагрузка"},
                new Device{ Name = new []{"34401A"},  Description = "Мультиметр"},
                new Device{ Name = new []{"В3-57"}, Description = "Микровольтметр", IsCanStringConnect = false}
            };

            TestDevices = new IDevice[] { new Device { Name = new[] { "Б5-71/4-ПРО" }, Description = "источник питания" } };

            //Необходимые аксесуары
            Accessories = new[]
            {
                "Нагрузка электронная Keysight N3300A с модулем n3303a",
                "Мультиметр цифровой Agilent/Keysight 34401A",
                "Преобразователь интерфесов National Instruments GPIB-USB",
                "Преобразователь интерфесов USB - RS-232 + нуль-модемный кабель",
                "Кабель banana - banana 6 шт.",
                "Кабель BNC - banan для В3-57"
            };

            UserItemOperation = new IUserItemOperationBase[]
            {
                //new Oper0VisualTest(this),
                //new Oper1Oprobovanie(this),
                new Oper2DcvOutput(this),
                new Oper3DcvMeasure(this),
                new Oper4VoltUnstable(this),
                new Oper6DciOutput(this),
                new Oper7DciMeasure(this),
                new Oper8DciUnstable(this),
                new Oper5VoltPulsation(this),
                new Oper9DciPulsation(this)
            };
        }
    }

    public class Oper0VisualTest : B5_71_PRO_Abstract.Oper0VisualTest
    {
        public Oper0VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }

    public class Oper1Oprobovanie : B5_71_PRO_Abstract.Oper1Oprobovanie
    {
        public Oper1Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        protected override void InitWork()
        {
            throw new NotImplementedException();
        }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Воспроизведение постоянного напряжения
    /// </summary>
    public class Oper2DcvOutput : B5_71_PRO_Abstract.Oper2DcvOutput
    {
        public Oper2DcvOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro4();
            Mult = new Mult_34401A();
            Load = new N3303A();
            Sheme = ShemeTemplate.TemplateSheme;
        }
    }

    /// <summary>
    /// Измерение постоянного напряжения
    /// </summary>
    public class Oper3DcvMeasure : B5_71_PRO_Abstract.Oper3DcvMeasure
    {
        public Oper3DcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro4();
            Mult = new Mult_34401A();
            Load = new N3303A();
            Sheme = ShemeTemplate.TemplateSheme;
        }
    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public class Oper4VoltUnstable : B5_71_PRO_Abstract.Oper4VoltUnstable
    {
        public Oper4VoltUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro4();
            Mult = new Mult_34401A();
            Load = new N3303A();
            Sheme = ShemeTemplate.TemplateSheme;
        }
    }

    /// <summary>
    /// Опрделение уровня пульсаций
    /// </summary>
    public class Oper5VoltPulsation : B5_71_PRO_Abstract.Oper5VoltPulsation
    {
        public Oper5VoltPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro4();
            Mult = new Mult_34401A();
            Load = new N3303A();
            Sheme = ShemeTemplate.TemplateSheme;
        }
    }

    /// <summary>
    /// Определение погрешности установки выходного тока
    /// </summary>
    public class Oper6DciOutput : B5_71_PRO_Abstract.Oper6DciOutput
    {
        public Oper6DciOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro4();
            Load = new N3303A();
            Sheme = ShemeTemplate.TemplateSheme;
        }
    }

    /// <summary>
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public class Oper7DciMeasure : B5_71_PRO_Abstract.Oper7DciMeasure
    {
        public Oper7DciMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro4();
            Load = new N3303A();
            Sheme = ShemeTemplate.TemplateSheme;
        }
    }

    /// <summary>
    /// Определение нестабильности выходного тока
    /// </summary>
    public class Oper8DciUnstable : B5_71_PRO_Abstract.Oper8DciUnstable
    {
        public Oper8DciUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro4();
            Load = new N3303A();
            Sheme = ShemeTemplate.TemplateSheme;
        }
    }

    /// <summary>
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public class Oper9DciPulsation : B5_71_PRO_Abstract.Oper9DciPulsation
    {
        public Oper9DciPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro4();
            Load = new N3303A();
            Mult = new Mult_34401A();
            Sheme = ShemeTemplate.TemplateSheme;
        }
    }

    internal static class ShemeTemplate
    {
        public static ShemeImage TemplateSheme = new ShemeImage { Description = "Измерительная схема", Number = 1, FileName = @"B5-71-4-PRO_N3303_34401_v3-57.jpg", ExtendedDescription = "Соберите измерительную схему, согласно рисунку" };
    }
}
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Port.Profigrupp;
using ASMC.Devices.WithoutInterface.Voltmetr;
using B5_71_PRO_Abstract;

// !!!!!!!! Внимание !!!!!!!!!
//  Имя последовательного порта прописано жестко!!!!
// Необходимо реализовать его настройку из ВНЕ - через интрефейс ASMC

namespace B5_71_2_PRO
{
    public class B5_71_2PRO : AbstractB571ProPlugin<Operation>
    {
        public B5_71_2PRO(ServicePack service) : base(service)
        {
            Type = "Б5-71/2-ПРО";
            Range = "0 - 50 В; 0 - 6 А";
        }
    }

    public class Operation : OperationMetrControlBase
    {
        public Operation(ServicePack servicePack)
        {
            UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePack);
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public class OpertionFirsVerf : B5_71_PRO_Abstract.OpertionFirsVerf
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            ControlDevices = new IDeviceUi[]
            {new Device
                    { Devices = new IDeviceBase[] { new N3303A() }, Description = "Электронная нагрузка"},
                new Device{Devices = new IDeviceBase[] { new Mult_34401A() }, Description = "Мультиметр"},
                new Device{Devices = new IUserType[] { new B3_57(),  }, Description = "Микровольтметр", IsCanStringConnect = false}
            };
            TestDevices = new IDeviceUi[]
            {new Device
                { Devices = new IDeviceBase[] { new B571Pro2(),  }, Description = "источник питания"}
            };

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper0VisualTest(this),
                new Oper1Oprobovanie(this),
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
            Bp = new B571Pro2();
            Mult = new Mult_34401A();
            Load = new N3303A();
        }
    }

    /// <summary>
    /// Воспроизведение постоянного напряжения
    /// </summary>
    public class Oper2DcvOutput : B5_71_PRO_Abstract.Oper2DcvOutput
    {
        public Oper2DcvOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro2();
            Mult = new Mult_34401A();
            Load = new N3303A();
        }
    }

    /// <summary>
    /// Измерение постоянного напряжения
    /// </summary>
    public class Oper3DcvMeasure : B5_71_PRO_Abstract.Oper3DcvMeasure
    {
        public Oper3DcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro2();
            Mult = new Mult_34401A();
            Load = new N3303A();
        }
    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public class Oper4VoltUnstable : B5_71_PRO_Abstract.Oper4VoltUnstable
    {
        public Oper4VoltUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro2();
            Mult = new Mult_34401A();
            Load = new N3303A();
        }
    }

    /// <summary>
    /// Опрделение уровня пульсаций
    /// </summary>
    public class Oper5VoltPulsation : B5_71_PRO_Abstract.Oper5VoltPulsation
    {
        public Oper5VoltPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro2();
            Mult = new Mult_34401A();
            Load = new N3303A();
        }
    }

    /// <summary>
    /// Определение погрешности установки выходного тока
    /// </summary>
    public class Oper6DciOutput : B5_71_PRO_Abstract.Oper6DciOutput
    {
        public Oper6DciOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro2();
            Load = new N3303A();
        }
    }

    /// <summary>
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public class Oper7DciMeasure : B5_71_PRO_Abstract.Oper7DciMeasure
    {
        public Oper7DciMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro2();
            Load = new N3303A();
        }
    }

    /// <summary>
    /// Определение нестабильности выходного тока
    /// </summary>
    public class Oper8DciUnstable : B5_71_PRO_Abstract.Oper8DciUnstable
    {
        public Oper8DciUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro2();
            Load = new N3303A();
        }
    }

    /// <summary>
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public class Oper9DciPulsation : B5_71_PRO_Abstract.Oper9DciPulsation
    {
        public Oper9DciPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Bp = new B571Pro2();
            Load = new N3303A();
            Mult = new Mult_34401A();
        }
    }
}
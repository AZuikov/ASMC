using APPA_107N_109N;
using ASMC.Data.Model;

namespace Appa107N
{
    public class Class1 :Appa107N109NBasePlugin
    {
        
        public Class1() 
        {
            this.Type = "APPA-107N";
            this.Range = "Пост. напр. 0 - 1000 В, пер. напр. 0 - 750 В,\n" +
                                      " пост./пер. ток 0 - 10 А, изм. частоты до 1 МГц,\n" +
                                      " эл. сопр. 0 - 2 ГОм, эл. ёмкость до 40 мФ.";
            this.Accuracy = "DCV 0.06%, ACV 1%, DCI 0.2%, ACI 1.2%,\n" +
                                         " FREQ 0.01%, OHM 5%, FAR 1.5%";
        }
    }

    public class OpertionFirsVerf : APPA_107N_109N.OpertionFirsVerf
    {
        public OpertionFirsVerf()
        {
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
    }

    public class Oper1VisualTest : APPA_107N_109N.Oper1VisualTest
    {
        public Oper1VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }

    public class Oper2Oprobovanie : APPA_107N_109N.Oper2Oprobovanie
    {
        public Oper2Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }


    public class Oper3DcvMeasure : APPA_107N_109N.Oper3DcvMeasure
    {
        public Oper3DcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }

    public class Oper4AcvMeasure : APPA_107N_109N.Oper4AcvMeasure
    {
        public Oper4AcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }

    public class Oper5DcIMeasure : APPA_107N_109N.Oper5DcIMeasure
    {
        public Oper5DcIMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }

    public class Oper6AcIMeasure : APPA_107N_109N.Oper6AcIMeasure
    {
        public Oper6AcIMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }

    public class Oper7FreqMeasure : APPA_107N_109N.Oper7FreqMeasure

    {
        public Oper7FreqMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }

    public class Oper8OhmMeasure : APPA_107N_109N.Oper8OhmMeasure
    {
        public Oper8OhmMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }

    public class Oper9FarMeasure : APPA_107N_109N.Oper9FarMeasure
    {
        public Oper9FarMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }


    public class Oper10TemperatureMeasure : APPA_107N_109N.Oper10TemperatureMeasure
    {
        public Oper10TemperatureMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }
}

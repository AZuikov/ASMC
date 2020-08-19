using System;
using APPA_107N_109N;
using ASMC.Data.Model;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.APPA;

namespace Appa107N
{
    public class APPA107N : Appa107N109NBasePlugin
    {
        public APPA107N(ServicePack servicePack) : base(servicePack)
        {
            Type = "APPA-107N";
            Range = "Пост. напр. 0 - 1000 В, пер. напр. 0 - 750 В,\n" +
                    " пост./пер. ток 0 - 10 А, изм. частоты до 1 МГц,\n" +
                    " эл. сопр. 0 - 2 ГОм, эл. ёмкость до 40 мФ.";
            Accuracy = "DCV 0.06%, ACV 1%, DCI 0.2%, ACI 1.2%,\n" +
                       " FREQ 0.01%, OHM 5%, FAR 1.5%";
            Operation = new Operation(servicePack);
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

    public class OpertionFirsVerf : APPA_107N_109N.OpertionFirsVerf
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            var DcvMode = new Oper3DcvMeasureBase(this);
            DcvMode.Nodes.Add(new Oper3_1DC_20mV_Measure(Mult107_109N.RangeNominal.Range20mV, this));
            DcvMode.Nodes.Add(new Oper3_1DC_200mV_Measure(Mult107_109N.RangeNominal.Range200mV, this));
            DcvMode.Nodes.Add(new Oper3_1DC_2V_Measure(Mult107_109N.RangeNominal.Range2V, this));
            //DcvMode.Nodes.Add(new Oper3_1DC_20V_Measure(Mult107_109N.RangeNominal.Range20V, this));
            //DcvMode.Nodes.Add(new Oper3_1DC_200V_Measure(Mult107_109N.RangeNominal.Range200V, this));
            //DcvMode.Nodes.Add(new Oper3_1DC_1000V_Measure(Mult107_109N.RangeNominal.Range1000V, this));

            var AcvMode = new Oper4AcvMeasureBase(this);
            AcvMode.Nodes.Add(new Ope4_1_AcV_20mV_Measure(Mult107_109N.RangeNominal.Range20mV, this));
            AcvMode.Nodes.Add(new Ope4_1_AcV_200mV_Measure(Mult107_109N.RangeNominal.Range200mV, this));
            AcvMode.Nodes.Add(new Ope4_1_AcV_2V_Measure(Mult107_109N.RangeNominal.Range2V, this));
            AcvMode.Nodes.Add(new Ope4_1_AcV_20V_Measure(Mult107_109N.RangeNominal.Range20V, this));
            AcvMode.Nodes.Add(new Ope4_1_AcV_200V_Measure(Mult107_109N.RangeNominal.Range200V, this));
            AcvMode.Nodes.Add(new Ope4_1_AcV_1000V_Measure(Mult107_109N.RangeNominal.Range1000V, this));

            UserItemOperation = new IUserItemOperationBase[]
            {
                //new Oper1VisualTest(this),

                //new Oper2Oprobovanie(this),

                DcvMode,
                //AcvMode
                //new Oper4AcvMeasure(this),
                //new Oper5DcIMeasure(this),
                //new Oper6AcIMeasure(this),
                //new Oper7FreqMeasure(this),
                //new Oper8OhmMeasure(this),
                //new Oper9FarMeasure(this),
                //new Oper10TemperatureMeasure(this),
            };
        }

        #region Methods

        public override void FindDivice()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //public class Oper3DcvMeasureBase : APPA_107N_109N.Oper3DcvMeasureBase
    //{
    //    public Oper3DcvMeasureBase(IUserItemOperation userItemOperation) : base(userItemOperation)
    //    {
    //        appa107N = new MultAPPA107N();
    //        flkCalib5522A = new Calib5522A();
    //    }
    //}

    public class Oper1VisualTest : APPA_107N_109N.Oper1VisualTest
    {
        public Oper1VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }
    }

    #region DCV

    public class Oper3_1DC_20mV_Measure : APPA_107N_109N.Oper3_1DC_20mV_Measure
    {
        public Oper3_1DC_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCmV;
        }
    }

    public class Oper3_1DC_200mV_Measure : APPA_107N_109N.Oper3_1DC_200mV_Measure
    {
        public Oper3_1DC_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCmV;
        }
    }

    public class Oper3_1DC_2V_Measure : APPA_107N_109N.Oper3_1DC_2V_Measure
    {
        public Oper3_1DC_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCV;
        }
    }

    public class Oper3_1DC_20V_Measure : APPA_107N_109N.Oper3_1DC_20V_Measure
    {
        public Oper3_1DC_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCV;
        }
    }

    public class Oper3_1DC_200V_Measure : APPA_107N_109N.Oper3_1DC_200V_Measure
    {
        public Oper3_1DC_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCV;
        }
    }

    public class Oper3_1DC_1000V_Measure : APPA_107N_109N.Oper3_1DC_1000V_Measure
    {
        public Oper3_1DC_1000V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCV;
        }
    }

    #endregion DCV

    #region ACV

    public class Ope4_1_AcV_20mV_Measure : APPA_107N_109N.Ope4_1_AcV_20mV_Measure
    {
        public Ope4_1_AcV_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
        }
    }

    public class Ope4_1_AcV_200mV_Measure : APPA_107N_109N.Ope4_1_AcV_200mV_Measure
    {
        public Ope4_1_AcV_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
        }
    }

    public class Ope4_1_AcV_2V_Measure : APPA_107N_109N.Ope4_1_AcV_2V_Measure
    {
        public Ope4_1_AcV_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
        }
    }

    public class Ope4_1_AcV_20V_Measure : APPA_107N_109N.Ope4_1_AcV_20V_Measure
    {
        public Ope4_1_AcV_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
        }
    }

    public class Ope4_1_AcV_200V_Measure : APPA_107N_109N.Ope4_1_AcV_200V_Measure
    {
        public Ope4_1_AcV_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
        }
    }

    public class Ope4_1_AcV_1000V_Measure : APPA_107N_109N.Ope4_1_AcV_1000V_Measure
    {
        public Ope4_1_AcV_1000V_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
        }
    }

    #endregion ACV
}
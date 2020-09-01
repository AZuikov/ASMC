using System;
using APPA_107N_109N;
using ASMC.Core.Model;
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
            DcvMode.Nodes.Add(new Oper3_1DC_20V_Measure(Mult107_109N.RangeNominal.Range20V, this));
            DcvMode.Nodes.Add(new Oper3_1DC_200V_Measure(Mult107_109N.RangeNominal.Range200V, this));
            DcvMode.Nodes.Add(new Oper3_1DC_1000V_Measure(Mult107_109N.RangeNominal.Range1000V, this));

            var AcvMode = new Oper4AcvMeasureBase(this);
            AcvMode.Nodes.Add(new Ope4_1_AcV_20mV_Measure(Mult107_109N.RangeNominal.Range20mV, this));
            AcvMode.Nodes.Add(new Ope4_1_AcV_200mV_Measure(Mult107_109N.RangeNominal.Range200mV, this));
            AcvMode.Nodes.Add(new Ope4_1_AcV_2V_Measure(Mult107_109N.RangeNominal.Range2V, this));
            AcvMode.Nodes.Add(new Ope4_1_AcV_20V_Measure(Mult107_109N.RangeNominal.Range20V, this));
            AcvMode.Nodes.Add(new Ope4_1_AcV_200V_Measure(Mult107_109N.RangeNominal.Range200V, this));
            AcvMode.Nodes.Add(new Ope41AcV750VMeasure(Mult107_109N.RangeNominal.Range750V, this));

            var FarMode = new Oper9FarMeasureBase(this);
            FarMode.Nodes.Add(new Oper9_1Far_4nF_Measure(Mult107_109N.RangeNominal.Range4nF, this));
            FarMode.Nodes.Add(new Oper9_1Far_40nF_Measure(Mult107_109N.RangeNominal.Range40nF, this));
            FarMode.Nodes.Add(new Oper9_1Far_400nF_Measure(Mult107_109N.RangeNominal.Range400nF, this));
            FarMode.Nodes.Add(new Oper9_1Far_4uF_Measure(Mult107_109N.RangeNominal.Range4uF, this));
            FarMode.Nodes.Add(new Oper9_1Far_40uF_Measure(Mult107_109N.RangeNominal.Range40uF, this));
            FarMode.Nodes.Add(new Oper9_1Far_400uF_Measure(Mult107_109N.RangeNominal.Range400uF, this));
            FarMode.Nodes.Add(new Oper9_1Far_4mF_Measure(Mult107_109N.RangeNominal.Range4mF, this));
            FarMode.Nodes.Add(new Oper9_1Far_40mF_Measure(Mult107_109N.RangeNominal.Range40mF, this));

            var OhmMode = new Oper8ResistanceMeasureBase(this);
            OhmMode.Nodes.Add(new Oper8_1Resistance_200Ohm_Measure(Mult107_109N.RangeNominal.Range200Ohm, this));
            OhmMode.Nodes.Add(new Oper8_1Resistance_2kOhm_Measure(Mult107_109N.RangeNominal.Range2kOhm, this));
            OhmMode.Nodes.Add(new Oper8_1Resistance_20kOhm_Measure(Mult107_109N.RangeNominal.Range20kOhm, this));
            OhmMode.Nodes.Add(new Oper8_1Resistance_200kOhm_Measure(Mult107_109N.RangeNominal.Range200kOhm, this));
            OhmMode.Nodes.Add(new Oper8_1Resistance_2MOhm_Measure(Mult107_109N.RangeNominal.Range2Mohm, this));
            OhmMode.Nodes.Add(new Oper8_1Resistance_20MOhm_Measure(Mult107_109N.RangeNominal.Range20Mohm, this));
            OhmMode.Nodes.Add(new Oper8_1Resistance_200MOhm_Measure(Mult107_109N.RangeNominal.Range200Mohm, this));
            OhmMode.Nodes.Add(new Oper8_1Resistance_2GOhm_Measure(Mult107_109N.RangeNominal.Range2Gohm, this));

            var DciMode = new Oper5DciMeasureBase(this);
            DciMode.Nodes.Add(new Oper5_1Dci_20mA_Measure(Mult107_109N.RangeNominal.Range20mA, this));
            DciMode.Nodes.Add(new Oper5_1Dci_200mA_Measure(Mult107_109N.RangeNominal.Range200mA, this));
            DciMode.Nodes.Add(new Oper5_1Dci_2A_Measure(Mult107_109N.RangeNominal.Range2A, this));
            DciMode.Nodes.Add(new Oper5_2_1Dci_10A_Measure(Mult107_109N.RangeNominal.Range10A, this));
            DciMode.Nodes.Add(new Oper5_2_2Dci_10A_Measure(Mult107_109N.RangeNominal.Range10A, this));

            var AciMode = new Oper6AciMeasureBase(this);
            AciMode.Nodes.Add(new Oper6_1Aci_20mA_Measure(Mult107_109N.RangeNominal.Range20mA, this));
            AciMode.Nodes.Add(new Oper6_1Aci_200mA_Measure(Mult107_109N.RangeNominal.Range200mA, this));
            AciMode.Nodes.Add(new Oper6_1Aci_2A_Measure(Mult107_109N.RangeNominal.Range2A, this));
            AciMode.Nodes.Add(new Oper6_2_1Aci_10A_Measure(Mult107_109N.RangeNominal.Range10A,this));
            AciMode.Nodes.Add(new Oper6_2_2Aci_10A_Measure(Mult107_109N.RangeNominal.Range10A,this));

            var FreqMode = new Oper7FreqMeasureBase(this);
            FreqMode.Nodes.Add(new Oper71Freq20HzMeasureBase(Mult107_109N.RangeNominal.Range20Hz,this));
            FreqMode.Nodes.Add(new Oper71Freq200HzMeasureBase(Mult107_109N.RangeNominal.Range200Hz,this));
            FreqMode.Nodes.Add(new Oper71Freq2kHzMeasureBase(Mult107_109N.RangeNominal.Range2kHz,this));
            FreqMode.Nodes.Add(new Oper71Freq20kHzMeasureBase(Mult107_109N.RangeNominal.Range20kHz,this));
            FreqMode.Nodes.Add(new Oper71Freq200kHzMeasureBase(Mult107_109N.RangeNominal.Range200kHz,this));
            FreqMode.Nodes.Add(new Oper71Freq1MHzMeasureBase(Mult107_109N.RangeNominal.Range1MHz,this));

            var TempMode = new Oper10TemperatureMeasureBase(this);
            TempMode.Nodes.Add(new Oper10_1Temperature_Minus200_Minus100_Measure(Mult107_109N.RangeNominal.Range400degC,this));
            TempMode.Nodes.Add(new Oper10_1Temperature_Minus100_400_Measure(Mult107_109N.RangeNominal.Range400degC,this));
            TempMode.Nodes.Add(new Oper10_1Temperature_400_1200_Measure(Mult107_109N.RangeNominal.Range1200degC,this));
            
            

                UserItemOperation = new IUserItemOperationBase[]
                {
                    //new Oper1VisualTest(this),
                    //new Oper2Oprobovanie(this),
                    //DcvMode,
                    AcvMode, 
                    //OhmMode,
                    //FarMode,
                    //TempMode,
                    //FreqMode,
                    //DciMode,
                    //AciMode
                    
                };
        }

        #region Methods

        public override void FindDevice()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

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
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACmV;
        }
    }

    public class Ope4_1_AcV_200mV_Measure : APPA_107N_109N.Ope4_1_AcV_200mV_Measure
    {
        public Ope4_1_AcV_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACmV;
        }
    }

    public class Ope4_1_AcV_2V_Measure : APPA_107N_109N.Ope4_1_AcV_2V_Measure
    {
        public Ope4_1_AcV_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
        }
    }

    public class Ope4_1_AcV_20V_Measure : APPA_107N_109N.Ope4_1_AcV_20V_Measure
    {
        public Ope4_1_AcV_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
        }
    }

    public class Ope4_1_AcV_200V_Measure : APPA_107N_109N.Ope4_1_AcV_200V_Measure
    {
        public Ope4_1_AcV_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
        }
    }

    public class Ope41AcV750VMeasure : Ope4_1_AcV_750V_Measure
    {
        public Ope41AcV750VMeasure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
        }
    }

    #endregion ACV

    #region OHM

    public class Oper8_1Resistance_200Ohm_Measure : APPA_107N_109N.Oper8_1Resistance_200Ohm_Measure
    {
        public Oper8_1Resistance_200Ohm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Oper8_1Resistance_2kOhm_Measure : APPA_107N_109N.Oper8_1Resistance_2kOhm_Measure
    {
        public Oper8_1Resistance_2kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Oper8_1Resistance_20kOhm_Measure : APPA_107N_109N.Oper8_1Resistance_20kOhm_Measure
    {
        public Oper8_1Resistance_20kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Oper8_1Resistance_200kOhm_Measure : APPA_107N_109N.Oper8_1Resistance_200kOhm_Measure
    {
        public Oper8_1Resistance_200kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Oper8_1Resistance_2MOhm_Measure : APPA_107N_109N.Oper8_1Resistance_2MOhm_Measure
    {
        public Oper8_1Resistance_2MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Oper8_1Resistance_20MOhm_Measure : APPA_107N_109N.Oper8_1Resistance_20MOhm_Measure
    {
        public Oper8_1Resistance_20MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Oper8_1Resistance_200MOhm_Measure : APPA_107N_109N.Oper8_1Resistance_200MOhm_Measure
    {
        public Oper8_1Resistance_200MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Oper8_1Resistance_2GOhm_Measure : APPA_107N_109N.Oper8_1Resistance_2GOhm_Measure
    {
        public Oper8_1Resistance_2GOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    #endregion OHM

    #region FAR

    public class Oper9_1Far_4nF_Measure : APPA_107N_109N.Oper9_1Far_4nF_Measure
    {
        public Oper9_1Far_4nF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Oper9_1Far_40nF_Measure : APPA_107N_109N.Oper9_1Far_40nF_Measure
    {
        public Oper9_1Far_40nF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Oper9_1Far_400nF_Measure : APPA_107N_109N.Oper9_1Far_400nF_Measure
    {
        public Oper9_1Far_400nF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Oper9_1Far_4uF_Measure : APPA_107N_109N.Oper9_1Far_4uF_Measure
    {
        public Oper9_1Far_4uF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Oper9_1Far_40uF_Measure : APPA_107N_109N.Oper9_1Far_40uF_Measure
    {
        public Oper9_1Far_40uF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Oper9_1Far_400uF_Measure : APPA_107N_109N.Oper9_1Far_400uF_Measure
    {
        public Oper9_1Far_400uF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Oper9_1Far_4mF_Measure : APPA_107N_109N.Oper9_1Far_4mF_Measure
    {
        public Oper9_1Far_4mF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Oper9_1Far_40mF_Measure : APPA_107N_109N.Oper9_1Far_40mF_Measure
    {
        public Oper9_1Far_40mF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    #endregion FAR

    #region DCI

    public class Oper5_1Dci_20mA_Measure : APPA_107N_109N.Oper5_1Dci_20mA_Measure
    {
        public Oper5_1Dci_20mA_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Oper5_1Dci_200mA_Measure : APPA_107N_109N.Oper5_1Dci_200mA_Measure
    {
        public Oper5_1Dci_200mA_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
           
        }
    }

    public class Oper5_1Dci_2A_Measure : APPA_107N_109N.Oper5_1Dci_2A_Measure
    {
        public Oper5_1Dci_2A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Oper5_2_1Dci_10A_Measure : APPA_107N_109N.Oper5_2_1Dci_10A_Measure
    {
        public Oper5_2_1Dci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
           
        }
    }

    public class Oper5_2_2Dci_10A_Measure : APPA_107N_109N.Oper5_2_2Dci_10A_Measure
    {
        public Oper5_2_2Dci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    #endregion DCI

    #region ACI

    public class Oper6_1Aci_20mA_Measure : APPA_107N_109N.Oper6_1Aci_20mA_Measure
    {
        public Oper6_1Aci_20mA_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Oper6_1Aci_200mA_Measure : APPA_107N_109N.Oper6_1Aci_200mA_Measure
    
    {
        public Oper6_1Aci_200mA_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Oper6_1Aci_2A_Measure : APPA_107N_109N.Oper6_1Aci_2A_Measure
    {
        public Oper6_1Aci_2A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Oper6_2_1Aci_10A_Measure : APPA_107N_109N.Oper6_2_1Aci_10A_Measure
    {
        public Oper6_2_1Aci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Oper6_2_2Aci_10A_Measure : APPA_107N_109N.Oper6_2_2Aci_10A_Measure
    {
        public Oper6_2_2Aci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }


    #endregion

    #region Freq

    public class Oper71Freq20HzMeasureBase : APPA_107N_109N.Oper71Freq20HzMeasureBase
    {
        public Oper71Freq20HzMeasureBase(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    public class Oper71Freq200HzMeasureBase : APPA_107N_109N.Oper71Freq200HzMeasureBase
    {
        public Oper71Freq200HzMeasureBase(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    public class Oper71Freq2kHzMeasureBase : APPA_107N_109N.Oper71Freq2kHzMeasureBase
    {
        public Oper71Freq2kHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    public class Oper71Freq20kHzMeasureBase : APPA_107N_109N.Oper71Freq20kHzMeasureBase
    {
        public Oper71Freq20kHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    public class Oper71Freq200kHzMeasureBase : APPA_107N_109N.Oper71Freq200kHzMeasureBase
    {
        public Oper71Freq200kHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    public class Oper71Freq1MHzMeasureBase : APPA_107N_109N.Oper71Freq1MHzMeasureBase
    {
        public Oper71Freq1MHzMeasureBase(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    #endregion

    #region TEMP

    public class Oper10_1Temperature_Minus200_Minus100_Measure : APPA_107N_109N.Oper10_1Temperature_Minus200_Minus100_Measure
    {
        public Oper10_1Temperature_Minus200_Minus100_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.degC;
        }
    }

    public class Oper10_1Temperature_Minus100_400_Measure : APPA_107N_109N.Oper10_1Temperature_Minus100_400_Measure
    {
        public Oper10_1Temperature_Minus100_400_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.degC;
        }
    }

    public class Oper10_1Temperature_400_1200_Measure : APPA_107N_109N.Oper10_1Temperature_400_1200_Measure
    {
        public Oper10_1Temperature_400_1200_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa107N = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.degC;
        }
    }

    #endregion
}
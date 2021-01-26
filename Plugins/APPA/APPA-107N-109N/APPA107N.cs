using System;
using System.Reflection;
using APPA_107N_109N;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.APPA;

namespace Appa107N
{
    public class APPA107N : Appa107N109NBasePlugin<Operation>
    {
        public APPA107N(ServicePack servicePack) : base(servicePack)
        {
            Type = "APPA-107N";
            Range = "Пост. напр. 0 - 1000 В, пер. напр. 0 - 750 В,\n" +
                    " пост./пер. ток 0 - 10 А, изм. частоты до 1 МГц,\n" +
                    " эл. сопр. 0 - 2 ГОм, эл. ёмкость до 40 мФ.";
            Accuracy = "DCV 0.06%, ACV 1%, DCI 0.2%, ACI 1.2%,\n" +
                       " FREQ 0.01%, OHM 5%, FAR 1.5%";
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
            TestDevices = new IDeviceUi[]
                {new Device { Devices = new IDeviceRemote[] { new MultAPPA107N()}, Description = "Цифровой портативный мультиметр"}};
            var DcvMode = new Oper3DcvMeasureBaseMeasureAppa(this, Assembly.GetExecutingAssembly().GetName().Name);
            DcvMode.Nodes.Add(new DC_20mV_Measure(Mult107_109N.RangeNominal.Range20mV, this));
            DcvMode.Nodes.Add(new DC_200mV_Measure(Mult107_109N.RangeNominal.Range200mV, this));
            DcvMode.Nodes.Add(new DC_2V_Measure(Mult107_109N.RangeNominal.Range2V, this));
            DcvMode.Nodes.Add(new DC_20V_Measure(Mult107_109N.RangeNominal.Range20V, this));
            DcvMode.Nodes.Add(new DC_200V_Measure(Mult107_109N.RangeNominal.Range200V, this));
            DcvMode.Nodes.Add(new DC_1000V_Measure(Mult107_109N.RangeNominal.Range1000V, this));

            var AcvMode = new Oper4AcvMeasureBaseMeasureAppaAc(this, Assembly.GetExecutingAssembly().GetName().Name);
            AcvMode.Nodes.Add(new AcV_20mV_Measure(Mult107_109N.RangeNominal.Range20mV, this));
            AcvMode.Nodes.Add(new AcV_200mV_Measure(Mult107_109N.RangeNominal.Range200mV, this));
            AcvMode.Nodes.Add(new AcV_2V_Measure(Mult107_109N.RangeNominal.Range2V, this));
            AcvMode.Nodes.Add(new AcV_20V_Measure(Mult107_109N.RangeNominal.Range20V, this));
            AcvMode.Nodes.Add(new AcV_200V_Measure(Mult107_109N.RangeNominal.Range200V, this));
            AcvMode.Nodes.Add(new AcV750VMeasure(Mult107_109N.RangeNominal.Range750V, this));

            var FarMode = new Oper9FarMeasureBase(this);
            FarMode.Nodes.Add(new Far_4nF_Measure(Mult107_109N.RangeNominal.Range4nF, this));
            FarMode.Nodes.Add(new Far_40nF_Measure(Mult107_109N.RangeNominal.Range40nF, this));
            FarMode.Nodes.Add(new Far_400nF_Measure(Mult107_109N.RangeNominal.Range400nF, this));
            FarMode.Nodes.Add(new Far_4uF_Measure(Mult107_109N.RangeNominal.Range4uF, this));
            FarMode.Nodes.Add(new Far_40uF_Measure(Mult107_109N.RangeNominal.Range40uF, this));
            FarMode.Nodes.Add(new Far_400uF_Measure(Mult107_109N.RangeNominal.Range400uF, this));
            FarMode.Nodes.Add(new Far_4mF_Measure(Mult107_109N.RangeNominal.Range4mF, this));
            FarMode.Nodes.Add(new Far_40mF_Measure(Mult107_109N.RangeNominal.Range40mF, this));

            var OhmMode = new Oper8ResistanceMeasureBase(this, Assembly.GetExecutingAssembly().GetName().Name);
            OhmMode.Nodes.Add(new Resistance_200Ohm_Measure(Mult107_109N.RangeNominal.Range200Ohm, this));
            OhmMode.Nodes.Add(new Resistance_2kOhm_Measure(Mult107_109N.RangeNominal.Range2kOhm, this));
            OhmMode.Nodes.Add(new Resistance_20kOhm_Measure(Mult107_109N.RangeNominal.Range20kOhm, this));
            OhmMode.Nodes.Add(new Resistance_200kOhm_Measure(Mult107_109N.RangeNominal.Range200kOhm, this));
            OhmMode.Nodes.Add(new Resistance_2MOhm_Measure(Mult107_109N.RangeNominal.Range2Mohm, this));
            OhmMode.Nodes.Add(new Resistance_20MOhm_Measure(Mult107_109N.RangeNominal.Range20Mohm, this));
            OhmMode.Nodes.Add(new Resistance_200MOhm_Measure(Mult107_109N.RangeNominal.Range200Mohm, this));
            OhmMode.Nodes.Add(new Resistance_2GOhm_Measure(Mult107_109N.RangeNominal.Range2Gohm, this));

            var DciMode = new Oper5DciMeasureBaseMeasureAppa(this, Assembly.GetExecutingAssembly().GetName().Name);
            DciMode.Nodes.Add(new Dci_20mA_Measure(Mult107_109N.RangeNominal.Range20mA, this));
            DciMode.Nodes.Add(new Dci_200mA_Measure(Mult107_109N.RangeNominal.Range200mA, this));
            DciMode.Nodes.Add(new Dci_2A_Measure(Mult107_109N.RangeNominal.Range2A, this));
            DciMode.Nodes.Add(new Dci_10A_Measure(Mult107_109N.RangeNominal.Range10A, this));
            DciMode.Nodes.Add(new Dci_10A_Measure2(Mult107_109N.RangeNominal.Range10A, this));

            var AciMode = new Oper6AciMeasureBaseMeasureAppaAc(this, Assembly.GetExecutingAssembly().GetName().Name);
            AciMode.Nodes.Add(new Aci_20mA_Measure(Mult107_109N.RangeNominal.Range20mA, this));
            AciMode.Nodes.Add(new Aci_200mA_Measure(Mult107_109N.RangeNominal.Range200mA, this));
            AciMode.Nodes.Add(new Aci_2A_Measure(Mult107_109N.RangeNominal.Range2A, this));
            AciMode.Nodes.Add(new Aci_10A_Measure(Mult107_109N.RangeNominal.Range10A,this));
            AciMode.Nodes.Add(new Aci_10A_Measure2(Mult107_109N.RangeNominal.Range10A,this));

            var FreqMode = new Oper7FreqMeasureBaseMeasureAppa(this, Assembly.GetExecutingAssembly().GetName().Name);
            FreqMode.Nodes.Add(new Freq20HzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal.Range20Hz,this));
            FreqMode.Nodes.Add(new Freq200HzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal.Range200Hz,this));
            FreqMode.Nodes.Add(new Freq2KHzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal.Range2kHz,this));
            FreqMode.Nodes.Add(new Freq20KHzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal.Range20kHz,this));
            FreqMode.Nodes.Add(new Freq200KHzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal.Range200kHz,this));
            FreqMode.Nodes.Add(new Freq1MHzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal.Range1MHz,this));

            var TempMode = new Oper10TemperatureMeasureBase(this, Assembly.GetExecutingAssembly().GetName().Name);
            TempMode.Nodes.Add(new Temperature_Minus200_Minus100_Measure(Mult107_109N.RangeNominal.Range400degC,this));
            TempMode.Nodes.Add(new Temperature_Minus100_400_Measure(Mult107_109N.RangeNominal.Range400degC,this));
            TempMode.Nodes.Add(new Temperature_400_1200_Measure(Mult107_109N.RangeNominal.Range1200degC,this));
            
            

                UserItemOperation = new IUserItemOperationBase[]
                {
                    new Oper1VisualTest(this),
                    new Oper2Oprobovanie(this),
                    DcvMode,
                    AcvMode, 
                    OhmMode,
                    FarMode,
                    FreqMode,
                    DciMode,
                    AciMode,
                    TempMode
                    
                };
        }

        #region Methods

        

       

        #endregion
    }
   



    #region DCV

    public class DC_20mV_Measure : APPA_107N_109N.OpertionFirsVerf.Oper3_1DC_20mV_Measure
    {
        public DC_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCmV;
        }
    }

    public class DC_200mV_Measure : APPA_107N_109N.OpertionFirsVerf.Oper3_1DC_200mV_Measure
    {
        public DC_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCmV;
        }
    }

    public class DC_2V_Measure : APPA_107N_109N.OpertionFirsVerf.Oper3_1DC_2V_Measure
    {
        public DC_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCV;
        }
    }

    public class DC_20V_Measure : APPA_107N_109N.OpertionFirsVerf.Oper3_1DC_20V_Measure
    {
        public DC_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCV;
        }
    }

    public class DC_200V_Measure : APPA_107N_109N.OpertionFirsVerf.Oper3_1DC_200V_Measure
    {
        public DC_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCV;
        }
    }

    public class DC_1000V_Measure : APPA_107N_109N.OpertionFirsVerf.Oper3_1DC_1000V_Measure
    {
        public DC_1000V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.DCV;
        }
    }

    #endregion DCV

    #region ACV

    public class AcV_20mV_Measure : APPA_107N_109N.OpertionFirsVerf.Ope4_1_AcV_20mV_Measure
    {
        public AcV_20mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACmV;
        }
    }

    public class AcV_200mV_Measure : APPA_107N_109N.OpertionFirsVerf.Ope4_1_AcV_200mV_Measure
    {
        public AcV_200mV_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACmV;
        }
    }

    public class AcV_2V_Measure : APPA_107N_109N.OpertionFirsVerf.Ope4_1_AcV_2V_Measure
    {
        public AcV_2V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
        }
    }

    public class AcV_20V_Measure : APPA_107N_109N.OpertionFirsVerf.Ope4_1_AcV_20V_Measure
    {
        public AcV_20V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
        }
    }

    public class AcV_200V_Measure : APPA_107N_109N.OpertionFirsVerf.Ope4_1_AcV_200V_Measure
    {
        public AcV_200V_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
        }
    }

    public class AcV750VMeasure : APPA_107N_109N.OpertionFirsVerf.Ope4_1_AcV_750V_Measure
    {
        public AcV750VMeasure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.ACV;
        }
    }

    #endregion ACV

    #region OHM

    public class Resistance_200Ohm_Measure : APPA_107N_109N.OpertionFirsVerf.Oper8_1Resistance_200Ohm_Measure
    {
        public Resistance_200Ohm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Resistance_2kOhm_Measure : APPA_107N_109N.OpertionFirsVerf.Oper8_1Resistance_2kOhm_Measure
    {
        public Resistance_2kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Resistance_20kOhm_Measure : APPA_107N_109N.OpertionFirsVerf.Oper8_1Resistance_20kOhm_Measure
    {
        public Resistance_20kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Resistance_200kOhm_Measure : APPA_107N_109N.OpertionFirsVerf.Oper8_1Resistance_200kOhm_Measure
    {
        public Resistance_200kOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Resistance_2MOhm_Measure : APPA_107N_109N.OpertionFirsVerf.Oper8_1Resistance_2MOhm_Measure
    {
        public Resistance_2MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Resistance_20MOhm_Measure : APPA_107N_109N.OpertionFirsVerf.Oper8_1Resistance_20MOhm_Measure
    {
        public Resistance_20MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Resistance_200MOhm_Measure : APPA_107N_109N.OpertionFirsVerf.Oper8_1Resistance_200MOhm_Measure
    {
        public Resistance_200MOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    public class Resistance_2GOhm_Measure : APPA_107N_109N.OpertionFirsVerf.Oper8_1Resistance_2GOhm_Measure
    {
        public Resistance_2GOhm_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Ohm;
        }
    }

    #endregion OHM

    #region FAR

    public class Far_4nF_Measure : APPA_107N_109N.OpertionFirsVerf.Oper9_1Far_4nF_Measure
    {
        public Far_4nF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Far_40nF_Measure : APPA_107N_109N.OpertionFirsVerf.Oper9_1Far_40nF_Measure
    {
        public Far_40nF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Far_400nF_Measure : APPA_107N_109N.OpertionFirsVerf.Oper9_1Far_400nF_Measure
    {
        public Far_400nF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Far_4uF_Measure : APPA_107N_109N.OpertionFirsVerf.Oper9_1Far_4uF_Measure
    {
        public Far_4uF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Far_40uF_Measure : APPA_107N_109N.OpertionFirsVerf.Oper9_1Far_40uF_Measure
    {
        public Far_40uF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Far_400uF_Measure : APPA_107N_109N.OpertionFirsVerf.Oper9_1Far_400uF_Measure
    {
        public Far_400uF_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Far_4mF_Measure : APPA_107N_109N.OpertionFirsVerf.Oper9_1Far_4mF_Measure
    {
        public Far_4mF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    public class Far_40mF_Measure : APPA_107N_109N.OpertionFirsVerf.Oper9_1Far_40mF_Measure
    {
        public Far_40mF_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Cap;
        }
    }

    #endregion FAR

    #region DCI

    public class Dci_20mA_Measure : APPA_107N_109N.OpertionFirsVerf.Oper5_1Dci_20mA_Measure
    {
        public Dci_20mA_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Dci_200mA_Measure : APPA_107N_109N.OpertionFirsVerf.Oper5_1Dci_200mA_Measure
    {
        public Dci_200mA_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
           
        }
    }

    public class Dci_2A_Measure : APPA_107N_109N.OpertionFirsVerf.Oper5_1Dci_2A_Measure
    {
        public Dci_2A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Dci_10A_Measure : APPA_107N_109N.OpertionFirsVerf.Oper5_2_1Dci_10A_Measure
    {
        public Dci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
           
        }
    }

    public class Dci_10A_Measure2 : APPA_107N_109N.OpertionFirsVerf.Oper5_2_2Dci_10A_Measure
    {
        public Dci_10A_Measure2(Mult107_109N.RangeNominal inRangeNominal,
            IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    #endregion DCI

    #region ACI

    public class Aci_20mA_Measure : APPA_107N_109N.OpertionFirsVerf.Oper6_1Aci_20mA_Measure
    {
        
        public Aci_20mA_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
            
        }
    }

    public class Aci_200mA_Measure : APPA_107N_109N.OpertionFirsVerf.Oper6_1Aci_200mA_Measure
    
    {
        public Aci_200mA_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Aci_2A_Measure : APPA_107N_109N.OpertionFirsVerf.Oper6_1Aci_2A_Measure
    {
        public Aci_2A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Aci_10A_Measure : APPA_107N_109N.OpertionFirsVerf.Oper6_2_1Aci_10A_Measure
    {
        public Aci_10A_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }

    public class Aci_10A_Measure2 : APPA_107N_109N.OpertionFirsVerf.Oper6_2_2Aci_10A_Measure
    {
        public Aci_10A_Measure2(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            
        }
    }


    #endregion

    #region Freq

    public class Freq20HzMeasureBaseMeasureAppa : APPA_107N_109N.OpertionFirsVerf.Oper71Freq20HzMeasureBaseMeasureAppa
    {
        public Freq20HzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    public class Freq200HzMeasureBaseMeasureAppa : APPA_107N_109N.OpertionFirsVerf.Oper71Freq200HzMeasureBaseMeasureAppa
    {
        public Freq200HzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    public class Freq2KHzMeasureBaseMeasureAppa : APPA_107N_109N.OpertionFirsVerf.Oper71Freq2KHzMeasureBaseMeasureAppa
    {
        public Freq2KHzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    public class Freq20KHzMeasureBaseMeasureAppa : APPA_107N_109N.OpertionFirsVerf.Oper71Freq20KHzMeasureBaseMeasureAppa
    {
        public Freq20KHzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    public class Freq200KHzMeasureBaseMeasureAppa : APPA_107N_109N.OpertionFirsVerf.Oper71Freq200KHzMeasureBaseMeasureAppa
    {
        public Freq200KHzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    public class Freq1MHzMeasureBaseMeasureAppa : APPA_107N_109N.OpertionFirsVerf.Oper71Freq1MHzMeasureBaseMeasureAppa
    {
        public Freq1MHzMeasureBaseMeasureAppa(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.Herz;
        }

    }

    #endregion

    #region TEMP

    public class Temperature_Minus200_Minus100_Measure : APPA_107N_109N.OpertionFirsVerf.Oper10_1Temperature_Minus200_Minus100_Measure
    {
        public Temperature_Minus200_Minus100_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.degC;
        }
    }

    public class Temperature_Minus100_400_Measure : APPA_107N_109N.OpertionFirsVerf.Oper10_1Temperature_Minus100_400_Measure
    {
        public Temperature_Minus100_400_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.degC;
        }
    }

    public class Temperature_400_1200_Measure : APPA_107N_109N.OpertionFirsVerf.Oper10_1Temperature_400_1200_Measure
    {
        public Temperature_400_1200_Measure(Mult107_109N.RangeNominal inRangeNominal, IUserItemOperation userItemOperation) :
            base(inRangeNominal, userItemOperation, Assembly.GetExecutingAssembly().GetName().Name)
        {
            appa10XN = new MultAPPA107N();
            flkCalib5522A = new Calib5522A();
            OperMeasureMode = Mult107_109N.MeasureMode.degC;
        }
    }

    #endregion
}
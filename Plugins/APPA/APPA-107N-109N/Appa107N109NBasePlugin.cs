﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.Math;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Port.APPA;
using DevExpress.Mvvm;
using NLog;

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
            ControlDevices = new IDevice[]
                {new Device {Name = new[] {"5522A"}, Description = "Многофунциональный калибратор"}};
            TestDevices = new IDevice[]
                {new Device {Name = new[] {"Appa-107N"}, Description = "Цифровой портативный мультиметр"}};

            Accessories = new[]
            {
                "Интерфейсный кабель для клибратора (GPIB или COM порт)",
                "Кабель banana - banana 2 шт.",
                "Интерфейсный кабель для прибора APPA-107N/APPA-109N USB-COM инфракрасный."
            };

            DocumentName = "appa";
        }

        #region Methods

        public override void FindDivice()
        {
            throw new NotImplementedException();
        }

        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        #endregion
    }

    public abstract class Oper1VisualTest : ParagraphBase, IUserItemOperation<bool>
    {
        public Oper1VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

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

        #endregion

        public List<IBasicOperation<bool>> DataRow { get; set; }

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
        public Oper2Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

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

        #endregion

        public List<IBasicOperation<bool>> DataRow { get; set; }

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)

        {
            var bo = new BasicOperation<bool> {Expected = true};
            //bo.IsGood = s => bo.Getting;

            DataRow.Add(bo);
        }
    }



    //////////////////////////////******DCV*******///////////////////////////////

    #region DCV

    public  class Oper3DcvMeasureBase : ParagraphBase
    {
        //базовые точки
        public  readonly decimal[] basePoint =
            {(decimal) 0.004, (decimal) 0.008, (decimal) 0.012, (decimal) 0.016, (decimal) 0.018, (decimal) -0.018};
        //множители для пределов
        public  readonly decimal[] baseMultipliers = { 1, 10, 100, 1000, 10000 };
        //конкретные точки для последнего предела измерения 1000 В
         public  readonly decimal[] dopPoint1000V = { 100, 200, 400, 700, 900, -900 };
         

             public Oper3DcvMeasureBase()
             {
                Name = "Определение погрешности измерения постоянного напряжения";
             }

             protected override DataTable FillData()
             {
                 return null;
             }

             protected override void InitWork()
             {
                 return;
             }

             public override async Task StartSinglWork(CancellationToken token, Guid guid)
             {
                return;
             }

             public override async Task StartWork(CancellationToken token)
             {
                 return;
             }
    }

    public class Oper3_1DC_2V_Measure : ParagraphBase, IUserItemOperationBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //контрлируемый прибор
        protected Mult107_109N appa107N;
        //эталон
        protected Calib5522A flkCalib5522A;
    }

    public class Oper3_1DC_20V_Measure : ParagraphBase, IUserItemOperationBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //контрлируемый прибор
        protected Mult107_109N appa107N;
        //эталон
        protected Calib5522A flkCalib5522A;
    }

    public class Oper3_1DC_200V_Measure : ParagraphBase, IUserItemOperationBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //контрлируемый прибор
        protected Mult107_109N appa107N;
        //эталон
        protected Calib5522A flkCalib5522A;
    }

    public class Oper3_1DC_1000V_Measure : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //контрлируемый прибор
        protected Mult107_109N appa107N;
        //эталон
        protected Calib5522A flkCalib5522A;

        public Oper3_1DC_1000V_Measure()
        {
            Name = "1000 В";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork()
        {
            DataRow.Clear();
            var par = Parent as Oper3DcvMeasureBase;
            foreach (decimal currPoint in par.dopPoint1000V  )
            {

                var operation = new BasicOperationVerefication<decimal>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        appa107N.StringConnection = GetStringConnect(appa107N);
                        flkCalib5522A.StringConnection = GetStringConnect(flkCalib5522A);

                        flkCalib5522A.Out.SetOutput(Calib5522A.COut.State.Off);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                };

                operation.BodyWork = () =>
                {
                    try
                    {
                        flkCalib5522A.Out.Set.Voltage.Dc.SetValue(currPoint);
                        decimal measurePoint = (decimal)appa107N.GetValue();

                        operation.Getting = measurePoint;
                        operation.Expected = currPoint;
                        operation.ErrorCalculation = (decimal inA, decimal inB) => (decimal)0.0006 * currPoint + (decimal)(10 * 0.1);
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                 (operation.Getting > operation.LowerTolerance);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };
                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer = this.UserItemOperation.ServicePack.MessageBox.Show(operation + $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                                        "Повторить измерение этой точки?",
                                                                                        "Информация по текущему измерению", MessageButton.YesNo, MessageIcon.Question, MessageResult.Yes) ;

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };


            }
        }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
        }
    }

    public  class Oper3_1DC_20mV_Measure : ParagraphBase, IUserItemOperationBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //контрлируемый прибор
        protected Mult107_109N appa107N;
        //эталон
        protected Calib5522A flkCalib5522A;
        private string range = "200 мВ";
        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork()
        {
            DataRow.Clear();
        }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
        }
    }

    public  class Oper3_1DC_200mV_Measure : ParagraphBase, IUserItemOperationBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //контрлируемый прибор
        protected Mult107_109N appa107N;
        //эталон
        protected Calib5522A flkCalib5522A;


        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork()
        {
            throw new NotImplementedException();
        }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }

    internal static class ShemeTemplate
    {
        public static readonly ShemeImage TemplateSheme;

        static ShemeTemplate()
        {
            TemplateSheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 1,
                FileName = @"APPA107N_109N_5522A_DCV.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }
    }

    #endregion





    //////////////////////////////******ACV*******///////////////////////////////

    #region ACV

    public abstract class Oper4AcvMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper4AcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)

        {
            throw new NotImplementedException();
        }
    }

    #endregion

    //////////////////////////////******DCI*******///////////////////////////////

    #region DCI

    public abstract class Oper5DcIMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper5DcIMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    //////////////////////////////******ACI*******///////////////////////////////

    #region ACI

    public abstract class Oper6AcIMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper6AcIMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    //////////////////////////////******FREQ*******///////////////////////////////

    #region FREQ

     public abstract class Oper7FreqMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper7FreqMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    #endregion


    //////////////////////////////******OHM*******///////////////////////////////

    #region OHM

 public abstract class Oper8OhmMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper8OhmMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    #endregion


    //////////////////////////////******FAR*******///////////////////////////////

    #region FAR

    public abstract class Oper9FarMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper9FarMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    #endregion


    //////////////////////////////******TEMP*******///////////////////////////////

    #region TEMP

    public abstract class Oper10TemperatureMeasure : ParagraphBase, IUserItemOperationBase
    {
        public Oper10TemperatureMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
        }

        #region Methods

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override Task StartSinglWork(CancellationToken token, Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken cancellationToken)

        {
            throw new NotImplementedException();
        }
    }

    #endregion
    
}
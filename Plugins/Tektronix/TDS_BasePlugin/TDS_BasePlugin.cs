using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AP.Utils.Data;
using AP.Utils.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope;
using NLog;

namespace TDS_BasePlugin
{
    public class TDS_BasePlugin<T>: Program<T> where  T: OperationMetrControlBase

    {
        public TDS_BasePlugin(ServicePack service) : base(service)
        {
            Grsi = "32618-06";
        }
    }

    public class Operation : OperationMetrControlBase
    {
        public Operation()
        {
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public abstract class OpertionFirsVerf : ASMC.Core.Model.Operation
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            ControlDevices = new IDeviceUi[] { new Device { Devices = new IDeviceBase[] { new Calibr9500B() }, Description = "Калибратор осциллографов" } };
            Accessories = new[]
            {
                "Интерфейсный кабель для клибратора (GPIB)"

            };

            DocumentName = "32618-06 TDS";
        }

        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }
    }

    public class Oper1VisualTest : ParagraphBase, IUserItemOperation<bool>
    {
        public Oper1VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = new DataTable { TableName = "ITBmVisualTest" };

            data.Columns.Add("Результат внешнего осмотра");
            var dataRow = data.NewRow();
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperation<bool>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Getting ? "Соответствует" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        protected override void InitWork()
        {
            DataRow.Clear();
            var operation = new BasicOperation<bool>();
            operation.Expected = true;
            operation.IsGood = () => Equals(operation.Getting, operation.Expected);
            operation.InitWork = () =>
            {
                var service = UserItemOperation.ServicePack.QuestionText;
                service.Title = "Внешний осмотр";
                service.Entity = new Tuple<string, Assembly>("TDS_VisualTest", null);
                service.Show();
                var res = service.Entity as Tuple<string, bool>;
                operation.Getting = res.Item2;
                operation.Comment = res.Item1;
                operation.IsGood = () => operation.Getting;

                return Task.CompletedTask;
            };

            operation.CompliteWork = () => { return Task.FromResult(true); };
            DataRow.Add(operation);
        }

        #endregion

        public List<IBasicOperation<bool>> DataRow { get; set; }
    }

    public class Oper2Oprobovanie : ParagraphBase, IUserItemOperation<bool>
    {
        public Oper2Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var data = new DataTable { TableName = "ITBmOprobovanie" };

            data.Columns.Add("Результат опробования");
            var dataRow = data.NewRow();
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperation<bool>;
                dataRow[0] = dds.Getting ? "Соответствует" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        protected override void InitWork()
        {
            DataRow.Clear();
            var operation = new BasicOperation<bool>();
            operation.Expected = true;
            operation.IsGood = () => Equals(operation.Getting, operation.Expected);
            operation.InitWork = () =>
            {
                var service = UserItemOperation.ServicePack.QuestionText;
                service.Title = "Опробование";
                service.Entity = new Tuple<string, Assembly>("TDS_Oprobovanie", null);
                service.Show();
                var res = service.Entity as Tuple<string, bool>;
                operation.Getting = res.Item2;
                operation.Comment = res.Item1;
                operation.IsGood = () => operation.Getting;

                return Task.CompletedTask;
            };

            operation.CompliteWork = () => { return Task.FromResult(true); };
            DataRow.Add(operation);
        }

        #endregion

        public List<IBasicOperation<bool>> DataRow { get; set; }
    }


    /*Определение погрешности коэффициентов отклонения.*/

    public abstract class Oper3KoefOtkl : ParagraphBase, IUserItemOperation<MeasPoint>
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public List<IBasicOperation<MeasPoint>> DataRow { get; set; }
        private Calibr9500B calibr9500B;
        private TDS_Oscilloscope someTdsOscilloscope;
        /// <summary>
        /// тестируемый канал.
        /// </summary>
        private TDS_Oscilloscope.ChanelSet TestingChanel;

        protected Oper3KoefOtkl(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet inTestingChanel) : base(userItemOperation)
        {
            Name = "Определение погрешности коэффициентов отклонения";
            DataRow = new List<IBasicOperation<MeasPoint>>();
            TestingChanel = inTestingChanel;
            //подключение к каналу это наверное можно сделать через сообщение
            // ли для каждого канала картинку нарисовать TemplateSheme

        }

        protected override void InitWork()
        {
            DataRow.Clear();
            foreach (TDS_Oscilloscope.VerticalScale currScale in Enum.GetValues(typeof(TDS_Oscilloscope.VerticalScale)))
            {
                var operation = new BasicOperationVerefication<MeasPoint>();
                operation.InitWork = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            calibr9500B.StringConnection = GetStringConnect(calibr9500B);
                            someTdsOscilloscope.StringConnection = GetStringConnect(someTdsOscilloscope);
                        });

                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };

                operation.BodyWork = () =>
                {
                    try
                    {

                        //1.нужно знать канал
                        someTdsOscilloscope.Chanel.SetChanelState(TestingChanel, TDS_Oscilloscope.State.ON);
                        //теперь нужно понять с каким каналом мы будем работать на калибраторе
                        var chnael = calibr9500B.FindActiveHeadOnChanel(new ActiveHead9510()).FirstOrDefault();
                        calibr9500B.Route.Chanel.SetChanel(chnael);
                        //2.установить развертку по вертикали
                        someTdsOscilloscope.Chanel.Vertical.SetSCAle(TestingChanel, currScale);
                        //3.установить развертку по времени
                        someTdsOscilloscope.Horizontal.SetHorizontalScale(TDS_Oscilloscope
                                                                         .HorizontalSCAle.Scal_500mkSec);
                        
                        //4.установить усреднение
                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope.MiscellaneousMode.SAMple); //так быстрее будет
                        //5.подать сигнал: меандр 1 кГц
                        operation.Expected = new MeasPoint(MeasureUnits.V);
                        calibr9500B.Source.SetFunc(Calibr9500B.Shap.SQU).
                                    Source.SetVoltage().
                                    Source.SetFreq(1, UnitMultipliers.Kilo);
                        calibr9500B.Source.Output(Calibr9500B.State.On);
                        //6.снять показания с осциллографа
                        
                        someTdsOscilloscope.Measurement.SetMeas(TestingChanel, TDS_Oscilloscope.TypeMeas.PK2,1);
                        someTdsOscilloscope.Acquire.SetDataCollection(TDS_Oscilloscope.MiscellaneousMode.AVErage);
                        someTdsOscilloscope.Measurement.MeasureValue(1);

                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    finally
                    {
                        calibr9500B.Source.Output(Calibr9500B.State.Off);
                        someTdsOscilloscope.Chanel.SetChanelState(TestingChanel, TDS_Oscilloscope.State.OFF);
                    }

                   


                    
                };
            }
        }



    }



    /*Определение погрешности измерения временных интервалов.*/
    /*Определение Времени нарастания переходной характеристики.*/
}

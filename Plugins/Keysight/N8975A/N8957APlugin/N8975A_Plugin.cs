using AP.Utils.Data;
using AP.Utils.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.Generator;
using ASMC.Devices.IEEE.Keysight.NoiseFigureAnalyzer;
using DevExpress.Mvvm;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace N8957APlugin
{
    public class N8975A_Plugin : Program<Operation>
    {
        public N8975A_Plugin(ServicePack service) : base(service)
        {
            Type = "N8975A";
            Grsi = "37178-08";
            Range = "no range";
            Accuracy = "none";
            Operation = new Operation(service);
        }
    }

    public class Operation : OperationMetrControlBase
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.

        public Operation(ServicePack servicePack)
        {
            UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePack);
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public class OpertionFirsVerf : ASMC.Core.Model.Operation

    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            DocumentName = "N8975A_8_3_4";

            ControlDevices = new IDeviceUi[]
                {new Device {Devices = new IDeviceBase[] {new E8257D()}, Description = "Генератор сигналов"}};
            TestDevices = new IDeviceUi[]
                {new Device {Devices = new IDeviceBase[] {new N8975A()}, Description = "Анализатор шума"}};

            //Необходимые аксесуары
            Accessories = new[]
            {
                "Стандарт частоты PENDULUM 6689",
                "Генератор сигналов Keysight/Agilent E8257D",
                "Преобразователь интерфесов National Instruments GPIB-USB",
                "Кабели:\n 1) для подключения стандарта частоты к генератору\n  2) для подключения выхода генератора ко входу анализатора шума"
            };

            UserItemOperation = new IUserItemOperationBase[] 
                {
                    new PrevSetup(this) , 
                    new Oper8_3_4FreqInSintezatorFrequency(this)
                };
        }

        #region Methods

        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        #endregion Methods
    }

    public class PrevSetup : ParagraphBase, IUserItemOperation<bool>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected E8257D e8257D;
        protected N8975A n8975;

        public PrevSetup(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Предварительная настройка";
            DataRow = new List<IBasicOperation<bool>>();
            e8257D = new E8257D();
            n8975 = new N8975A();
        }

        protected override DataTable FillData()
        {
            return null;
        }

        protected override void InitWork()
        {
            var operation = new BasicOperationVerefication<bool>();
            operation.InitWork =  () =>
            {
                try
                {
                   
                    
                        e8257D.StringConnection = GetStringConnect(e8257D);
                        n8975.StringConnection = GetStringConnect(n8975);
                    

                    //предварительная подготовка прибора к работе
                    n8975.WriteLine("*rst");
                    
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    throw;
                }

                var service = UserItemOperation.ServicePack.QuestionText;
                service.Title = "Внешний осмотр";
                service.Entity = new Tuple<string, Assembly>("N8975_PreSetup", null);
                service.Show();
                var res = service.Entity as Tuple<string, bool>;
                operation.Getting = res.Item2;
                operation.Comment = res.Item1;
                operation.IsGood = () => operation.Getting;
                

                n8975.WriteLine("CALibration:AUTO:state 1");
                n8975.WriteLine("CALibration:YTF");

                UserItemOperation.ServicePack.MessageBox
                                 .Show("Дождитесь окончания настройки, затем нажмите ОК!",
                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                       MessageResult.OK);


                return Task.CompletedTask;
            };
           

            operation.CompliteWork = () => { return Task.FromResult(true); };
            DataRow.Add(operation);
        }

        public List<IBasicOperation<bool>> DataRow { get; set; }
    }

    public class Oper8_3_4FreqInSintezatorFrequency : ParagraphBase, IUserItemOperation<MeasPoint>
    {
        private Dictionary<int, int> freqAndTolDictionary;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Property

        public List<IBasicOperation<MeasPoint>> DataRow { get; set; }

        protected E8257D e8257D;
        protected N8975A n8975;

        #endregion Property

        public Oper8_3_4FreqInSintezatorFrequency(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            e8257D = new E8257D();
            n8975 = new N8975A();
            Name = "Определение погрешности установки частоты внутреннего синтезатора";
            DataRow = new List<IBasicOperation<MeasPoint>>();
            //Забиваем словарь: частота - погрешность
            freqAndTolDictionary = new Dictionary<int, int>();
            //                       точка Мгц    кГц погрешность
            freqAndTolDictionary.Add(15, 100);
            freqAndTolDictionary.Add(495, 100);
            freqAndTolDictionary.Add(1495, 100);
            freqAndTolDictionary.Add(1995, 100);
            freqAndTolDictionary.Add(2995, 100);
            freqAndTolDictionary.Add(3995, 400);
            freqAndTolDictionary.Add(4995, 400);
            freqAndTolDictionary.Add(5995, 400);
            freqAndTolDictionary.Add(14995, 400);
            freqAndTolDictionary.Add(17995, 400);
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = "FillTabBmSintezatorFrequency" };
            dataTable.Columns.Add("fН");
            dataTable.Columns.Add("fЦ");
            dataTable.Columns.Add("Δf");
            dataTable.Columns.Add("Максимально допустимое значение Δf");
            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<MeasPoint>;
                if (dds == null) continue;
                dataRow[0] = dds?.Expected.Description;
                dataRow[1] = dds?.Getting.Description;
                var tol = new MeasPoint(dds.Expected.Units, dds.Expected.UnitMultipliersUnit,
                                        dds.Expected.Value - dds.Getting.Value);
                dataRow[2] = tol.Description;
                dataRow[3] = dds?.Error.Description;
            }

            return dataTable;
        }

        protected override void InitWork()
        {
            if (e8257D == null || n8975 == null) return;
            DataRow.Clear();
            foreach (int freq in freqAndTolDictionary.Keys)
            {
                var operation = new BasicOperationVerefication<MeasPoint>();

                operation.InitWork = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            e8257D.StringConnection = GetStringConnect(e8257D);
                            n8975.StringConnection = GetStringConnect(n8975);

                            double testFreqqPoint = (double)freq * UnitMultipliers.Mega.GetDoubleValue();
                            //подготовка генератора
                            e8257D.WriteLine(":OUTPut:MODulation 0"); //выключаем модуляцию
                            e8257D.WriteLine(":pow -20"); //ставим амплитуду -20 дБм
                            e8257D.WriteLine($"FREQuency {testFreqqPoint.ToString().Replace(',', '.')}"); //частоту
                            // e8257D.WriteLine("FREQuency:MODE "); //ставим режим воспроизведения частоты
                            //какие параметры сигнала еще должны быть?

                            n8975.WriteLine($"INITiate:CONTinuous 0");
                            n8975.WriteLine("CONFigure:MODE:DUT AMPLifier");
                            n8975.WriteLine("freq:mode swe");
                            n8975.WriteLine($"FREQuency:CENTer {testFreqqPoint.ToString().Replace(',', '.')}"); //центральная частота
                            if (freq == 15)
                                n8975.WriteLine($"FREQuency:span 10e6"); //обзор
                            else
                                n8975.WriteLine($"FREQuency:span 8e6"); //обзор

                            n8975.WriteLine($"AVERage:STATe 0");
                            n8975.WriteLine($"BANDwidth 4MHz");
                            n8975.WriteLine($"SWEep:POINts 201");  //сколько точек измерения

                            n8975.WriteLine("DISPlay:FORMat TABLe");//устнавливаем отображение таблицы
                            n8975.WriteLine("DISPlay:DATA:UNITs phot, lin");//задаем единицы измерения

                            e8257D.WriteLine(":OUTP 1");
                            Thread.Sleep(300);
                            // единичное измерение
                            n8975.WriteLine("INITiate");
                        });

                        
                        //n8975.Sinchronization();
                        //как то нужно от прибора получить ответ, что он закончил измерения
                        UserItemOperation.ServicePack.MessageBox
                                         .Show("Дождитесь окончания измерения, затем нажмите ОК!",
                                               "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                               MessageResult.OK);
                        string[] answerFreqList = n8975.QueryLine("FREQuency:LIST:DATA?").TrimEnd('\n').TrimEnd('0').TrimEnd('\0').Split(',');//считать частоты для коэффициентов
                        string[] answerPHot = n8975.QueryLine("FETC:UNC:PHOT? LIN").TrimEnd('\n').TrimEnd('0').TrimEnd('\0').Split(','); ;//считывание коэффициентов PHOT Lin
                        

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
                       
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    finally
                    {
                        e8257D.WriteLine(":OUTP 0");
                    }
                };
                operation.CompliteWork = () => Hepls.HelpsCompliteWork(operation, UserItemOperation);

                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<MeasPoint>)operation.Clone());
            }
        }

        #endregion Methods
    }

    public static class Hepls
    {
        #region Methods

        public static Task<bool> HelpsCompliteWork(BasicOperationVerefication<MeasPoint> operation,
            IUserItemOperation UserItemOperation)
        {
            if (!operation.IsGood())
            {
                var answer =
                    UserItemOperation.ServicePack.MessageBox
                                     .Show($"Текущая точка {operation.Expected.Description} не проходит по допуску:\n" +
                                           $"Минимально допустимое значение {operation.LowerTolerance.Description}\n" +
                                           $"Максимально допустимое значение {operation.UpperTolerance.Description}\n" +
                                           $"Допустимое значение погрешности {operation.Error.Description}\n" +
                                           $"ИЗМЕРЕННОЕ значение {operation.Getting.Description}\n\n" +
                                           $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected.Value - operation.Getting.Value}\n\n" +
                                           "Повторить измерение этой точки?",
                                           "Информация по текущему измерению",
                                           MessageButton.YesNo, MessageIcon.Question,
                                           MessageResult.Yes);

                if (answer == MessageResult.No) return Task.FromResult(true);
            }

            return Task.FromResult(operation.IsGood());
        }

        #endregion Methods
    }
}
using System;
using System.Linq;
using System.Threading;
using AP.Reports.Utils;

namespace ASMC.Devises.IEEE.Keysight.ElectronicLoad
{
    public abstract class Main_N3300 : Main
    {
       
        private const string FuncCurr = "CURR";
        private const string FuncVolt = "VOLT";
        private const string FuncRes = "RES";

        /// <summary>
        /// Номер канала, в котором установлен модуль нагрузки.
        /// </summary>
        protected int chanNum;

        //массивы с номиналами пределов модуля нагрузки
        //будет инициализироваться в конструкторе
        protected decimal[] rangeResistanceArr;
        protected decimal[] rangeVoltArr;
        protected decimal[] rangeCurrentArr;
        protected string ModuleModel;

        protected Main_N3300(int chanNum)
        {
            this.DeviseType = "N3300A";
            //закрепим номер канала за вставкой нагрузки
            this.chanNum = chanNum;
            
        }

        public enum VoltMultipliers
        {
          // [StringValue(" V")] Volt,
           [StringValue("")] SI
        }

        public enum CurrMultipliers
        {
            //[StringValue(" A")] Amp,
            [StringValue("")] SI
        }

        public enum ResistanceMultipliers
        {
            //[StringValue(" OHM")] Ohm,
            [StringValue("")] SI
        }

        public string GetModuleModel()
        {
            return this.ModuleModel;
        }


        /// <summary>
        /// Возвращает массив с моделями установленных вставок нагрузки
        /// </summary>
        /// <returns></returns>
        public string[] GetInstalledModulesName()
        {
            
            this.WriteLine("*RDT?");
            Thread.Sleep(300);
            string answer = this.ReadLine().TrimEnd('\n');
            
           return answer.Split(';');
        }

        /// <summary>
        /// Устанавливает рабочий канал нагрузки
        /// </summary>
        /// <param name="ChanNum">номер канала нагрузки - целое число</param>
        /// <returns></returns>
        public  bool SetWorkingChanel()
        {

            this.WriteLine("CHAN:LOAD " + chanNum);
            this.WriteLine("CHAN:LOAD?");
            int answer = int.Parse(this.ReadLine());

            return answer == chanNum ? true : false;
        }

        /// <summary>
        /// Возвращает номер канала модуля нагрузки
        /// </summary>
        /// <returns></returns>
        public int GetChanelNumb()
        {
            return this.chanNum;
        }


        /// <summary>
        /// Отвечает, какой текущий режим канала
        /// </summary>
        /// <returns></returns>
        public string AskMyFunc()
        {
            
            this.WriteLine("FUNC?");
            string answer = this.ReadLine();
            

            return answer;
        }

        /// <summary>
        /// Устанавливает режим CR на нагрузке
        /// </summary>
        /// <returns></returns>
        public bool SetResistanceFunc()
        {
            
            this.WriteLine("FUNC RES");
            

            return this.AskMyFunc().Equals(FuncRes);
        }

       

        /// <summary>
        /// Устанавливает режим CV на нагрузке
        /// </summary>
        /// <returns></returns>
        public bool SetVoltFunc()
        {
            
            this.WriteLine("FUNC Volt");
            

            return this.AskMyFunc().Equals(FuncVolt);
        }

        /// <summary>
        /// Устанавливает режим CC на нагрузке
        /// </summary>
        /// <returns></returns>
        public bool SetCurrFunc()
        {
            
            this.WriteLine("FUNC CURRent");
            

            return this.AskMyFunc().Equals(FuncCurr);

        }

        /// <summary>
        /// Задает ограничение по току на канале
        /// </summary>
        /// <param name="currLevelIn"></param>
        /// <returns></returns>
        public bool SetCurrLevel(decimal currLevelIn)
        {
            
            this.WriteLine("CURR:LEV " + currLevelIn);
            Thread.Sleep(700);
            //!!!!   доделать с прибором   !!!!!
            decimal currLevAnswer = this.GetCurrLevel();

            
            return currLevelIn == currLevAnswer? true: false;
        }

        /// <summary>
        /// Возвращает ограничение тока на канале
        /// </summary>
        /// <returns></returns>
        public decimal GetCurrLevel()
        {
            
             this.WriteLine("CURR:LEV?");
            string answer = this.ReadLine();
            

            string[] val = answer.TrimEnd('\n').Split('E');

            return decimal.Parse(val[0]) * (decimal)System.Math.Pow(10, double.Parse(val[1]));
        }

       
        /// <summary>
        /// Возвращает измеренное значение тока в цепи
        /// </summary>
        /// <returns></returns>

        public decimal GetMeasCurr()
        {
            Thread.Sleep(1000);
            this.WriteLine("MEASure:CURRent?");
            Thread.Sleep(1000);
            string answer = this.ReadLine();

            string[] val = answer.TrimEnd('\n').Split('E');

            return decimal.Parse(val[0]) * (decimal)System.Math.Pow(10, double.Parse(val[1]));
        }

        /// <summary>
        /// Возвращает измеренное значение напряжения на нагрузке
        /// </summary>
        /// <returns></returns>
        public decimal GetMeasVolt()
        {
            
            this.WriteLine("MEAS:VOLT?");
            Thread.Sleep(300);
            string answer = this.ReadLine();
            

            return Decimal.Parse(answer);
        }

        public decimal GetMeasPower()
        {
            
            this.WriteLine("MEASure:POWer?");
            Thread.Sleep(300);
            string answer = this.ReadLine();
            

            return Decimal.Parse(answer);
        }

        /// <summary>
        /// Устанавливает предел ИЗМЕРЯЕМОГО тока для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        /// <param name="inRange">Значение предела измерения тока</param>
        public  bool SetCurrMeasRange(decimal inRange)
        {
            decimal moduleMaxRange = rangeCurrentArr.Last();
            if (inRange > moduleMaxRange) throw new ArgumentOutOfRangeException();

            this.WriteLine("SENSe:CURR:RANGe " + inRange);
            return true;
        }

        /// <summary>
        /// Устанавливает предел ИЗМЕРЯЕМОГО напряжения для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        /// <param name="inRange">Значение предела измерения напряжения</param>
        public  bool SetVoltMeasRange(decimal inRange)
        {
            decimal moduleMaxRange = rangeVoltArr.Last();
            if (inRange > moduleMaxRange) throw new ArgumentOutOfRangeException();

            this.WriteLine("SENSe:VOLTage:RANGe " + inRange);
            return true;

        }

        /// <summary>
        /// Устанавливает ПРЕДЕЛ сопротивления для режима CR 
        /// </summary>
        /// <param name="inRange">Значение сопротивления, которое нужно установить</param>
        public bool SetResistanceRange(decimal inRange )
        {

            //узнаем максимальный предел для конкретного модуля
            decimal moduleMaxRange = rangeResistanceArr.Last();

                //если предел который хотим поставить больше чем может канал, то выбросим исключение
                if (inRange > moduleMaxRange) throw new ArgumentException();

                //выбираем предел измерения из списка для конкретного канала
            foreach (decimal rangeDestination in this.rangeResistanceArr)
            {
                if (inRange <= rangeDestination)
                {
                    this.WriteLine("RESistance:RANGe " + rangeDestination);
                    break;
                }
            }

            return true;          

        }

        /// <summary>
        /// Устанавливает ВЕЛИЧИНУ сопротивления для режима CR
        /// </summary>
        /// <param name="inResist">Величина сопротивления в Омах</param>
        public bool SetResistance(decimal inResist)
        {
            if (this.SetResistanceRange(inResist))
            { 
                this.WriteLine("RESistance " + inResist);
                return true;

            }
            return false;
        }
        


        /// <summary>
        /// Устанавливает МАКСИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО тока для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetCurrMaxMeasRange()
        {
            

            this.WriteLine("SENS:CURR:RANGE MAX");
            
            
        }

        /// <summary>
        /// Устанавливает МИНИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО тока для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetCurrMinMeasRange()
        {
           
            this.WriteLine("SENS:CURR:RANGE MIN");
        }

        /// <summary>
        /// Возвращает текущий предел измерения по току
        /// </summary>
        /// <returns></returns>
        public decimal GetCurrMeasRange()
        {
            
            this.WriteLine("SENS:CURR:RANGE?");
            Thread.Sleep(300);
            string answer = this.ReadLine();
            

            return Decimal.Parse(answer);
        }

        /// <summary>
        /// Ограничение напряжения на канале для режима CV
        /// </summary>
        /// <param name="inLevel"></param>
        public bool SetVoltLevel(decimal inLevel)
        {
            if (inLevel > rangeVoltArr.Last()) throw new ArgumentOutOfRangeException();
            
            //отправляем команду с уставкой по вольтам
            this.WriteLine("VOLT " + inLevel);
            Thread.Sleep(100);
            
            //удостоверимся, что значение принято
            this.WriteLine("VOLT?");
            string answer =this.ReadLine();
            
            //преобразуем строку в число
            string[] val = answer.TrimEnd('\n').Split('E');
            decimal resultVoltLevel = decimal.Parse(val[0]) * (decimal)System.Math.Pow(10, double.Parse(val[1]));

            //если значение установилось, то ответ прибора будет тем же числом, что мы отправили на прибор - тогда вернем true
            return resultVoltLevel == inLevel? true : false;
        }

        /// <summary>
        /// Устанавливает МАКСИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО напряжения для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetVoltMaxMeasRange()
        {
            this.WriteLine("SENS:VOLT:RANGE MAX");
        }

        /// <summary>
        /// Устанавливает МИНИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО напряжения для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetVoltMinMeasRange()
        {
            

            this.WriteLine("SENS:VOLT:RANGE MIN");

            
        }

        /// <summary>
        /// Включает выходы электронной нагрузки
        /// </summary>
        /// <returns></returns>
        public bool OnOutput()
        {
            
            this.WriteLine("outp 1");
            Thread.Sleep(200);
            this.WriteLine("outp?");
            int answer = int.Parse(this.ReadLine());
            

            return answer == 1 ? true : false;
        }


        /// <summary>
        /// Выключает выходы электронной нагрузки
        /// </summary>
        /// <returns></returns>
        public bool OffOutput()
        {
            
            this.WriteLine("outp 0");
            Thread.Sleep(200);
            this.WriteLine("outp?");
            int answer = int.Parse(this.ReadLine());
            

            return answer == 0 ? true : false;
        }

    }

   
}

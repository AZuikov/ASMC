﻿using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.Interface;
using System;
using System.Threading.Tasks;

namespace ASMC.Devices.Rohde_Schwarz
{
    public abstract class RohdeSchwarz_RTMOscilloscope : IeeeBase, IOscilloscope, IOscillMeasure
    {
        public IOscillChanel[] Chanels
        {
            get => GetChanels();
        }

        protected abstract IOscillChanel[] GetChanels();

        public void ChanelOn(int ChanelNumber)
        {
            ChanelNumber--;

            if (Chanels.Length >= ChanelNumber)
                throw new ArgumentException($"Осциллограф имеет {Chanels.Length} каналов. Нельзя получить доступ к несуществующему каналу {ChanelNumber}.");

            Chanels[ChanelNumber].IsEnable = true;
        }

        public void ChanelOff(int ChanelNumber)
        {
            ChanelNumber--;

            if (Chanels.Length >= ChanelNumber)
                throw new ArgumentException($"Осциллограф имеет {Chanels.Length} каналов. Нельзя получить доступ к несуществующему каналу {ChanelNumber}.");

            Chanels[ChanelNumber].IsEnable = false;
        }

        private static readonly Coupling[] couplings = new[] { Coupling.AC, Coupling.DC, Coupling.GND };
        private static readonly MeasPoint<Frequency>[] chanelBand = new MeasPoint<Frequency>[] { 
            new MeasPoint<Frequency>(20,UnitMultiplier.Mega), 
            new MeasPoint<Frequency>(200, UnitMultiplier.Mega),
            new MeasPoint<Frequency>(400,UnitMultiplier.Mega)
        };

        public class RohdeSchwarz_RtmOscChanel : IOscillChanel, IOscillBandWidth
        {
            public RohdeSchwarz_RtmOscChanel(int numberChanel, IeeeBase device)
            {
                Number = numberChanel;
                Device = device;
            }

            public IeeeBase Device { get; }
            public int Number { get; }
            public bool IsEnable { get; set; }
            public MeasPoint<Voltage> VerticalOffset { get; set; }
            public MeasPoint<Voltage> Vertical { get; set; }
            public MeasPoint<Resistance> Impedance { get; set; }
            public MeasPoint<Frequency> BandWidth { get; set; }
            public Coupling coupling { get; set; }
            public int Probe { get; set; }

            public void Getting()
            {
                string answer = Device.QueryLine($"CHANnel{Number}:STATe?");
                IsEnable = answer.Equals("1");

                answer = Device.QueryLine($"CHANnel{Number}:OFFSet?");
                decimal digitsValue = (decimal)StrToDoubleMindMind(answer);
                VerticalOffset = new MeasPoint<Voltage>(digitsValue);

                digitsValue = 0;
                answer = Device.QueryLine($"CHANnel{Number}:scale?");
                digitsValue = (decimal)StrToDoubleMindMind(answer);
                Vertical = new MeasPoint<Voltage>(digitsValue);

                answer = Device.QueryLine($"CHANnel{Number}:COUPling?");
                Impedance = answer.IndexOf('L') != -1 ? new MeasPoint<Resistance>(50) :
                    new MeasPoint<Resistance>(1, UnitMultiplier.Mega);

                foreach (Coupling coupl in couplings)
                {
                    if (answer.IndexOf(coupl.ToString()) != -1) coupling = coupl;
                    //наверное больше ничего прибор в этом случае ответить не сможет...
                }
                
                

                //с пробником(делителем) пока непонятно, не могу найти команду, которая позволяет считать его настройки
            }

            public void Setting()
            {
                int ChanStat = IsEnable ? 1 : 0;
                Device.WriteLine($"CHANnel{Number}:STATe {ChanStat}");

                string valueToWrite = Vertical.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.');
                Device.WriteLine($"CHANnel{Number}:scale {valueToWrite}");

                valueToWrite = VerticalOffset
                                     .MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.');
                Device.WriteLine($"CHANnel{Number}:OFFSet {valueToWrite}");

                valueToWrite = $"CHANnel{Number}:COUPling {coupling}";
                if (Impedance == new MeasPoint<Resistance>(1, UnitMultiplier.Mega))
                    valueToWrite = valueToWrite + "L";
                Device.WriteLine($"CHANnel{Number}:COUPling {valueToWrite}");
            }


            
        }

        
    }

    public class RTM2054 : RohdeSchwarz_RTMOscilloscope
    {
        protected override IOscillChanel[] GetChanels()
        {
            return new IOscillChanel[]
            {
                new RohdeSchwarz_RtmOscChanel(1,this),
                new RohdeSchwarz_RtmOscChanel(2,this),
                new RohdeSchwarz_RtmOscChanel(3,this),
                new RohdeSchwarz_RtmOscChanel(4,this)
            };
        }

        /// <summary>
        /// Считываем настройки каналов осциллографа.
        /// </summary>
        /// <returns></returns>
        public override Task InitializeAsync()
        {
            return Task.Factory.StartNew(() =>
             {
                 foreach (IOscillChanel chanel in Chanels)
                 {
                     chanel.Getting();
                 }
             });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.Interface;

namespace ASMC.Devices.Rohde_Schwarz
{
    public abstract  class RS_RTM_Oscilloscope: IeeeBase, IOscilloscope
    {
        
       public IChanel[] Chanels
       {
           get => GetChanels();
       }
       protected abstract IChanel[] GetChanels();

       public void ChanelOn(int ChanelNumber)
        {
            ChanelNumber--;

            if (Chanels.Length>= ChanelNumber)
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

        public class RS_RTM_Chanel : IChanel
        {

            public RS_RTM_Chanel(int numberChanel, IeeeBase device)
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
            public int Probe { get; set; }
            public void Getting()
            {
                Device.QueryLine("");
            }

            public void Setting()
            {
                Device.WriteLine("");
            }
        }

    }

    public class RTM2054 :RS_RTM_Oscilloscope
    {
        protected override IChanel[] GetChanels()
        {
            return new IChanel[]
            {
                new RS_RTM_Chanel(1,this),
                new RS_RTM_Chanel(2,this),
                new RS_RTM_Chanel(3,this),
                new RS_RTM_Chanel(4,this)
            } ;
        }
    }

   
}

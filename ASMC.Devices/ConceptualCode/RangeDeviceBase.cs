using System.Linq;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.Model
{
    public abstract class RangeDeviceBase<TPhysicalQuantity> : IRangePhysicalQuantity<TPhysicalQuantity> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        protected RangeDeviceBase()
        {
            Ranges = new RangeStorage<PhysicalRange<TPhysicalQuantity>>();
        }
        /// <inheritdoc />
        public virtual RangeStorage<PhysicalRange<TPhysicalQuantity>> Ranges { get;  set; }

        /// <inheritdoc />
        public PhysicalRange<TPhysicalQuantity> SelectRange { get; private set; }

        /// <inheritdoc />
        public void SetRange(PhysicalRange<TPhysicalQuantity> inRange)
        {
            SelectRange = Ranges.Ranges.FirstOrDefault(q=>q.Start== inRange.Start&& q.End == inRange.End);
        }

        /// <inheritdoc />
        public void SetRange(MeasPoint<TPhysicalQuantity> inRange)
        {
            foreach (PhysicalRange<TPhysicalQuantity> range in Ranges)
            {
                if (range.End.MainPhysicalQuantity.GetNoramalizeValueToSi() >=
                    inRange.MainPhysicalQuantity.GetNoramalizeValueToSi())
                {
                    SelectRange = range;
                    break;
                }
            }
        }

        /// <inheritdoc />
        public bool IsAutoRange { get; set; }
    }

    public abstract class RangeDeviceBase<TPhysicalQuantity, TPhysicalQuantity2> : 
        IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> 
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
        protected RangeDeviceBase()
        {
            Ranges = new RangeStorage<PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2>>();
        }
        /// <inheritdoc />
        public virtual RangeStorage<PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2>> Ranges { get; set; }

        /// <inheritdoc />
        public PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2> SelectRange { get; private set; }

        /// <inheritdoc />
        public void SetRange(PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2> inRange)
        {
            
            SelectRange = Ranges.Ranges.FirstOrDefault(q => 
                                                           q.Start.MainPhysicalQuantity.GetNoramalizeValueToSi() <= inRange.Start.MainPhysicalQuantity.GetNoramalizeValueToSi() &&
                                                            q.End.MainPhysicalQuantity.GetNoramalizeValueToSi() >= inRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi() && 
                                                            q.Start.AdditionalPhysicalQuantity.GetNoramalizeValueToSi() <= inRange.Start.AdditionalPhysicalQuantity.GetNoramalizeValueToSi() &&
                                                            q.End.AdditionalPhysicalQuantity.GetNoramalizeValueToSi() >= inRange.End.AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
            
        }

        /// <inheritdoc />
        public void SetRange(MeasPoint<TPhysicalQuantity, TPhysicalQuantity2> inRange)
        {
            foreach (PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2> range in Ranges)
            {
                if (range.End.MainPhysicalQuantity.GetNoramalizeValueToSi() >=
                    inRange.MainPhysicalQuantity.GetNoramalizeValueToSi() &&
                    inRange.AdditionalPhysicalQuantity.GetNoramalizeValueToSi()>= range.Start.AdditionalPhysicalQuantity.GetNoramalizeValueToSi()&&
                    inRange.AdditionalPhysicalQuantity.GetNoramalizeValueToSi() <= range.End.AdditionalPhysicalQuantity.GetNoramalizeValueToSi())
                {
                    SelectRange = range;
                    break;
                }
            }
        }
        /// <inheritdoc />
        public bool IsAutoRange { get; set; }
    }
}
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
        public RangeStorage<PhysicalRange<TPhysicalQuantity>> Ranges { get;  set; }

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
            SelectRange = Ranges.GetRangePointBelong(inRange);
        }

        /// <inheritdoc />
        public bool IsAutoRange { get; set; }
    }
}
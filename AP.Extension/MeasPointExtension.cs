using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;

namespace AP.Extension
{
    public static class MeasPointExtension
    {

        public static MeasPoint[] GetArayMeasPointsInParcent(this MeasPoint range, params int[] pointParcent)
        {
            var listPoint = new List<MeasPoint>();
            foreach (var countPoint in pointParcent)
            {
                if (countPoint>100 || countPoint<0)
                {
                    throw  new ArgumentOutOfRangeException(nameof(pointParcent),countPoint.ToString());
                }
                listPoint.Add(new MeasPoint(range.Units, range.UnitMultipliersUnit, range.Value * countPoint / 100));
            }
            return listPoint.ToArray();
        }
    }
}

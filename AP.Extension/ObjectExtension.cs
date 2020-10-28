using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;

namespace AP.Extension
{
    public static class ObjectExtension
    {
        public static void FillRangesDevice(this object Devise, string path)
        {
          var property =  Devise.GetType().GetProperties()
                .Where(q => Equals(q.CanWrite, true)).ToArray();
        }
    }
}

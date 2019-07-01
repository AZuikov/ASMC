using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronBarCode;

namespace AP.Reports
{
    public class Barcode
    {
        public Barcode()
        {
            IronBarCode.License.LicenseKey = "IRONBARCODE-MYLICENSE-KEY-1EF01";
            var dfsadsa= IronBarCode.BarcodeWriter. CreateBarcode("Щоце токе", BarcodeEncoding.Code128);
            dfsadsa.SaveAsJpeg(@"C:\Users\02tav01\Pictures\Ошибка связи1.JPG");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ASMC.Data.Model.Interface;
using Palsys.Data.Model.Report;
using Palsys.Utils.Data;

namespace ASMC.Data.Model.Asumc
{
    public class VerificationSi : InformationInCards
    {
        public override void FillDate(IDataProvider dp)
        {
            Cards = dp.Execute("[dbo].[up_gr_RepSettingSelect]", true,
                new SqlParameter("dll", SqlDbType.VarChar) { Value = "Reports_EKZ" },
                new SqlParameter("prdll", SqlDbType.NVarChar) { Value = 1 },
                new SqlParameter("idobj", SqlDbType.Int) { Value = Id });
            base.FillDate(dp);

            var sets = new MapperSetting().Map<RegCardMi>(Cards);
         
            RegNumber = sets.RegNumber; 
            CertificatNumber = sets.CertificatNumber;
            DateNextCertification = sets.DateNextCertification;
            DateCertification = sets.DateCertification;
            NumberReplaced = sets.NumberReplaced;
            WhatReplaced = sets.WhatReplaced;
            DecommissioningDate = sets.DecommissioningDate;
            DecommissioningReason = sets.DecommissioningReason;
            AddditionalInformation = new RichTextBox { Rtf = sets.AddditionalInformation }.Text;
            
        }

        public override List<string> Checked()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text;
using Palsys.Data.Model.Metr;
using Palsys.Utils.Data;

namespace ASMC.Data.Model.Asumc
{
    public abstract class InformationInCards : Ekz
    {

        /// <summary>
        /// Возвращает или задает дату МК.
        /// </summary>
        [Column("DTMKFK", TypeName = "datetime")]
        public DateTime? Date
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает дату приемки.
        /// </summary>
        [Column("DTPRM", TypeName = "datetime")]
        public DateTime? DateOfAcceptance
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает плановую дату МК.
        /// </summary>
        [TableCell("Плановая дата аттестации")]
        public DateTime? DateOfPlanned
        {
            get; set;
        }
        public virtual void FillDate(IDataProvider dp)
        {
            ArrRanges = FillIsDate(ReferensceRanges);
            ArrErrors = FillIsDate(ReferenceError);
            Ranges = DataAssembly(ArrRanges);
            Errors = DataAssembly(ArrErrors);
        }

        private static readonly string[] ReferensceRanges =
        {
        "Cell227", "Cell231", "Cell235", "Cell239", "Cell243", "Cell247", "Cell251", "Cell364", "Cell368", "Cell372",
        "Cell376", "Cell380", "Cell384", "Cell388"
    };

        private static readonly string[] ReferenceError =
        {
        "Cell229", "Cell233", "Cell237", "Cell241", "Cell245", "Cell249", "Cell253", "Cell366", "Cell370", "Cell374",
        "Cell378", "Cell382", "Cell386", "Cell390"
    };

        public abstract List<string> Checked();
        /// <summary>
        ///  Дата атестации 
        /// </summary>
        [TableCell("Дата атестации ")]
        public DateTime? DateCertification
        {
            get;
            protected set;
        }

        /// <summary>
        ///  Дата следующей атестации 
        /// </summary>
        [TableCell("Дата следующей атестации")]
        public DateTime? DateNextCertification
        {
            get;
            protected set;
        }

        /// <summary>
        /// № Сведетелдьства
        /// </summary>
        [TableCell("№ Сведетелдьства")]
        public string CertificatNumber
        {
            get;
            protected set;
        }

        /// <summary>
        /// Регистрационный номер
        /// </summary>
        [TableCell("Рег. номер")]
        public string RegNumber
        {
            get;
            protected set;
        }  

        public string[] ArrErrors
        {
            get;
            protected set;
        }

        public string[] ArrRanges
        {
            get;
            protected set;
        }

        public string RangeErrorsToString()
        {
            var str = new StringBuilder();
            for(var i = 0; i < ArrErrors.Length; i++)
            {
                if(ArrRanges[i].Length > 1 || ArrErrors[i].Length > 1)
                {
                    str.AppendLine($@"{ArrRanges[i]} {ArrErrors}");
                }
            }

            return str.ToString().TrimEnd(' ');
        }

        private string[] FillIsDate(string[] array)
        {
            var arr = new List<string>();
            for(var i = 0; i < Cards.Rows.Count; i++)
            {
                foreach(var value in array)
                {
                    if(Cards.Rows[i][0].ToString() != value)
                        continue;
                    arr.Add(Cards.Rows[i][1].ToString().Trim());
                    break;
                }
            }

            return arr.ToArray();
        }

        private string DataAssembly(IEnumerable<string> array)
        {
            var str = new StringBuilder();
            foreach(var unit in array)
            {
                if(!string.IsNullOrEmpty(unit))
                {
                    str.AppendLine(unit + ",");
                }
                else
                {
                    str.AppendLine();
                }
            }

            return str.ToString().TrimEnd(',', '\n', '\r');
        }

        protected T FillInFieldByName<T>(string name)
        {
            for(var i = 0; i < Cards.Rows.Count; i++)
            {
                if(!string.Equals(Cards.Rows[i][0].ToString(), name))
                    continue;
                if(string.IsNullOrWhiteSpace((string)Cards.Rows[i][1]))
                    return default(T);
                try
                {
                    return (T)ConvertValue(Cards.Rows[i][1], typeof(T));
                }
                catch(Exception)
                {
                    return default(T);
                }
            }

            return default(T);
        }

        private static object ConvertValue(object value, Type destinationType)
        {
            if(value == null || value == DBNull.Value)
                return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            if(destinationType == typeof(bool))
                return (value is byte b) && b == 1;

            var nullableType = Nullable.GetUnderlyingType(destinationType);
            return Convert.ChangeType(value, nullableType ?? destinationType);
        }

        protected DataTable Cards
        {
            get; set;
        }


        /// <summary>
        /// Диапазоны из карточки
        /// </summary>
        /// <value>
        /// The ranges.
        /// </value>
        protected string Ranges { get; set; }

        /// <summary>
        /// Погрешности из карточки
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        protected string Errors { get; set; }


        /// <summary>
        /// Дополнительная информация из карточки
        /// </summary>
        /// <value>
        /// The addditional information.
        /// </value>
        protected string AddditionalInformation { get; set; }

        /// <summary>
        /// Чем заменено списанное средство
        /// </summary>
        protected string WhatReplaced { get; set; }

        /// <summary>
        ///Номер паспорта средства котором заменили
        /// </summary>
        protected int? NumberReplaced { get; set; }

        /// <summary>
        /// Даты вывода из эксплуатации
        /// </summary>
        protected DateTime? DecommissioningDate { get; set; }

        /// <summary>
        /// Причина вывода из эксплуатации
        /// </summary>
        protected string DecommissioningReason { get; set; }
    }
}

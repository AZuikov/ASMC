using ASMC.Data.Model.Interface;
using DataBase;
using MathStatistick;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.ClimateModel
{
    public class Parametr : IParametrs
    {
        public delegate double DoubleValueGetter(object param);

        public delegate double ParamConverter(double value);
        public int Id { get; set; }
        public string ParametrName { get; set; }
        
        //Функция, возвращающая значение переменной из внешнего источника
        public DoubleValueGetter GetOuterValue { get; set; }
        //Функция, пересчитывающяя значение из базы данных в нужную систему изменений
        public ParamConverter ConvertValue { get; set; }

        public double GetValue(object param = null)
        {
            return ConvertValue(GetOuterValue(param));
        }

        public int Sing { get; set; }
    }
}

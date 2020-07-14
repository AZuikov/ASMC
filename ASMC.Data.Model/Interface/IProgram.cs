using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core;

namespace ASMC.Data.Model.Interface
{
    public interface IProgram    
    {

        /// <summary>
        /// Позволяет получать тип СИ
        /// </summary>
        string Type
        {
            get; 
        }
        /// <summary>
        /// Позволяет получать госреестр СИ
        /// </summary>
        string Grsi
        {
            get; 
        }
        /// <summary>
        /// Позволяет получать диапазон СИ
        /// </summary>
        string Range
        {
            get;
        }
        /// <summary>
        /// Позволяет получить характеристику точности СИ
        /// </summary>
        string Accuracy
        {
            get;
        }
         ITaskMessageService TaskMessageService
        {
            get;
             set;
         }
        /// <summary>
        /// Позволяет получить сушность метрологических операций
        /// </summary>
        AbstraktOperation AbstraktOperation
        {
            get;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP.Utils.Data
{
    /// <summary>
    /// Представляет имя столбца базы данных, на
    /// который проецируется столбец данных свойства.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MapColumnAttribute : Attribute
    {
        /// <summary>
        /// Возвращает имя исходного
        /// столбца.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Возвращает имя целевого
        /// столбца.
        /// </summary>
        public string TargetName
        {
            get;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса
        /// <see cref="MapColumnAttribute"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="targetName"></param>
        public MapColumnAttribute(string name, string targetName)
        {
            Name = name;
            TargetName = targetName;
        }
    }
}

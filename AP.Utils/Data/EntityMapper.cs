using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace AP.Utils.Data
{
    /// <summary>
    /// Представляет вспомогательный класс для проецирования
    /// <see cref="DataTable"/>, <see cref="DataRow"/> в
    /// экземпляр <see cref="T:TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Тип объекта, на который
    /// будет производиться проецирование.</typeparam>
    public class EntityMapper<TEntity> where TEntity : new()
    {
        #region Methods

        /// <summary>
        /// Проецирует <see cref="DataRow"/>
        /// в <see cref="T:TEntity"/>.
        /// </summary>
        /// <param name="dataRow">Экземпляр <see cref="DataRow"/>
        /// для проецирования.</param>
        public TEntity Map(DataRow dataRow)
        {
            var entity = Activator.CreateInstance<TEntity>();
            Map(dataRow, entity);

            return entity;
        }

        /// <summary>
        /// Проецирует <see cref="DataTable"/>
        /// в список <see cref="T:TEntity"/>.
        /// </summary>
        /// <param name="dataTable">Экземпляр <see cref="DataTable"/>
        /// для проецирования.</param>
        public IEnumerable<TEntity> Map(DataTable dataTable)
        {
            var properties = (typeof(TEntity)).GetProperties().ToList();

            var list = new List<TEntity>();
            foreach(var row in dataTable.Rows.Cast<DataRow>())
            {
                var entity = new TEntity();
                foreach(var prop in properties)
                    Map(row, prop, entity);

                list.Add(entity);
            }

            return list.ToArray();
        }

        private static void Map(DataRow dataRow, object entity)
        {
            var properties = entity.GetType().GetProperties().ToList();
            foreach(var prop in properties)
                Map(dataRow, prop, entity);
        }

        private static void Map(DataRow dataRow, PropertyInfo prop, object entity)
        {
            var columnName = prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name;
            if(!prop.CanWrite)
                return;

            if(dataRow.Table.Columns.Contains(columnName))
            {
                var value = ConvertValue(dataRow[columnName], prop.PropertyType);
                if(value != null)
                    prop.SetValue(entity, value);
            }

            var foreignKey = prop.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
            if(foreignKey == null)
                return;

            if(dataRow.Table.Columns.Contains(foreignKey))
            {
                var child = Activator.CreateInstance(prop.PropertyType);
                Map(dataRow, child);

                prop.SetValue(entity, child);
            }
            else
            {
                var foreignProp = entity.GetType().GetProperty(foreignKey);
                if(foreignProp == null || !foreignProp.CanWrite)
                    return;

                var child = Activator.CreateInstance(foreignProp.PropertyType);
                Map(dataRow, child);

                foreignProp.SetValue(entity, child);
            }
        }

        private static object ConvertValue(object value, Type destinationType)
        {
            if(value == null || value == DBNull.Value)
                return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            if(destinationType == typeof(bool))
                return (value is byte b) && b == 1;

            return Convert.ChangeType(value, destinationType);
        }

        #endregion
    }
}

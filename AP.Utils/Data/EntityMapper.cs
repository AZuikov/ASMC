using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace AP.Utils.Data
{
    /// <summary>
    /// Представляет вспомогательный класс для проецирования
    /// <see cref="DataTable"/>, <see cref="DataRow"/> в
    /// объект.
    /// </summary>
    public sealed class EntityMapper
    {
        private static IDictionary<Type, string> _keyCache;

        #region Methods

        /// <summary>
        /// Проецирует <see cref="DataRow"/>
        /// в <see cref="T:TEntity"/>.
        /// </summary>
        /// <param name="dataRow">Экземпляр <see cref="DataRow"/>
        /// для проецирования.</param>
        public TEntity Map<TEntity>(DataRow dataRow) where TEntity : new()
        {
            return (TEntity)Map(dataRow, typeof(TEntity));
        }

        /// <summary>
        /// Проецирует <see cref="DataTable"/>
        /// в список <see cref="T:TEntity"/>.
        /// </summary>
        /// <param name="dataTable">Экземпляр <see cref="DataTable"/>
        /// для проецирования.</param>
        public IEnumerable<TEntity> Map<TEntity>(DataTable dataTable) where TEntity : new()
        {
            var list = new List<TEntity>();
            foreach(var row in dataTable.Rows.Cast<DataRow>())
                list.Add(Map<TEntity>(row));

            return list.ToArray();
        }

        /// <summary>
        /// Проецирует <see cref="DataRow"/>
        /// в объект.
        /// </summary>
        /// <param name="dataRow">Экземпляр <see cref="DataRow"/>
        /// для проецирования.</param>
        /// <param name="entityType">Тип объекта, в который
        /// производится проецирование.</param>
        public object Map(DataRow dataRow, Type entityType)
        {
            var entity = Activator.CreateInstance(entityType);
            Map(dataRow, entity);

            return entity;
        }

        private static void Map(DataRow dataRow, object entity, object key = null, MapColumnAttribute[] mapColumns = null)
        {
            var properties = entity.GetType().GetProperties().ToList();
            foreach(var prop in properties)
                if(key != null && prop.IsDefined(typeof(KeyAttribute)) && prop.CanWrite)
                    prop.SetValue(entity, key);
                else
                    Map(dataRow, prop, entity, mapColumns);
        }

        private static void Map(DataRow dataRow, PropertyInfo prop, object entity, MapColumnAttribute[] mapColumns)
        {
            var columnName = prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name;
            if(!prop.CanWrite)
                return;

            var mappedColumn = mapColumns?.FirstOrDefault(map => string.Equals(map.TargetName, columnName, StringComparison.OrdinalIgnoreCase))?.Name;
            if(mappedColumn != null)
                columnName = mappedColumn;

            if(dataRow.Table.Columns.Contains(columnName))
            {
                var value = ConvertValue(dataRow[columnName], prop.PropertyType);
                if(value != null)
                    prop.SetValue(entity, value);
            }

            var foreignKey = prop.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
            if(foreignKey == null)
                return;

            var nullableType = Nullable.GetUnderlyingType(prop.PropertyType);
            if(nullableType != null)
                return;

            if(dataRow.Table.Columns.Contains(foreignKey))
            {
                if(dataRow[foreignKey] == DBNull.Value)
                    return;

                var child = Activator.CreateInstance(prop.PropertyType);
                Map(dataRow, child, dataRow[foreignKey], prop.GetCustomAttributes<MapColumnAttribute>().ToArray());

                prop.SetValue(entity, child);
            }
            else
            {
                var foreignProp = entity.GetType().GetProperty(foreignKey);
                if(foreignProp == null || !foreignProp.CanWrite)
                    return;

                var key = GetEntityKey(foreignProp.PropertyType);
                if(key == null || !dataRow.Table.Columns.Contains(key) || dataRow[key] == DBNull.Value)
                    return;

                var child = Activator.CreateInstance(foreignProp.PropertyType);
                Map(dataRow, child, dataRow[key], foreignProp.GetCustomAttributes<MapColumnAttribute>().ToArray());

                foreignProp.SetValue(entity, child);
            }
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

        private static string GetEntityKey(Type entityType)
        {
            if(_keyCache == null)
                _keyCache = new ConcurrentDictionary<Type, string>();
            else if(_keyCache.ContainsKey(entityType))
                return _keyCache[entityType];

            var prop = entityType.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            return _keyCache[entityType] = prop?.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop?.Name;
        }

        #endregion
    }

}

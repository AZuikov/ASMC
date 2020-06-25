using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Palsys.Utils.Data;

namespace ASMC.Data.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class MapperSetting  
    {               
        /// <summary>
        ///     Проецирует <see cref="DataRow" />
        ///     в <see cref="T:TEntity" />.
        /// </summary>
        /// <param name="dataRow">
        ///     Экземпляр <see cref="DataRow" />
        ///     для проецирования.
        /// </param>
        public TEntity Map<TEntity>(DataRow dataRow) where TEntity : new()
        {
            return (TEntity)Map(dataRow, typeof(TEntity));
        }

        public DataRow Map<TEntity>(TEntity entity, DataTable dataTable) where TEntity : new()
        {
            var row = dataTable.NewRow();
            InverseMap(row, entity);
            return row;
        }

        public object Map(DataRow dataRow, Type entityType)
        {
            var entity = Activator.CreateInstance(entityType);
            Map(dataRow, entity);
            return entity;
        }


        protected static object ConvertValue(object value, Type destinationType)
        {
            if (value == null || value == DBNull.Value)
                return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            if (destinationType == typeof(bool))
                return value is byte b && b == 1;
            var nullableType = Nullable.GetUnderlyingType(destinationType);
            return Convert.ChangeType(value, nullableType ?? destinationType);
        }

        protected static string MapAttibuteValue(MapColumnAttribute[] mapColumns, string currentName)
        {
            var mappedCell = mapColumns?.FirstOrDefault(map =>
                string.Equals(map.TargetName, currentName, StringComparison.OrdinalIgnoreCase))?.Name;
            return mappedCell;
        }

        public void ColumnGenerationForEntit<TEntity>(TEntity entity, DataTable table, ref Dictionary<string, Type> dictionary) where TEntity : new()
        {
            if (entity == null)
                return;

            var properties = entity.GetType().GetProperties();

            foreach (var properti in properties)
            {
                if (!properti.CanRead)
                    continue;
                var foreingKeyName = properti.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
                if (!string.IsNullOrEmpty(foreingKeyName))
                {
                    if (dictionary.ContainsKey(foreingKeyName)) continue;

                    if (properti.GetValue(entity) == null)
                    {
                        var obj = Activator.CreateInstance(properti.PropertyType);
                        entity.GetType().GetProperty(properti.Name)?.SetValue(entity,obj);
                        ColumnGenerationForEntit(properti.GetValue(entity), table, ref dictionary);
                    }
                    else
                    {
                        ColumnGenerationForEntit(properti.GetValue(entity), table, ref dictionary);
                    }
                    dictionary.Add(foreingKeyName, properti.PropertyType);

                }
                else
                {
                    var name = properti.GetCustomAttribute<ColumnAttribute>()?.Name ??
                               properti.GetCustomAttribute<Palsys.Report.Utils.Data.TableCellAttribute>()?.Name;
                    try
                    {

                        if (!string.IsNullOrEmpty(name))
                            table.Columns.Add(name);
                    }
                    catch (DuplicateNameException)
                    {

                    }
                }
            }

        }
        public void ColumnGenerationForEntit<TEntity>(TEntity entity, DataTable table) where TEntity : new()
        {
            if(table == null || table.AsEnumerable().Any())
                return;
            var dic = new Dictionary<string, Type>();
            ColumnGenerationForEntit(entity, table, ref dic);
        }
        private void InverseMap(DataRow row, object entity)
        {
            if (entity == null) return;
            var properties = entity.GetType().GetProperties();
            foreach (var properti in properties)
            {
                if (!properti.CanRead) continue;
                if (!string.IsNullOrEmpty(properti.GetCustomAttribute<ForeignKeyAttribute>()?.Name))
                {
                    InverseMap(row, properti.GetValue(entity));
                }
                else
                {
                    var name = properti.GetCustomAttribute<ColumnAttribute>()?.Name ??
                               properti.GetCustomAttribute<Palsys.Report.Utils.Data.TableCellAttribute>()?.Name;

                    if (string.IsNullOrEmpty(name)) continue;
                    row[name] = properti.GetValue(entity)?.ToString();
                }
            }
        }
    
        public TEntity Map<TEntity>(DataTable dataTable) where TEntity : new()
        {

            var entity = new TEntity();
                foreach(var row in dataTable.Rows.Cast<DataRow>())
                   Map(row, entity);

            return entity;

        }

        protected void Map(DataRow dataRow, object entity, object key = null, MapColumnAttribute[] mapColumns = null)
        {
            var properties = entity.GetType().GetProperties().ToList();
            foreach(var prop in properties)
                Map(dataRow, prop, entity);
        }

        /// <inheritdoc />
        protected void Map(DataRow dataRow, PropertyInfo prop, object entity, MapColumnAttribute[] mapColumns = null)
        {
            var cellName = prop.GetCustomAttribute<Palsys.Report.Utils.Data.TableCellAttribute>()?.Name ?? prop.Name;
            if(!prop.CanWrite)
                return;
            
          var mappedCell = MapAttibuteValue(mapColumns, cellName);
            if(mappedCell != null) cellName = mappedCell;

            if(!string.Equals(dataRow[Palsys.Report.Utils.Data.TableCellAttribute.ColumnName].ToString(), cellName,
                StringComparison.Ordinal))
                return;
            object value;
            try
            {
                value = ConvertValue(dataRow[Palsys.Report.Utils.Data.TableCellAttribute.ColumnNameValue], prop.PropertyType);
            }
            catch(InvalidCastException)
            {
                value = default(object);
            }
            catch(FormatException)
            {
                value = default(object);
            }
            prop.SetValue(entity, value);
        }  
    }
}

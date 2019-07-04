using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP.Utils.Data
{
    public class ForeignKeyAttributeTypeMapper : FallBackTypeMapper
    {
        public ForeignKeyAttributeTypeMapper(Type sourceType)
            : base(new SqlMapper.ITypeMap[]
            {
                new CustomPropertyTypeMap(sourceType,
                    (type, columnName) =>
                        type.GetProperties().FirstOrDefault(prop =>
                            prop.GetCustomAttributes(false)
                                .OfType<ColumnAttribute>()
                                .Any(attribute => string.Equals(attribute.Name, columnName, StringComparison.OrdinalIgnoreCase))
                        )
                ),
                new DefaultTypeMap(sourceType)
            })
        {
        }
    }
}

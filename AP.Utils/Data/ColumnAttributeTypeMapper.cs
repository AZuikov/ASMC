﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

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

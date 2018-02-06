namespace JQCore.DataAccess.Utils
{
    /// <summary>
    /// Copyright (C) 2018 备胎 版权所有。
    /// 类名：SqlBuilder.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：SQL拼接
    /// 创建标识：yjq 2018/1/12 16:21:34
    /// </summary>
    public class SqlBuilder
    {
        private string _baseSql;
        private DatabaseType _databaseType;
        private SqlWhereBuilder _sqlWhereBuilder;

        public SqlBuilder(DatabaseType databaseType = DatabaseType.MSSQLServer)
        {
            _databaseType = databaseType;
            _sqlWhereBuilder = new SqlWhereBuilder(dbType: databaseType);
        }

        public SqlBuilder(string baseSql, DatabaseType databaseType) : this(databaseType: databaseType)
        {
            _baseSql = baseSql;
        }

        public SqlWhereBuilder WhereBuilder { get { return _sqlWhereBuilder; } }

        public override string ToString()
        {
            if (WhereBuilder.IsEmpty)
            {
                return _baseSql;
            }
            else
            {
                return $"{_baseSql} WHERE {WhereBuilder.ToString()}";
            }
        }
    }
}
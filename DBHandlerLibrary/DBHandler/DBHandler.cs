using System;
using System.Collections.Generic;
using System.Linq;

// Added usings
using DBHandler;
using System.Data;
using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data.SQLite;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;


namespace DBHandler
{
    public partial class DataBaseHandler
    {
        #region base values & constructor

        /// <summary>
        /// The registered object types and the tables to which they're linked
        /// </summary>
        public static Dictionary<Type, string> RegisteredTypes { get; private set; }

        /// <summary>
        /// The registered Database Engine Type
        /// </summary>
        public DatabaseType DBEngineType { get; private set; }

        /// <summary>
        /// The Universal SQL Handler which handles all connections to any database engine and table with any object or non-object used by the coder.
        /// </summary>
        private UniversalSQLHandler universalSQLHandler = null;
        
        /// <summary>
        /// Initializes a new DBHandler class with SQL Connection derived from the specified SQL Database ConnectionString (CAN be either MSSQL or MYSQL)
        /// </summary>
        /// <param name="connString">The SQL Database connection string to use</param>
        public DataBaseHandler(DatabaseType dbType ,string connString)
        {
            try
            {
                if (dbType == DatabaseType.MSSQL)
                {
                    DBEngineType = DatabaseType.MSSQL;
                    universalSQLHandler = new UniversalSQLHandler(DatabaseType.MSSQL, connString);
                }
                else if (dbType == DatabaseType.MYSQL)
                {
                    DBEngineType = DatabaseType.MYSQL;
                    universalSQLHandler = new UniversalSQLHandler(DatabaseType.MYSQL, connString);
                }
                else if (dbType == DatabaseType.SQLITE)
                {
                    DBEngineType = DatabaseType.SQLITE;
                    universalSQLHandler = new UniversalSQLHandler(DatabaseType.SQLITE, connString);
                }
                RegisteredTypes = new Dictionary<Type, string>();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        
        #endregion
    }
}
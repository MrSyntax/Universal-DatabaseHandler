﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace DBHandler
{
    public partial class DataBaseHandler
    {
        #region CRUD commands
        /// <summary>
        /// Inserts the specified OBJECT into the given database table
        /// </summary>
        /// <param name="tableName">The table to add the object to</param>
        /// <param name="o">The Object of type Tshirt, Jas(coat), Customer or Order to add to the table</param>
        /// <returns>Whether successful or not</returns>
        public bool SqlInsert(Object o)
        {
            bool flag = false;

            string cmdtext = "";
            string tableName = string.Empty;

            if (RegisteredTypes.ContainsKey(o.GetType()))
            {
                tableName = RegisteredTypes[o.GetType()];
            }

            if (tableName == string.Empty || tableName == "")
            {
                return false;
            }

            if (DBEngineType == DatabaseType.MSSQL)
            {
                SqlCommand sqlComm = new SqlCommand();
                sqlComm.Parameters.AddRange(universalSQLHandler.GenerateParameters(CRUDCommand.INSERT, o, out cmdtext));
                sqlComm.CommandText = string.Format("INSERT INTO {0} {1}", tableName, cmdtext);
                try
                {
                    universalSQLHandler.ExecuteSqlCommand(CRUDCommand.INSERT, sqlComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            else if (DBEngineType == DatabaseType.MYSQL)
            {
                MySqlCommand mySqlComm = new MySqlCommand();
                mySqlComm.Parameters.AddRange(universalSQLHandler.GenerateParameters(CRUDCommand.INSERT, o, out cmdtext));
                mySqlComm.CommandText = string.Format("INSERT INTO {0} {1}", tableName, cmdtext);
                try
                {
                    universalSQLHandler.ExecuteSqlCommand(CRUDCommand.INSERT, mySqlComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            else if (DBEngineType == DatabaseType.SQLITE)
            {
                SQLiteCommand SQLiteComm = new SQLiteCommand();
                SQLiteComm.Parameters.AddRange(universalSQLHandler.GenerateParameters(CRUDCommand.INSERT, o, out cmdtext));
                SQLiteComm.CommandText = string.Format("INSERT INTO {0} {1}", tableName, cmdtext);
                try
                {
                    universalSQLHandler.ExecuteSqlCommand(CRUDCommand.INSERT, SQLiteComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }

            return flag;
        }

        /// <summary>
        /// Selects all rows from the specified database table and returns a DataTable containing the results
        /// </summary>
        /// <param name="returnDT">The results of the executed query</param>
        /// <returns>Wether successful or not</returns>
        public bool SqlSelectAll(Type objectType, out DataTable returnDT)
        {
            bool flag = false;
            string tableName = string.Empty;

            if (RegisteredTypes.ContainsKey(objectType))
            {
                tableName = RegisteredTypes[objectType];
            }
            else
            {
                returnDT = null;
                return false;
            }

            DataTable dt = null;
            if (DBEngineType == DatabaseType.MSSQL)
            {
                SqlCommand sqlComm = new SqlCommand();
                sqlComm.CommandText = string.Format("SELECT * FROM {0}", tableName);
                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.SELECT, sqlComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            else if (DBEngineType == DatabaseType.MYSQL)
            {
                MySqlCommand mySqlComm = new MySqlCommand();
                mySqlComm.CommandText = string.Format("SELECT * FROM {0}", tableName);
                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.SELECT, mySqlComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }

            }
            else if (DBEngineType == DatabaseType.SQLITE)
            {
                SQLiteCommand SQLiteComm = new SQLiteCommand();
                SQLiteComm.CommandText = string.Format("SELECT * FROM {0}", tableName);
                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.SELECT, SQLiteComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }

            }
            returnDT = dt;
            return flag;
        }

        /// <summary>
        /// Selects the row of which the ID column/value corresponds from the specified database table and returns a DataTable containing the results
        /// </summary>
        /// <param name="tableName">The name of the database table to search in</param>
        /// <param name="idColumnValue">The ID Column name and value to search for</param>
        /// <param name="returnDT">The results of the executed query</param>
        /// <returns>Wether successful or not</returns>
        public bool SqlSelectSpecific(string tableName, KeyValuePair<string, int> idColumnValue, out DataTable returnDT)
        {
            bool flag = false;
            DataTable dt = null;
            if (DBEngineType == DatabaseType.MSSQL)
            {
                SqlCommand sqlComm = new SqlCommand();
                sqlComm.Parameters.Add(new SqlParameter(string.Format("@{0}", idColumnValue.Key), System.Data.SqlDbType.Int, 10, idColumnValue.Key) { Value = idColumnValue.Value });
                sqlComm.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = @{2}", tableName, idColumnValue.Key, idColumnValue.Key);

                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.SELECT, sqlComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            else if (DBEngineType == DatabaseType.MYSQL)
            {
                MySqlCommand mySqlComm = new MySqlCommand();
                mySqlComm.Parameters.Add(new MySqlParameter(string.Format("@{0}", idColumnValue.Key), MySqlDbType.Int32, 10, idColumnValue.Key) { Value = idColumnValue.Value });
                mySqlComm.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = @{2}", tableName, idColumnValue.Key, idColumnValue.Key);

                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.SELECT, mySqlComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            else if (DBEngineType == DatabaseType.SQLITE)
            {
                SQLiteCommand SQLiteComm = new SQLiteCommand();
                SQLiteComm.Parameters.Add(new SQLiteParameter(string.Format("@{0}", idColumnValue.Key), DbType.Int32, 10, idColumnValue.Key) { Value = idColumnValue.Value });
                SQLiteComm.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = @{2}", tableName, idColumnValue.Key, idColumnValue.Key);

                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.SELECT, SQLiteComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            returnDT = dt;
            return flag;
        }

        /// <summary>
        /// Selects the row of which the ID column/value corresponds from the specified database table and returns a DataTable containing the results
        /// </summary>
        /// <param name="objectType">The registered TYPE of the object to search for in the database</param>
        /// <param name="idColumnValue">The ID Column name and value to search for</param>
        /// <param name="returnDT">The results of the executed query</param>
        /// <returns>Wether successful or not</returns>
        public bool SqlSelect(Type objectType, KeyValuePair<string, int> idColumnValue, out DataTable returnDT)
        {
            bool flag = false;
            string tableName = string.Empty;

            if (RegisteredTypes.ContainsKey(objectType))
            {
                tableName = RegisteredTypes[objectType];
            }

            if (tableName == string.Empty || tableName == "")
            {
                returnDT = null;
                return false;
            }

            DataTable dt = null;
            if (DBEngineType == DatabaseType.MSSQL)
            {
                SqlCommand sqlComm = new SqlCommand();
                sqlComm.Parameters.Add(new SqlParameter(string.Format("@{0}", idColumnValue.Key), System.Data.SqlDbType.Int, 10, idColumnValue.Key) { Value = idColumnValue.Value });
                sqlComm.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = @{2}", tableName, idColumnValue.Key, idColumnValue.Key);
                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.SELECT, sqlComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            else if (DBEngineType == DatabaseType.MYSQL)
            {
                MySqlCommand mySqlComm = new MySqlCommand();
                mySqlComm.Parameters.Add(new MySqlParameter(string.Format("@{0}", idColumnValue.Key), MySqlDbType.Int32, 10, idColumnValue.Key) { Value = idColumnValue.Value });
                mySqlComm.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = @{2}", tableName, idColumnValue.Key, idColumnValue.Key);
                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.SELECT, mySqlComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            else if (DBEngineType == DatabaseType.SQLITE)
            {
                SQLiteCommand SQLiteComm = new SQLiteCommand();
                SQLiteComm.Parameters.Add(new SQLiteParameter(string.Format("@{0}", idColumnValue.Key), DbType.Int32, 10, idColumnValue.Key) { Value = idColumnValue.Value });
                SQLiteComm.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = @{2}", tableName, idColumnValue.Key, idColumnValue.Key);
                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.SELECT, SQLiteComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            returnDT = dt;
            return flag;
        }

        /// <summary>
        /// Updates the specified OBJECT from the specified database table
        /// </summary>
        /// <param name="tableName">The table name that contains the OBJECT</param>
        /// <param name="o">The OBJECT to update in the specified database table</param>
        /// <returns>Wether successful or not</returns>
        public bool SqlUpdate(Object o)
        {
            bool flag = false;
            string tableName = string.Empty;

            if (RegisteredTypes.ContainsKey(o.GetType()))
            {
                tableName = RegisteredTypes[o.GetType()];
            }
            if (tableName == string.Empty || tableName == "")
            {
                return false;
            }

            string cmdtext = "";
            if (DBEngineType == DatabaseType.MSSQL)
            {
                SqlCommand sqlComm = new SqlCommand();
                sqlComm.Parameters.AddRange(universalSQLHandler.GenerateParameters(CRUDCommand.UPDATE, o, out cmdtext));
                sqlComm.CommandText = string.Format("UPDATE {0} SET {1}", tableName, cmdtext);
                DataTable dt = null;
                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.UPDATE, sqlComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            else if (DBEngineType == DatabaseType.MYSQL)
            {
                MySqlCommand mySqlComm = new MySqlCommand();
                mySqlComm.Parameters.AddRange(universalSQLHandler.GenerateParameters(CRUDCommand.UPDATE, o, out cmdtext));
                mySqlComm.CommandText = string.Format("UPDATE {0} SET {1}", tableName, cmdtext);
                DataTable dt = null;
                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.UPDATE, mySqlComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            else if (DBEngineType == DatabaseType.SQLITE)
            {
                SQLiteCommand SQLiteComm = new SQLiteCommand();
                SQLiteComm.Parameters.AddRange(universalSQLHandler.GenerateParameters(CRUDCommand.UPDATE, o, out cmdtext));
                SQLiteComm.CommandText = string.Format("UPDATE {0} SET {1}", tableName, cmdtext);
                DataTable dt = null;
                try
                {
                    dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.UPDATE, SQLiteComm);
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            return flag;
        }

        // TODO

        /// <summary>
        /// Removes the specified object from the DBHandler's Universql SQL Handler's linked database type and database object Table.
        /// </summary>
        /// <param name="o">The object to remove from the database</param>
        /// <returns>True if object was successfully removed, false if something went wrong.</returns>
        public bool SqlRemove(Object o)
        {
            bool flag = false;
            string commandtext = string.Empty;
            string tableName = string.Empty;
            DataTable dt = null;

            if (RegisteredTypes.ContainsKey(o.GetType()))
            {
                tableName = RegisteredTypes[o.GetType()];
            }
            if (tableName == string.Empty || tableName == "")
            {
                return false;
            }

            switch (DBEngineType)
            {
                case DatabaseType.MSSQL:
                    SqlCommand msSqlComm = new SqlCommand();
                    msSqlComm.Parameters.AddRange(universalSQLHandler.GenerateParameters(CRUDCommand.REMOVE, o, out commandtext));
                    msSqlComm.CommandText = string.Format("REMOVE * FROM {0} WHERE {1}", tableName, commandtext);
                    try
                    {
                        dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.UPDATE, msSqlComm);
                        flag = true;
                    }
                    catch
                    {
                        flag = false;
                    }
                    break;
                case DatabaseType.MYSQL:
                    MySqlCommand mySqlComm = new MySqlCommand();
                    mySqlComm.Parameters.AddRange(universalSQLHandler.GenerateParameters(CRUDCommand.REMOVE, o, out commandtext));
                    mySqlComm.CommandText = string.Format("REMOVE * FROM {0} WHERE {1}", tableName, commandtext);
                    try
                    {
                        dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.UPDATE, mySqlComm);
                        flag = true;
                    }
                    catch
                    {
                        flag = false;
                    }
                    break;
                case DatabaseType.SQLITE:
                    SQLiteCommand SQLiteComm = new SQLiteCommand();
                    SQLiteComm.Parameters.AddRange(universalSQLHandler.GenerateParameters(CRUDCommand.REMOVE, o, out commandtext));
                    SQLiteComm.CommandText = string.Format("REMOVE * FROM {0} WHERE {1}", tableName, commandtext);
                    try
                    {
                        dt = universalSQLHandler.ExecuteSqlCommand(CRUDCommand.UPDATE, SQLiteComm);
                        flag = true;
                    }
                    catch
                    {
                        flag = false;
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }



            return flag;
        }

        #endregion

        #region Stored Procedure Query methods
        /// <summary>
        /// Gets the specified overview (VIEW) from the database
        /// </summary>
        /// <param name="storedProcedureName">The name of the overview (VIEW) to show</param>
        /// <returns>Datatable containing the overview (VIEW)</returns>
        public DataTable GetOverView(string storedProcedureName)
        {
            if (DBEngineType == DatabaseType.MSSQL)
            {
                return universalSQLHandler.ExecuteStoredProcedure(storedProcedureName);
            }
            else if (DBEngineType == DatabaseType.MYSQL)
            {
                return universalSQLHandler.ExecuteStoredProcedure(storedProcedureName);
            }
            else if (DBEngineType == DatabaseType.SQLITE)
            {
                return universalSQLHandler.ExecuteStoredProcedure(storedProcedureName);
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}

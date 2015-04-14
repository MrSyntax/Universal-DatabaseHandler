using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;

// Added usings
using DBHandler;
using System.Data;

namespace DBHandler
{
    public class DataBaseHandler
    {
        #region base values & constructor

        public static Dictionary<Type, string> RegisteredTypes { get; private set; }
        static private SqlConnection sqlconn;
        static public bool IsClosed { get; private set; }
        
        /// <summary>
        /// Initializes a new DBHandler class with SQL Connection derived from the specified SQL Database ConnectionString (MUST be MSSQL)
        /// </summary>
        /// <param name="connString">The SQL Database connection string to use</param>
        public DataBaseHandler(string connString)
        {
            sqlconn = new SqlConnection(connString);
            RegisteredTypes = new Dictionary<Type, string>();
        }

        /// <summary>
        /// Initializes a new DBHandler class with SQL Connection derived from the specified SQL Database ConnectionString (CAN be either MSSQL or MYSQL)
        /// </summary>
        /// <param name="connString">The SQL Database connection string to use</param>
        public DataBaseHandler(DatabaseType dbType ,string connString)
        {
            if (dbType == DatabaseType.MSSQL)
            {
                sqlconn = new SqlConnection(connString);
            }
            else if (dbType == DatabaseType.MYSQL)
            {
                // TODO
            }
            RegisteredTypes = new Dictionary<Type, string>();
        }
        #endregion

        #region Type (Un-)Registration

        /// <summary>
        /// Registers the specified type with the specified Database Table Name in the internal DBHandler TYPE Dictionary
        /// </summary>
        /// <param name="type">The type to add (Example: typeof(ClassName)) and link to the specified database table entity</param>
        /// <param name="DBTableName">The database table entity name to link to the specified type</param>
        /// <returns>Result of the type registration request</returns>
        public TypeRegResult RegisterType(Type type, string DBTableName)
        {
            foreach (Type reggedType in RegisteredTypes.Keys)
            {
                if (reggedType == type)
                {
                    return TypeRegResult.TypeAlreadyExists;
                }
                else if (RegisteredTypes[reggedType] == DBTableName)
                {
                    return TypeRegResult.DBTableAlreadyExists;
                }
            }

            try
            {
                RegisteredTypes.Add(type, DBTableName);
                if (RegisteredTypes.ContainsKey(type))
                {
                    return TypeRegResult.Successfull;
                }
                else
                {
                    return TypeRegResult.Unsuccessfull;
                }
            }
            catch
            {
                return TypeRegResult.Unknown;
            }
        }

        /// <summary>
        /// Unregisters the specified type from the internal TYPE Dictionary.
        /// </summary>
        /// <param name="type">The type of the Type / DBTableName KeyValuePair to remove</param>
        /// <returns>Unregistration result</returns>
        public TypeRegResult UnRegisterType(Type type)
        {
            if (RegisteredTypes.ContainsKey(type))
            {
                if (RegisteredTypes.Remove(type))
                {
                    return TypeRegResult.Successfull;
                }
                else
                {
                    return TypeRegResult.Unsuccessfull;
                }
            }
            else
            {
                return TypeRegResult.TypeNonExistant;
            }
        }

        /// <summary>
        /// Unregisters the specified type from the internal TYPE Dictionary.
        /// </summary>
        /// <param name="type">The type of the Type / DBTableName KeyValuePair to remove</param>
        /// <returns>Unregistration result</returns>
        public TypeRegResult UnRegisterType(string DBTableName)
        {
            if (RegisteredTypes.ContainsValue(DBTableName))
            {
                foreach (Type type in RegisteredTypes.Keys)
                {
                    if (RegisteredTypes[type] == DBTableName)
                    {
                        if (RegisteredTypes.Remove(type))
                        {
                            return TypeRegResult.Successfull;
                        }
                        else
                        {
                            return TypeRegResult.Unsuccessfull;
                        }
                    }
                }
                return TypeRegResult.Unknown;
            }
            else
            {
                return TypeRegResult.DBTableNameNonExistant;
            }
        }

        #endregion

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

            if(tableName == string.Empty || tableName == "")
            {
                return false;
            }

            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Parameters.AddRange(GenerateParameters(CRUDCommand.INSERT, o, out cmdtext));
            sqlComm.CommandText = string.Format("INSERT INTO {0} {1}", tableName, cmdtext);
            try
            {
                ExecuteSqlCommand("insert", sqlComm);
                flag = true;
            }
            catch
            {
                flag = false;
            }

            return flag;
        }

        /// <summary>
        /// Selects all rows from the specified database table and returns a DataTable containing the results
        /// </summary>
        /// <param name="returnDT">The results of the executed query</param>
        /// <returns>Wether successful or not</returns>
        public bool SqlSelectAll(Type objectType,out DataTable returnDT)
        {
            bool flag = false;
            string tableName = string.Empty;

            if(RegisteredTypes.ContainsKey(objectType))
            {
                tableName = RegisteredTypes[objectType];
            }
            else
            {
                returnDT = null;
                return false;
            }

            SqlCommand sqlComm = new SqlCommand();
            sqlComm.CommandText = string.Format("SELECT * FROM {0}", tableName);
            DataTable dt = null;
            try
            {
                dt = ExecuteSqlCommand("select", sqlComm);
                flag = true;
            }
            catch
            {
                flag = false;
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
            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Parameters.Add(new SqlParameter(string.Format("@{0}", idColumnValue.Key), System.Data.SqlDbType.Int, 10, idColumnValue.Key) { Value = idColumnValue.Value });
            sqlComm.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = @{2}", tableName, idColumnValue.Key, idColumnValue.Key);
            DataTable dt = null;
            try
            {
                dt = ExecuteSqlCommand("select", sqlComm);
                flag = true;
            }
            catch
            {
                flag = false;
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

            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Parameters.Add(new SqlParameter(string.Format("@{0}", idColumnValue.Key), System.Data.SqlDbType.Int, 10, idColumnValue.Key) { Value = idColumnValue.Value });
            sqlComm.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = @{2}", tableName, idColumnValue.Key, idColumnValue.Key);
            DataTable dt = null;
            try
            {
                dt = ExecuteSqlCommand("select", sqlComm);
                flag = true;
            }
            catch
            {
                flag = false;
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
            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Parameters.AddRange(GenerateParameters(CRUDCommand.UPDATE,o, out cmdtext));
            sqlComm.CommandText = string.Format("UPDATE {0} SET {1}", tableName, cmdtext);
            DataTable dt = null;
            try
            {
                dt = ExecuteSqlCommand("update", sqlComm);
                flag = true;
            }
            catch
            {
                flag = false;
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
            return ExecuteStoredProcedure(storedProcedureName);
        }
        #endregion

        #region Database Executables
        /// <summary>
        /// Executes the specified SQL Command as a stored procedure and returns the ROW ID of the row added/edited
        /// </summary>
        /// <param name="sqlComm">The PREPPED SQL Command</param>
        /// <param name="rowID">The ROW ID returned by the query</param>
        /// <returns>Wether successful or not</returns>
        private bool ExecuteStoredProcedure(SqlCommand sqlComm, out int rowID)
        {
            try
            {
                int rowid = 0;
                sqlComm.Connection = sqlconn;
                sqlComm.CommandType = System.Data.CommandType.StoredProcedure;

                if (sqlconn.State != System.Data.ConnectionState.Open)
                {
                    sqlconn.Open();
                    IsClosed = false;
                }

                rowid = (int)sqlComm.ExecuteScalar();


                if (sqlconn.State != System.Data.ConnectionState.Closed)
                {
                    sqlconn.Close();
                }
                rowID = rowid;
                return true;
            }
            catch
            {
                rowID = 0;
                return false;
            }
        }
        /// <summary>
        /// Executes the specified SQL Command as a stored procedure
        /// </summary>
        /// <param name="sqlComm">The PREPPED SQL Command</param>
        private bool ExecuteStoredProcedure(SqlCommand sqlComm)
        {
            sqlComm.Connection = sqlconn;
            sqlComm.CommandType = System.Data.CommandType.StoredProcedure;
            try
            {
                if (sqlconn.State != System.Data.ConnectionState.Open)
                {
                    sqlconn.Open();
                    IsClosed = false;
                }

                sqlComm.ExecuteScalar();


                if (sqlconn.State != System.Data.ConnectionState.Closed)
                {
                    sqlconn.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Executes the specified STORED PROCEDURE and returns a DataTable containing the results
        /// </summary>
        /// <param name="StoredProcedureName">The name of the STORED PROCEDURE to execute</param>
        /// <returns>DataTable containing the STORED PROCEDURE results</returns>
        private DataTable ExecuteStoredProcedure(string StoredProcedureName)
        {
            SqlCommand sqlComm = new SqlCommand(string.Format("SELECT * FROM {0}", StoredProcedureName));
            sqlComm.Connection = sqlconn;
            sqlComm.CommandType = System.Data.CommandType.Text;
            DataTable dt = null;
            try
            {
                if (sqlconn.State != System.Data.ConnectionState.Open)
                {
                    sqlconn.Open();
                    IsClosed = false;
                }

                SqlDataReader sqlReader = sqlComm.ExecuteReader();
                if (sqlReader.HasRows)
                {
                    dt = new DataTable();
                    dt.Load(sqlReader);
                }


                if (sqlconn.State != System.Data.ConnectionState.Closed)
                {
                    sqlconn.Close();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return dt;
        }

        /// <summary>
        /// Executes the given SQLCommand with the appropiate tools derived from the given crudCommand.
        /// </summary>
        /// <param name="crudCmd">Can be either 'select','insert','update' or 'delete'</param>
        /// <param name="sqlComm">The prepped SQLCommand to execute</param>
        private DataTable ExecuteSqlCommand(string crudCmd, SqlCommand sqlComm)
        {
            sqlComm.Connection = sqlconn;
            DataTable dt = null;
            if (sqlconn.State != System.Data.ConnectionState.Open)
            {
                sqlconn.Open();
                IsClosed = false;
            }

            switch (crudCmd)
            {
                case "select":
                    SqlDataReader sqlReader = sqlComm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(sqlReader);

                    break;
                case "insert":

                    int rowsAffected = sqlComm.ExecuteNonQuery();

                    break;
                case "update":
                    rowsAffected = sqlComm.ExecuteNonQuery();

                    break;
                case "remove":
                    rowsAffected = sqlComm.ExecuteNonQuery();

                    break;
            }

            if (sqlconn.State != System.Data.ConnectionState.Closed)
            {
                sqlconn.Close();
            }
            return dt;
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Closes the connection to the SQL database if the connection is deemed still open. Returns true or false depending on a successful connection close.
        /// </summary>
        static public bool CloseConnection
        {
            get
            {
                bool flag = false;
                try
                {
                    sqlconn.Close();
                    flag = true;
                }
                catch (Exception ex)
                {
                    flag = false;
                }
                IsClosed = flag;
                return flag;
            }
        }

        /// <summary>
        /// Generates an insert SQL query with the needed parameters derived from the Object specified.
        /// </summary>
        /// <param name="O">Object of a type that has been registered in the internal type library of the DBHandler</param>
        /// <param name="cmdText">The string field to store the SQL Query string in</param>
        /// <returns>An array of SQL Parameters to be added to the SQLCommand</returns>
        private SqlParameter[] GenerateParameters(CRUDCommand crudCmd,Object O, out string cmdText)
        {
            List<SqlParameter> sqlParamColl = new List<SqlParameter>();
            string columnNames = "";
            string values = "";
            string cmdtext = "";

            switch (crudCmd)
            {
                case CRUDCommand.SELECT:
                    break;
                case CRUDCommand.INSERT:
                    columnNames = "(";
                    values = " ) VALUES (";
                    cmdtext = "";
                    break;
                case CRUDCommand.UPDATE:
                    columnNames = "";
                    values = "";
                    cmdtext = "";
                    break;
                case CRUDCommand.REMOVE:
                    break;
            }

            if (RegisteredTypes.ContainsKey(O.GetType()))
            {
                // Cast the object to the base class
                DBHandlerEntity dbhe = (DBHandlerEntity)O;
                Dictionary<string, object> data = dbhe.GetData;

                // Get the specific column properties needed for the parameters to work
                Dictionary<string, KeyValuePair<SqlDbType, int>> ColumnSpecifics = GetColumnSpecifics(RegisteredTypes[O.GetType()]);

                // Iterate through all the keys/columns and generate all needed parameters
                foreach(string key in data.Keys)
                {
                    if(crudCmd == CRUDCommand.INSERT)
                    {
                        // PreFormat the command text
                        columnNames += key;
                        values += string.Format("{0} = @{1}", key, key);

                        // Seperate command text parameters if needed
                        if (key != data.Keys.Last())
                        {
                            columnNames += ", ";
                            values += ", ";
                        }
                    }
                    else if(crudCmd == CRUDCommand.UPDATE)
                    {
                        values += string.Format("{0} = @{1}", key, key);
                        // Seperate command text parameters if needed
                        if (key != data.Keys.Last())
                        {
                            values += ", ";
                        }
                    }

                    // Create the parameter and assign the appropiate value
                    SqlParameter keyParam = new SqlParameter(string.Format("@{0}", key), ColumnSpecifics[key].Key, ColumnSpecifics[key].Value, key) { Value =  data[key] };

                    sqlParamColl.Add(keyParam);
                }
                if(crudCmd == CRUDCommand.INSERT)
                {
                    // Finalize the command text
                    cmdtext = string.Format("{0} {1}", columnNames, values);
                }
                else if(crudCmd == CRUDCommand.UPDATE)
                {
                    // Finalize the command text
                    cmdtext = string.Format("{0}", values);
                }
            }

            // Return the generated results
            cmdText = cmdtext;
            return sqlParamColl.ToArray();
        }

        /// <summary>
        /// Gets the column specific details for the specified table
        /// </summary>
        /// <param name="tableName">The table for which to get the columns details</param>
        /// <returns>Keyvaluepairs of the value TYPE and the field Length arranged by column name</returns>
        private Dictionary<string, KeyValuePair<SqlDbType, int>> GetColumnSpecifics(string tableName)
        {
            Dictionary<string, KeyValuePair<SqlDbType, int>> columnSpecifics = null;

            //select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'myTable'
            string commandText = string.Format("select COLUMN_NAME, DATA_TYPE, NUMERIC_PRECISION from INFORMATION_SCHEMA.COLUMNS where @TABLE_NAME = '{0}'", tableName);
            SqlCommand sqlComm = new SqlCommand(commandText, sqlconn);
            sqlComm.Parameters.Add(new SqlParameter(string.Format("@TABLE_NAME"), SqlDbType.NVarChar, 128, "TABLE_NAME") { Value = tableName });
            DataTable resultsDT = null;

            try
            {
                // Execute the command and load the results into a datatable for ease of usage
                if(sqlconn.State != ConnectionState.Open)
                {
                    sqlconn.Open();
                }

                SqlDataReader sqlReader = sqlComm.ExecuteReader();

                resultsDT = sqlReader.GetSchemaTable();
                resultsDT.Load(sqlReader);

                if (sqlconn.State != ConnectionState.Closed)
                {
                    sqlconn.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (resultsDT != null)
            {
                if (resultsDT.Rows.Count > 0)
                {
                    columnSpecifics = new Dictionary<string, KeyValuePair<SqlDbType, int>>();
                    foreach (DataRow dr in resultsDT.Rows)
                    {
                        switch (dr["DATA_TYPE"].ToString())
                        {
                            case "int":
                                columnSpecifics.Add(dr["COLUMN_NAME"].ToString(), new KeyValuePair<SqlDbType, int>(SqlDbType.Int, int.Parse(dr["NUMERIC_PRECISION"].ToString())));
                                break;
                            case "varchar":
                                columnSpecifics.Add(dr["COLUMN_NAME"].ToString(), new KeyValuePair<SqlDbType, int>(SqlDbType.VarChar, int.Parse(dr["NUMERIC_PRECISION"].ToString())));
                                break;
                            case "nvarchar":
                                columnSpecifics.Add(dr["COLUMN_NAME"].ToString(), new KeyValuePair<SqlDbType, int>(SqlDbType.NVarChar, int.Parse(dr["NUMERIC_PRECISION"].ToString())));
                                break;
                            case "decimal":
                                columnSpecifics.Add(dr["COLUMN_NAME"].ToString(), new KeyValuePair<SqlDbType, int>(SqlDbType.Decimal, int.Parse(dr["NUMERIC_PRECISION"].ToString())));
                                break;
                        }
                    }
                }
            }
            return columnSpecifics;
        }

        public static class Convert
        {
            public static class DBTable
            {
                /// <summary>
                /// Work in Progress
                /// </summary>
                /// <param name="objectType">The TYPE of the object for which to convert the datatable for</param>
                /// <param name="dt">The datatable to convert to a class Object</param>
                /// <returns>The object containing the information from the datatable</returns>
                public static Object ToObject(Type objectType, DataTable dt)
                {
                    Object objToReturn = null;
                    
                    if(DataBaseHandler.RegisteredTypes.ContainsKey(objectType))
                    {
                        objToReturn = new Object();
                        DBHandlerEntity dbhe = (DBHandlerEntity)objToReturn;

                        if (dt.Rows.Count == 1)
                        {
                            Dictionary<string, object> objDetails = new Dictionary<string, object>();
                            foreach(DataColumn column in dt.Columns)
                            {
                                objDetails.Add(column.ColumnName, dt.Rows[0][column]);
                            }
                            dbhe.SetData = objDetails;
                        }
                        else
                        {
                            return null;
                        }
                        objToReturn = dbhe;
                    }
                    return objToReturn;
                }
                /// <summary>
                /// TODO
                /// </summary>
                /// <param name="SYSID"></param>
                /// <param name="o"></param>
                /// <returns></returns>
                public static DataTable ToDataTable(int SYSID, Object o)
                {
                    DBHandlerEntity dbhe = null;
                    if (DataBaseHandler.RegisteredTypes.ContainsKey(o.GetType()))
                    {
                        dbhe = (DBHandlerEntity)o;
                    }
                    else
                    {
                        return null;
                    }

                    Dictionary<string, object> objectData = dbhe.GetData;
                    DataTable dt = new DataTable();
                    
                    foreach(string key in objectData.Keys)
                    {
                        dt.Columns.Add(key);
                    }

                    dt.Rows.Add(objectData.Values);

                    return new DataTable();
                }
            }
        }
        #endregion

        public bool SqlRemove(Object o)
        {
            throw new NotImplementedException();
        }
    }
}
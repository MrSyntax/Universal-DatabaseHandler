using System;
using System.Collections.Generic;
using System.Linq;

// Added usings
using System.Data;

using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Data.SQLite;
using Oracle.DataAccess.Client;
using Npgsql;
using NpgsqlTypes;


namespace DBHandler
{
    /// <summary>
    /// A Universal SQL Handler which handles any type of database engine soo long as it's registered in the internal DBHandler.DataBaseType library
    /// </summary>
    public class UniversalSQLHandler
    {
        DatabaseType DataBaseType;
        private static object UniversalSqlConnection;
        
        /// <summary>
        /// Instantiate a new Universal SQL Handler class to handle connections and commands with the specified database engine type and connectionstring.
        /// </summary>
        /// <param name="databaseType">The Database Engine Type to instantiate the class for (e.g. MSSQL, MYSQL,..)</param>
        /// <param name="connectionString">The connectionstring to the database that is to be manipulated.</param>
        public UniversalSQLHandler(DatabaseType databaseType, string connectionString)
        {
            DataBaseType = databaseType;
            switch (DataBaseType)
            {
                case DatabaseType.MSSQL:
                    SqlConnection msSqlConn = new SqlConnection(connectionString);
                    UniversalSqlConnection = msSqlConn;
                    break;
                case DatabaseType.MYSQL:
                    MySqlConnection mySqlConn = new MySqlConnection(connectionString);
                    UniversalSqlConnection = mySqlConn;
                    break;
                case DatabaseType.SQLITE:
                    SQLiteConnection SQLiteConn = new SQLiteConnection(connectionString);
                    UniversalSqlConnection = SQLiteConn;
                    break;
                case DatabaseType.ORACLEDB:
                    OracleConnection oracleConn = new OracleConnection(connectionString);
                    UniversalSqlConnection = oracleConn;
                    break;
                case DatabaseType.POSTGRESQL:
                    NpgsqlConnection postGreSqlConn = new NpgsqlConnection(connectionString);
                    UniversalSqlConnection = postGreSqlConn;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        #region Parameter Generation and Column Specification
        /// <summary>
        /// Generates an insert SQL query with the needed parameters derived from the Object specified.
        /// </summary>
        /// <param name="O">Object of a type that has been registered in the internal type library of the DBHandler</param>
        /// <param name="cmdText">The string field to store the SQL Query string in</param>
        /// <returns>An array of SQL Parameters to be added to the SQLCommand</returns>
        public object[] GenerateParameters(CRUDCommand crudCmd, Object O, out string cmdText)
        {
            // Parameter array
            List<object> sqlParamColl = new List<object>();

            string columnNames = "";
            string values = "";
            string cmdtext = "";

            #region Crud command defining
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
            #endregion

            #region Object casting and memory object data retrieval

            // Cast the object to the base class

            DBHandlerEntity dbhe = (DBHandlerEntity)O;
            Dictionary<string, object> data = dbhe.GetData;

            // Get the specific column properties needed for the parameters to work
            Dictionary<string, KeyValuePair<object, int>> ColumnSpecifics = GetColumnSpecifics(DataBaseHandler.RegisteredTypes[O.GetType()]);

            #endregion

            #region Parameter building

            // Iterate through all the keys/columns and generate all needed parameters
            foreach (string key in data.Keys)
            {
                #region Define command text buildup
                if (crudCmd == CRUDCommand.INSERT)
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
                else if (crudCmd == CRUDCommand.UPDATE)
                {
                    values += string.Format("{0} = @{1}", key, key);
                    // Seperate command text parameters if needed
                    if (key != data.Keys.Last())
                    {
                        values += ", ";
                    }
                }
                #endregion

                // Create the parameter and assign the appropiate value
                switch (DataBaseType)
                {
                    case DatabaseType.MSSQL:
                        SqlParameter sqlParam = new SqlParameter(string.Format("@{0}", key), (SqlDbType)ColumnSpecifics[key].Key, ColumnSpecifics[key].Value, key) { Value = data[key] };

                        sqlParamColl.Add(sqlParam);
                        break;
                    case DatabaseType.MYSQL:
                        MySqlParameter mySqlParam = new MySqlParameter(string.Format("@{0}", key), (MySqlDbType)ColumnSpecifics[key].Key, ColumnSpecifics[key].Value, key) { Value = data[key] };

                        sqlParamColl.Add(mySqlParam);
                        break;
                    case DatabaseType.SQLITE:
                        SQLiteParameter SQLiteParam = new SQLiteParameter(string.Format("@{0}", key), (DbType)ColumnSpecifics[key].Key, ColumnSpecifics[key].Value, key) { Value = data[key] };

                        sqlParamColl.Add(SQLiteParam);
                        break;
                    case DatabaseType.ORACLEDB:
                        OracleParameter oracleParam = new OracleParameter(string.Format("@{0}", key), (OracleDbType)ColumnSpecifics[key].Key, ColumnSpecifics[key].Value, key) { Value = data[key] };

                        sqlParamColl.Add(oracleParam);
                        break;
                    case DatabaseType.POSTGRESQL:
                        NpgsqlParameter postGreSqlParam = new NpgsqlParameter(string.Format("@{0}", key), (NpgsqlDbType)ColumnSpecifics[key].Key, ColumnSpecifics[key].Value, key) { Value = data[key] };

                        sqlParamColl.Add(postGreSqlParam);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            #endregion

            #region Command text finalization
            if (crudCmd == CRUDCommand.INSERT)
            {
                // Finalize the command text
                cmdtext = string.Format("{0} {1}", columnNames, values);
            }
            else if (crudCmd == CRUDCommand.UPDATE)
            {
                // Finalize the command text
                cmdtext = string.Format("{0}", values);
            }
            #endregion

            // Return the generated results
            cmdText = cmdtext;
            return sqlParamColl.ToArray();
        }

        /// <summary>
        /// Gets the column specific details for the specified table
        /// </summary>
        /// <param name="tableName">The table for which to get the columns details</param>
        /// <returns>Keyvaluepairs of the value TYPE and the field Length arranged by column name</returns>
        private Dictionary<string, KeyValuePair<object, int>> GetColumnSpecifics(string tableName)
        {
            Dictionary<string, KeyValuePair<object, int>> columnSpecifics = null;
            string commandText = string.Empty;
            DataTable resultsDT = null;

            switch (DataBaseType)
            {
                case DatabaseType.MSSQL:
                    #region MSSQL Table Information command execution
                    commandText = string.Format("select COLUMN_NAME, DATA_TYPE, NUMERIC_PRECISION from INFORMATION_SCHEMA.COLUMNS where @TABLE_NAME = '{0}'", tableName);
                    SqlConnection msSqlConn = (SqlConnection)UniversalSqlConnection;
                    SqlCommand msSqlComm = new SqlCommand(commandText, (SqlConnection)UniversalSqlConnection);
                    msSqlComm.Parameters.Add(new SqlParameter(string.Format("@TABLE_NAME"), SqlDbType.NVarChar, 128, "TABLE_NAME") { Value = tableName });

                    // Execute the command and load the results into a datatable for ease of usage
                    try
                    {
                        if (msSqlConn.State != ConnectionState.Open)
                        {
                            msSqlConn.Open();
                        }

                        SqlDataReader sqlReader = msSqlComm.ExecuteReader();

                        resultsDT = sqlReader.GetSchemaTable();
                        resultsDT.Load(sqlReader);

                        if (msSqlConn.State != ConnectionState.Closed)
                        {
                            msSqlConn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    #endregion
                    break;
                case DatabaseType.MYSQL:
                    #region MYSQL Table Information command execution
                    commandText = string.Format("describe {0}", tableName);
                    MySqlConnection mySqlConn = (MySqlConnection)UniversalSqlConnection;
                    MySqlCommand sqlComm = new MySqlCommand(commandText, mySqlConn);

                    // Execute the command and load the results into a datatable for ease of usage
                    try
                    {
                        if (mySqlConn.State != ConnectionState.Open)
                        {
                            mySqlConn.Open();
                        }

                        MySqlDataReader mySqlReader = sqlComm.ExecuteReader();

                        resultsDT = mySqlReader.GetSchemaTable();
                        resultsDT.Load(mySqlReader);

                        if (mySqlConn.State != ConnectionState.Closed)
                        {
                            mySqlConn.Close();
                        }
                    }
                    catch
                    {
                        throw;
                    }
                    #endregion
                    break;
                case DatabaseType.SQLITE:
                    #region SQLite Table Information command execution
                    commandText = string.Format("describe {0}", tableName);
                    SQLiteConnection SQLiteConn = (SQLiteConnection)UniversalSqlConnection;
                    SQLiteCommand SQLiteComm = new SQLiteCommand(commandText, SQLiteConn);

                    // Execute the command and load the results into a datatable for ease of usage
                    try
                    {
                        if (OpenUniversalSQLConnection())
                        {

                            SQLiteDataReader mySqlReader = SQLiteComm.ExecuteReader();

                            resultsDT = mySqlReader.GetSchemaTable();
                            resultsDT.Load(mySqlReader);

                            CloseUniversalSQLConnection();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    #endregion
                    break;
                case DatabaseType.ORACLEDB:
                    #region ORACLE Table Information command execution
                    commandText = string.Format("DESCRIBE {0}", tableName);
                    OracleConnection oracleConn = (OracleConnection)UniversalSqlConnection;
                    OracleCommand oracleComm = new OracleCommand(commandText, oracleConn);

                    // Execute the command and load the results into a datatable for ease of usage
                    try
                    {
                        if (oracleConn.State != ConnectionState.Open)
                        {
                            oracleConn.Open();
                        }

                        OracleDataReader oracleSqlReader = oracleComm.ExecuteReader();

                        resultsDT = oracleSqlReader.GetSchemaTable();
                        resultsDT.Load(oracleSqlReader);

                        if (oracleConn.State != ConnectionState.Closed)
                        {
                            oracleConn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    #endregion
                    break;
                case DatabaseType.POSTGRESQL:
                    #region PostGreSQL Table Information command execution
                    commandText = string.Format("DESCRIBE {0}", tableName);
                    NpgsqlConnection postGreSqlConn = (NpgsqlConnection)UniversalSqlConnection;
                    NpgsqlCommand postGreSqlComm = new NpgsqlCommand(commandText, postGreSqlConn);

                    // Execute the command and load the results into a datatable for ease of usage
                    try
                    {
                        if (postGreSqlConn.State != ConnectionState.Open)
                        {
                            postGreSqlConn.Open();
                        }

                        NpgsqlDataReader oracleSqlReader = postGreSqlComm.ExecuteReader();

                        resultsDT = oracleSqlReader.GetSchemaTable();
                        resultsDT.Load(oracleSqlReader);

                        if (postGreSqlConn.State != ConnectionState.Closed)
                        {
                            postGreSqlConn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    #endregion
                    break;
                default:
                    throw new NotSupportedException();
            }

            #region DataTable information retrieval
            if (resultsDT != null)
            {
                if (resultsDT.Rows.Count > 0)
                {
                    columnSpecifics = new Dictionary<string, KeyValuePair<object, int>>();
                    switch (DataBaseType)
                    {
                        case DatabaseType.MSSQL:
                            #region MSSQL DataTable retrieval
                            foreach (DataRow dr in resultsDT.Rows)
                            {
                                switch (dr["DATA_TYPE"].ToString())
                                {
                                    case "int":
                                        columnSpecifics.Add(dr["COLUMN_NAME"].ToString(), new KeyValuePair<object, int>(SqlDbType.Int, int.Parse(dr["NUMERIC_PRECISION"].ToString())));
                                        break;
                                    case "varchar":
                                        columnSpecifics.Add(dr["COLUMN_NAME"].ToString(), new KeyValuePair<object, int>(SqlDbType.VarChar, int.Parse(dr["NUMERIC_PRECISION"].ToString())));
                                        break;
                                    case "nvarchar":
                                        columnSpecifics.Add(dr["COLUMN_NAME"].ToString(), new KeyValuePair<object, int>(SqlDbType.NVarChar, int.Parse(dr["NUMERIC_PRECISION"].ToString())));
                                        break;
                                    case "decimal":
                                        columnSpecifics.Add(dr["COLUMN_NAME"].ToString(), new KeyValuePair<object, int>(SqlDbType.Decimal, int.Parse(dr["NUMERIC_PRECISION"].ToString())));
                                        break;
                                }
                            }
                            #endregion
                            break;
                        case DatabaseType.MYSQL:
                            #region MYSQL DataTable retrieval
                            foreach (DataRow dr in resultsDT.Rows)
                            {
                                string typeAndLength = dr["Type"].ToString();
                                int oBracket = typeAndLength.IndexOf('(');
                                int cBracket = typeAndLength.IndexOf(')');
                                int columnLength = int.Parse(typeAndLength.Substring(oBracket, (cBracket - oBracket)));

                                switch (typeAndLength.Substring(0, oBracket))
                                {
                                    case "int":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(MySqlDbType.Int32, columnLength));
                                        break;
                                    case "varchar":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(MySqlDbType.VarChar, columnLength));
                                        break;
                                    case "nvarchar":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(MySqlDbType.VarString, columnLength));
                                        break;
                                    case "decimal":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(MySqlDbType.Decimal, columnLength));
                                        break;
                                }
                            }
                            #endregion
                            break;
                        case DatabaseType.SQLITE:
                            #region SQLITE DataTable retrieval
                            foreach (DataRow dr in resultsDT.Rows)
                            {
                                string typeAndLength = dr["Type"].ToString();
                                int oBracket = typeAndLength.IndexOf('(');
                                int cBracket = typeAndLength.IndexOf(')');
                                int columnLength = int.Parse(typeAndLength.Substring(oBracket, (cBracket - oBracket)));

                                switch (typeAndLength.Substring(0, oBracket))
                                {
                                    case "int":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(DbType.Int32, columnLength));
                                        break;
                                    case "string":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(DbType.String, columnLength));
                                        break;
                                    case "ansistring":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(DbType.AnsiString, columnLength));
                                        break;
                                    case "decimal":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(DbType.Decimal, columnLength));
                                        break;
                                }
                            }
                            #endregion
                            break;
                        case DatabaseType.ORACLEDB:
                            #region ORACLEDB DataTable retrieval
                            foreach (DataRow dr in resultsDT.Rows)
                            {
                                string typeAndLength = dr["Type"].ToString();
                                int oBracket = typeAndLength.IndexOf('(');
                                int cBracket = typeAndLength.IndexOf(')');
                                int columnLength = int.Parse(typeAndLength.Substring(oBracket, (cBracket - oBracket)));

                                switch (typeAndLength.Substring(0, oBracket))
                                {
                                    case "int":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(OracleDbType.Int32, columnLength));
                                        break;
                                    case "varchar2":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(OracleDbType.Varchar2, columnLength));
                                        break;
                                    case "nvarchar2":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(OracleDbType.NVarchar2, columnLength));
                                        break;
                                    case "decimal":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(OracleDbType.Decimal, columnLength));
                                        break;
                                }
                            }
                            #endregion
                            break;
                        case DatabaseType.POSTGRESQL:
                            #region PostGreSQL DataTable retrieval
                            foreach (DataRow dr in resultsDT.Rows)
                            {
                                string typeAndLength = dr["Type"].ToString();
                                int oBracket = typeAndLength.IndexOf('(');
                                int cBracket = typeAndLength.IndexOf(')');
                                int columnLength = int.Parse(typeAndLength.Substring(oBracket, (cBracket - oBracket)));

                                switch (typeAndLength.Substring(0, oBracket))
                                {
                                    case "integer":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(NpgsqlDbType.Integer, columnLength));
                                        break;
                                    case "varchar":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(NpgsqlDbType.Varchar, columnLength));
                                        break;
                                    case "text":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(NpgsqlDbType.Text, columnLength));
                                        break;
                                    case "double":
                                        columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<object, int>(NpgsqlDbType.Double, columnLength));
                                        break;
                                }
                            }
                            #endregion
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            #endregion

            return columnSpecifics;
        }
        #endregion

        #region Database Executables
        /// <summary>
        /// Executes the specified SQL Command as a stored procedure and returns the ROW ID of the row added/edited
        /// </summary>
        /// <param name="sqlComm">The PREPPED SQL Command</param>
        /// <param name="rowID">The ROW ID returned by the query</param>
        /// <returns>Wether successful or not</returns>
        internal bool ExecuteStoredProcedure(object SqlCommandObject, out int rowID)
        {
            int rowid = 0;
            bool flag = false;

            switch (DataBaseType)
            {
                case DatabaseType.MSSQL:
                    #region MSSQL Command Execution
                    SqlCommand msSqlComm = (SqlCommand)SqlCommandObject;
                    SqlConnection msSqlConn = (SqlConnection)UniversalSqlConnection;
                    msSqlComm.Connection = msSqlConn;
                    msSqlComm.CommandType = System.Data.CommandType.StoredProcedure;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            rowid = (int)msSqlComm.ExecuteScalar();
                            flag = true;
                        }
                        catch
                        {
                            rowID = 0;
                            return false;
                        }
                        CloseUniversalSQLConnection();
                    }
                    break;
                    #endregion
                case DatabaseType.MYSQL:
                    #region MYSQL Command Execution
                    MySqlCommand mySqlComm = (MySqlCommand)SqlCommandObject;
                    MySqlConnection mySqlConn = (MySqlConnection)UniversalSqlConnection;
                    mySqlComm.Connection = mySqlConn;
                    mySqlComm.CommandType = System.Data.CommandType.StoredProcedure;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            rowid = (int)mySqlComm.ExecuteScalar();
                            flag = true;
                        }
                        catch
                        {
                            rowID = 0;
                            return false;
                        }
                        CloseUniversalSQLConnection();
                    }
                    break;
                    #endregion
                case DatabaseType.SQLITE:
                    #region SQLite Command Execution
                    SQLiteCommand SQLiteComm = (SQLiteCommand)SqlCommandObject;
                    SQLiteConnection SQLiteConn = (SQLiteConnection)UniversalSqlConnection;
                    SQLiteComm.Connection = SQLiteConn;
                    SQLiteComm.CommandType = System.Data.CommandType.StoredProcedure;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            rowid = (int)SQLiteComm.ExecuteScalar();
                            flag = true;
                        }
                        catch
                        {
                            rowID = 0;
                            return false;
                        }
                        CloseUniversalSQLConnection();
                    }
                    break;
                    #endregion
                case DatabaseType.ORACLEDB:
                    #region Oracle Command Execution
                    OracleCommand oracleComm = (OracleCommand)SqlCommandObject;
                    OracleConnection oracleConn = (OracleConnection)UniversalSqlConnection;
                    oracleComm.Connection = oracleConn;
                    oracleComm.CommandType = System.Data.CommandType.StoredProcedure;
                    if(OpenUniversalSQLConnection())
                    {
                        try
                        {
                            rowid = (int)oracleComm.ExecuteScalar();
                            flag = true;
                        }
                        catch
                        {
                            rowID = 0;
                            return false;
                        }
                        CloseUniversalSQLConnection();
                    }
                    break;
                    #endregion
                case DatabaseType.POSTGRESQL:
                    #region PostGreSql Command Execution
                    NpgsqlCommand postGreSqlComm = (NpgsqlCommand)SqlCommandObject;
                    NpgsqlConnection postGreSqlConn = (NpgsqlConnection)UniversalSqlConnection;
                    postGreSqlComm.Connection = postGreSqlConn;
                    postGreSqlComm.CommandType = System.Data.CommandType.StoredProcedure;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            rowid = (int)postGreSqlComm.ExecuteScalar();
                            flag = true;
                        }
                        catch
                        {
                            rowID = 0;
                            return false;
                        }
                        CloseUniversalSQLConnection();
                    }
                    break;
                    #endregion
                default:
                    throw new NotSupportedException();
            }
            rowID = rowid;
            return flag;
        }

        /// <summary>
        /// Executes the specified SQL Command as a stored procedure
        /// </summary>
        /// <param name="sqlComm">The PREPPED SQL Command</param>
        internal bool ExecuteStoredProcedure(object UniversalSQLCommandObject)
        {
            bool flag = false;
            switch (DataBaseType)
            {
                case DatabaseType.MSSQL:
                    #region MSSQL Command Execution
                    SqlCommand msSqlComm = (SqlCommand)UniversalSQLCommandObject;
                    msSqlComm.Connection = (SqlConnection)UniversalSqlConnection;
                    msSqlComm.CommandType = System.Data.CommandType.StoredProcedure;

                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            msSqlComm.ExecuteScalar();
                            flag = true;
                        }
                        catch
                        {
                            return false;
                        }
                        finally
                        {
                            CloseUniversalSQLConnection();
                        }
                    }
                    break;
                    #endregion
                case DatabaseType.MYSQL:
                    #region MYSQL Command Execution
                    MySqlCommand mySqlComm = (MySqlCommand)UniversalSQLCommandObject;
                    mySqlComm.Connection = (MySqlConnection)UniversalSqlConnection;
                    mySqlComm.CommandType = System.Data.CommandType.StoredProcedure;

                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            mySqlComm.ExecuteScalar();
                            flag = true;
                        }
                        catch
                        {
                            return false;
                        }
                        finally
                        {
                            CloseUniversalSQLConnection();
                        }
                    }
                    break;
                    #endregion
                case DatabaseType.SQLITE:
                    #region SQLite Command Execution
                    SQLiteCommand SQLiteComm = (SQLiteCommand)UniversalSQLCommandObject;
                    SQLiteConnection SQLiteConn = (SQLiteConnection)UniversalSqlConnection;
                    SQLiteComm.Connection = SQLiteConn;
                    SQLiteComm.CommandType = System.Data.CommandType.StoredProcedure;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            SQLiteComm.ExecuteScalar();
                            flag = true;
                        }
                        catch
                        {
                            return false;
                        }
                        finally
                        {
                            CloseUniversalSQLConnection();
                        }
                    }
                    break;
                    #endregion
                case DatabaseType.ORACLEDB:
                    #region Oracle Command Execution
                    OracleCommand oracleComm = (OracleCommand)UniversalSQLCommandObject;
                    OracleConnection oracleConn = (OracleConnection)UniversalSqlConnection;
                    oracleComm.Connection = oracleConn;
                    oracleComm.CommandType = System.Data.CommandType.StoredProcedure;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            oracleComm.ExecuteScalar();
                            flag = true;
                        }
                        catch
                        {
                            return false;
                        }
                        finally
                        {
                            CloseUniversalSQLConnection();
                        }
                    }
                    break;
                    #endregion
                case DatabaseType.POSTGRESQL:
                    #region Oracle Command Execution
                    NpgsqlCommand postGreSqlComm = (NpgsqlCommand)UniversalSQLCommandObject;
                    NpgsqlConnection postGreSqlConn = (NpgsqlConnection)UniversalSqlConnection;
                    postGreSqlComm.Connection = postGreSqlConn;
                    postGreSqlComm.CommandType = System.Data.CommandType.StoredProcedure;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            postGreSqlComm.ExecuteScalar();
                            flag = true;
                        }
                        catch
                        {
                            return false;
                        }
                        finally
                        {
                            CloseUniversalSQLConnection();
                        }
                    }
                    break;
                    #endregion
                default:
                    throw new NotSupportedException();
            }
            return flag;
        }

        /// <summary>
        /// Executes the specified STORED PROCEDURE and returns a DataTable containing the results
        /// </summary>
        /// <param name="StoredProcedureName">The name of the STORED PROCEDURE to execute</param>
        /// <returns>DataTable containing the STORED PROCEDURE results</returns>
        internal DataTable ExecuteStoredProcedure(string StoredProcedureName)
        {
            DataTable dt = null;
            switch (DataBaseType)
            {
                case DatabaseType.MSSQL:
                    #region MSSQL Command Execution
                    SqlCommand msSqlComm = new SqlCommand(string.Format("SELECT * FROM {0}", StoredProcedureName));
                    msSqlComm.Connection = (SqlConnection)UniversalSqlConnection;
                    msSqlComm.CommandType = System.Data.CommandType.Text;
                    try
                    {
                        if (OpenUniversalSQLConnection())
                        {
                            SqlDataReader sqlReader = msSqlComm.ExecuteReader();
                            if (sqlReader.HasRows)
                            {
                                dt = new DataTable();
                                dt.Load(sqlReader);
                            }
                            CloseUniversalSQLConnection();
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                    break;
                    #endregion
                case DatabaseType.MYSQL:
                    #region MYSQL Command Execution
                    MySqlCommand mySqlComm = new MySqlCommand(string.Format("SELECT * FROM {0}", StoredProcedureName));
                    mySqlComm.Connection = (MySqlConnection)UniversalSqlConnection;
                    mySqlComm.CommandType = System.Data.CommandType.Text;
                    try
                    {
                        if (OpenUniversalSQLConnection())
                        {
                            MySqlDataReader mySqlReader = mySqlComm.ExecuteReader();
                            if (mySqlReader.HasRows)
                            {
                                dt = new DataTable();
                                dt.Load(mySqlReader);
                            }
                            CloseUniversalSQLConnection();
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                    break;
                    #endregion
                case DatabaseType.SQLITE:
                    #region SQLite Command Execution
                    SQLiteCommand SQLiteComm = new SQLiteCommand(string.Format("SELECT * FROM {0}", StoredProcedureName));
                    SQLiteConnection SQLiteConn = (SQLiteConnection)UniversalSqlConnection;
                    SQLiteComm.Connection = SQLiteConn;
                    SQLiteComm.CommandType = System.Data.CommandType.StoredProcedure;
                    try
                    {
                        if (OpenUniversalSQLConnection())
                        {
                            SQLiteDataReader SQLiteReader = SQLiteComm.ExecuteReader();
                            if (SQLiteReader.HasRows)
                            {
                                dt = new DataTable();
                                dt.Load(SQLiteReader);
                            }
                            CloseUniversalSQLConnection();
                        }
                    }
                    catch
                    {
                        return null;
                    }
                    finally
                    {
                        CloseUniversalSQLConnection();
                    }

                    break;
                    #endregion
                case DatabaseType.ORACLEDB:
                    #region Oracle Command Execution
                    OracleCommand oracleComm = new OracleCommand(string.Format("SELECT * FROM {0}", StoredProcedureName));
                    OracleConnection oracleConn = (OracleConnection)UniversalSqlConnection;
                    oracleComm.Connection = oracleConn;
                    oracleComm.CommandType = System.Data.CommandType.StoredProcedure;
                    try
                    {
                        if (OpenUniversalSQLConnection())
                        {
                            OracleDataReader oracleReader = oracleComm.ExecuteReader();
                            if (oracleReader.HasRows)
                            {
                                dt = new DataTable();
                                dt.Load(oracleReader);
                            }
                            CloseUniversalSQLConnection();
                        }
                    }
                    catch
                    {
                        return null;
                    }
                    finally
                    {
                        CloseUniversalSQLConnection();
                    }
                    break;
                    #endregion
                case DatabaseType.POSTGRESQL:
                    #region PostGreSQL Command Execution
                    NpgsqlCommand postGreSqlComm = new NpgsqlCommand(string.Format("SELECT * FROM {0}", StoredProcedureName));
                    NpgsqlConnection postGreSqlConn = (NpgsqlConnection)UniversalSqlConnection;
                    postGreSqlComm.Connection = postGreSqlConn;
                    postGreSqlComm.CommandType = System.Data.CommandType.StoredProcedure;
                    try
                    {
                        if (OpenUniversalSQLConnection())
                        {
                            NpgsqlDataReader postGreSqlReader = postGreSqlComm.ExecuteReader();
                            if (postGreSqlReader.HasRows)
                            {
                                dt = new DataTable();
                                dt.Load(postGreSqlReader);
                            }
                            CloseUniversalSQLConnection();
                        }
                    }
                    catch
                    {
                        return null;
                    }
                    finally
                    {
                        CloseUniversalSQLConnection();
                    }
                    break;
                    #endregion
                default:
                    throw new NotSupportedException();
            }
            return dt;
        }

        /// <summary>
        /// Executes the given SQLCommand with the appropiate tools derived from the given crudCommand.
        /// </summary>
        /// <param name="crudCmd">Can be either 'select','insert','update' or 'delete'</param>
        /// <param name="sqlComm">The prepped SQLCommand to execute</param>
        internal DataTable ExecuteSqlCommand(CRUDCommand crudCmd, object UniversalSqlCommandObject)
        {
            DataTable dt = null;
            int rowsAffected;
            switch (DataBaseType)
            {
                case DatabaseType.MSSQL:
                    #region MSSQL Command Execution
                    SqlCommand msSqlComm = (SqlCommand)UniversalSqlCommandObject;
                    msSqlComm.Connection = (SqlConnection)UniversalSqlConnection;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            switch (crudCmd)
                            {
                                case CRUDCommand.SELECT:
                                    SqlDataReader sqlReader = msSqlComm.ExecuteReader();
                                    dt = new DataTable();
                                    dt.Load(sqlReader);

                                    break;
                                case CRUDCommand.INSERT:

                                    rowsAffected = msSqlComm.ExecuteNonQuery();

                                    break;
                                case CRUDCommand.UPDATE:
                                    rowsAffected = msSqlComm.ExecuteNonQuery();

                                    break;
                                case CRUDCommand.REMOVE:
                                    rowsAffected = msSqlComm.ExecuteNonQuery();

                                    break;
                            }
                            CloseUniversalSQLConnection();
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                    break;
                    #endregion
                case DatabaseType.MYSQL:
                    #region MYSQL Command Execution
                    MySqlCommand mySqlComm = (MySqlCommand)UniversalSqlCommandObject;
                    mySqlComm.Connection = (MySqlConnection)UniversalSqlConnection;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            switch (crudCmd)
                            {
                                case CRUDCommand.SELECT:
                                    MySqlDataReader mySqlReader = mySqlComm.ExecuteReader();
                                    dt = new DataTable();
                                    dt.Load(mySqlReader);
                                    break;
                                case CRUDCommand.INSERT:
                                    rowsAffected = mySqlComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                                case CRUDCommand.UPDATE:
                                    rowsAffected = mySqlComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                                case CRUDCommand.REMOVE:
                                    rowsAffected = mySqlComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                            }
                            CloseUniversalSQLConnection();
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                    break;
                    #endregion
                case DatabaseType.SQLITE:
                    #region SQLite Command Execution
                    SQLiteCommand SQLiteComm = (SQLiteCommand)UniversalSqlCommandObject;
                    SQLiteComm.Connection = (SQLiteConnection)UniversalSqlConnection;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            switch (crudCmd)
                            {
                                case CRUDCommand.SELECT:
                                    SQLiteDataReader mySqlReader = SQLiteComm.ExecuteReader();
                                    dt = new DataTable();
                                    dt.Load(mySqlReader);
                                    break;
                                case CRUDCommand.INSERT:
                                    rowsAffected = SQLiteComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                                case CRUDCommand.UPDATE:
                                    rowsAffected = SQLiteComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                                case CRUDCommand.REMOVE:
                                    rowsAffected = SQLiteComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                            }
                            CloseUniversalSQLConnection();
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                    break;
                    #endregion
                case DatabaseType.ORACLEDB:
                    #region Oracle Command Execution
                    OracleCommand oracleComm = (OracleCommand)UniversalSqlCommandObject;
                    oracleComm.Connection = (OracleConnection)UniversalSqlConnection;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            switch (crudCmd)
                            {
                                case CRUDCommand.SELECT:
                                    OracleDataReader oracleReader = oracleComm.ExecuteReader();
                                    dt = new DataTable();
                                    dt.Load(oracleReader);
                                    break;
                                case CRUDCommand.INSERT:
                                    rowsAffected = oracleComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                                case CRUDCommand.UPDATE:
                                    rowsAffected = oracleComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                                case CRUDCommand.REMOVE:
                                    rowsAffected = oracleComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                            }
                            CloseUniversalSQLConnection();
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                    break;
                    #endregion
                case DatabaseType.POSTGRESQL:
                    #region PostGreSQL Command Execution
                    NpgsqlCommand postGreSqlComm = (NpgsqlCommand)UniversalSqlCommandObject;
                    postGreSqlComm.Connection = (NpgsqlConnection)UniversalSqlConnection;
                    if (OpenUniversalSQLConnection())
                    {
                        try
                        {
                            switch (crudCmd)
                            {
                                case CRUDCommand.SELECT:
                                    NpgsqlDataReader postGreSqlReader = postGreSqlComm.ExecuteReader();
                                    dt = new DataTable();
                                    dt.Load(postGreSqlReader);
                                    break;
                                case CRUDCommand.INSERT:
                                    rowsAffected = postGreSqlComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                                case CRUDCommand.UPDATE:
                                    rowsAffected = postGreSqlComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                                case CRUDCommand.REMOVE:
                                    rowsAffected = postGreSqlComm.ExecuteNonQuery();
                                    dt = new DataTable();
                                    dt.Columns.Add("RowsAffected", typeof(int));
                                    break;
                            }
                            CloseUniversalSQLConnection();
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                    break;
                    #endregion
                default:
                    throw new NotSupportedException();
            }
            return dt;
        }
        #endregion

        #region Database Connection methods
        /// <summary>
        /// Opens the DBHandler connection to the Universal SQL Handler's specified database type using the specified connectionstring
        /// </summary>
        /// <returns>True if connection is successfully opened, false if something went wrong</returns>
        private bool OpenUniversalSQLConnection()
        {
            bool flag = false;

            switch (DataBaseType)
            {
                case DatabaseType.MSSQL:
                    SqlConnection msSqlConn = (SqlConnection)UniversalSqlConnection;

                    try
                    {
                        if (msSqlConn.State != System.Data.ConnectionState.Open)
                        {
                            msSqlConn.Open();
                            flag = true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    break;
                case DatabaseType.MYSQL:
                    MySqlConnection mySqlConn = (MySqlConnection)UniversalSqlConnection;

                    try
                    {
                        if (mySqlConn.State != System.Data.ConnectionState.Open)
                        {
                            mySqlConn.Open();
                            flag = true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    break;
                case DatabaseType.SQLITE:
                    SQLiteConnection SQLiteConn = (SQLiteConnection)UniversalSqlConnection;

                    try
                    {
                        if (SQLiteConn.State != System.Data.ConnectionState.Open)
                        {
                            SQLiteConn.Open();
                            flag = true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    break;
                case DatabaseType.ORACLEDB:
                    OracleConnection oracleConn = (OracleConnection)UniversalSqlConnection;

                    try
                    {
                        if (oracleConn.State != System.Data.ConnectionState.Open)
                        {
                            oracleConn.Open();
                            flag = true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    break;
                case DatabaseType.POSTGRESQL:
                    NpgsqlConnection postGreSqlConn = (NpgsqlConnection)UniversalSqlConnection;

                    try
                    {
                        if (postGreSqlConn.State != System.Data.ConnectionState.Open)
                        {
                            postGreSqlConn.Open();
                            flag = true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }


            return flag;
        }

        /// <summary>
        /// Closes the DBHandler connection to the Universal SQL Handler's specified database type and database.
        /// </summary>
        /// <returns>True if connection is successfully closed, false if something went wrong</returns>
        private bool CloseUniversalSQLConnection()
        {
            bool flag = false;

            switch (DataBaseType)
            {
                case DatabaseType.MSSQL:
                    SqlConnection msSqlConn = (SqlConnection)UniversalSqlConnection;

                    if (msSqlConn.State != System.Data.ConnectionState.Closed)
                    {
                        msSqlConn.Close();
                        flag = true;
                    }

                    break;
                case DatabaseType.MYSQL:
                    MySqlConnection mySqlConn = (MySqlConnection)UniversalSqlConnection;

                    if (mySqlConn.State != System.Data.ConnectionState.Closed)
                    {
                        mySqlConn.Close();
                        flag = true;
                    }
                    break;
                case DatabaseType.SQLITE:
                    SQLiteConnection SQLiteConn = (SQLiteConnection)UniversalSqlConnection;

                    if (SQLiteConn.State != System.Data.ConnectionState.Closed)
                    {
                        SQLiteConn.Close();
                        flag = true;
                    }
                    break;
                case DatabaseType.ORACLEDB:
                    OracleConnection oracleConn = (OracleConnection)UniversalSqlConnection;

                    if (oracleConn.State != System.Data.ConnectionState.Closed)
                    {
                        oracleConn.Close();
                        flag = true;
                    }
                    break;
                case DatabaseType.POSTGRESQL:
                    NpgsqlConnection postGreSqlConn = (NpgsqlConnection)UniversalSqlConnection;

                    if (postGreSqlConn.State != System.Data.ConnectionState.Closed)
                    {
                        postGreSqlConn.Close();
                        flag = true;
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
            return flag;
        }
        #endregion
    }
}

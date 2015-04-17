using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Added usings
using System.Data;
using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace DBHandler
{
    class ObsoleteCodes
    {

        public abstract class SQLHandlerEntity
        {
            static public bool IsClosed { get; set; }

        }

        public class MSSQLHandlerEntity : SQLHandlerEntity
        {
            private static SqlConnection msSqlConn;

            public MSSQLHandlerEntity(string connectionString)
            {
                msSqlConn = new SqlConnection(connectionString);
            }


            /// <summary>
            /// Generates an insert SQL query with the needed parameters derived from the Object specified.
            /// </summary>
            /// <param name="O">Object of a type that has been registered in the internal type library of the DBHandler</param>
            /// <param name="cmdText">The string field to store the SQL Query string in</param>
            /// <returns>An array of SQL Parameters to be added to the SQLCommand</returns>
            public SqlParameter[] GenerateParameters(CRUDCommand crudCmd, Object O, out string cmdText)
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

                if (DataBaseHandler.RegisteredTypes.ContainsKey(O.GetType()))
                {
                    // Cast the object to the base class

                    DBHandlerEntity dbhe = (DBHandlerEntity)O;
                    Dictionary<string, object> data = dbhe.GetData;

                    // Get the specific column properties needed for the parameters to work
                    Dictionary<string, KeyValuePair<SqlDbType, int>> ColumnSpecifics = GetColumnSpecifics(DataBaseHandler.RegisteredTypes[O.GetType()]);

                    // Iterate through all the keys/columns and generate all needed parameters
                    foreach (string key in data.Keys)
                    {
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

                        // Create the parameter and assign the appropiate value
                        SqlParameter keyParam = new SqlParameter(string.Format("@{0}", key), ColumnSpecifics[key].Key, ColumnSpecifics[key].Value, key) { Value = data[key] };

                        sqlParamColl.Add(keyParam);
                    }
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
                SqlCommand sqlComm = new SqlCommand(commandText, msSqlConn);
                sqlComm.Parameters.Add(new SqlParameter(string.Format("@TABLE_NAME"), SqlDbType.NVarChar, 128, "TABLE_NAME") { Value = tableName });
                DataTable resultsDT = null;

                try
                {
                    // Execute the command and load the results into a datatable for ease of usage
                    if (msSqlConn.State != ConnectionState.Open)
                    {
                        msSqlConn.Open();
                    }

                    SqlDataReader sqlReader = sqlComm.ExecuteReader();

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


            #region Database Executables
            /// <summary>
            /// Executes the specified SQL Command as a stored procedure and returns the ROW ID of the row added/edited
            /// </summary>
            /// <param name="sqlComm">The PREPPED SQL Command</param>
            /// <param name="rowID">The ROW ID returned by the query</param>
            /// <returns>Wether successful or not</returns>
            internal bool ExecuteStoredProcedure(SqlCommand sqlComm, out int rowID)
            {
                try
                {
                    int rowid = 0;
                    sqlComm.Connection = msSqlConn;
                    sqlComm.CommandType = System.Data.CommandType.StoredProcedure;

                    if (msSqlConn.State != System.Data.ConnectionState.Open)
                    {
                        msSqlConn.Open();
                        IsClosed = false;
                    }

                    rowid = (int)sqlComm.ExecuteScalar();


                    if (msSqlConn.State != System.Data.ConnectionState.Closed)
                    {
                        msSqlConn.Close();
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
            internal bool ExecuteStoredProcedure(SqlCommand sqlComm)
            {
                sqlComm.Connection = msSqlConn;
                sqlComm.CommandType = System.Data.CommandType.StoredProcedure;
                try
                {
                    if (msSqlConn.State != System.Data.ConnectionState.Open)
                    {
                        msSqlConn.Open();
                        IsClosed = false;
                    }

                    sqlComm.ExecuteScalar();


                    if (msSqlConn.State != System.Data.ConnectionState.Closed)
                    {
                        msSqlConn.Close();
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
            internal DataTable ExecuteStoredProcedure(string StoredProcedureName)
            {
                SqlCommand sqlComm = new SqlCommand(string.Format("SELECT * FROM {0}", StoredProcedureName));
                sqlComm.Connection = msSqlConn;
                sqlComm.CommandType = System.Data.CommandType.Text;
                DataTable dt = null;
                try
                {
                    if (msSqlConn.State != System.Data.ConnectionState.Open)
                    {
                        msSqlConn.Open();
                        IsClosed = false;
                    }

                    SqlDataReader sqlReader = sqlComm.ExecuteReader();
                    if (sqlReader.HasRows)
                    {
                        dt = new DataTable();
                        dt.Load(sqlReader);
                    }


                    if (msSqlConn.State != System.Data.ConnectionState.Closed)
                    {
                        msSqlConn.Close();
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
            internal DataTable ExecuteSqlCommand(string crudCmd, SqlCommand sqlComm)
            {
                sqlComm.Connection = msSqlConn;
                DataTable dt = null;
                if (msSqlConn.State != System.Data.ConnectionState.Open)
                {
                    msSqlConn.Open();
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

                if (msSqlConn.State != System.Data.ConnectionState.Closed)
                {
                    msSqlConn.Close();
                }
                return dt;
            }
            #endregion

        }

        public class MYSQLHandlerEntity : SQLHandlerEntity
        {
            private static MySqlConnection mySqlConn;

            public MYSQLHandlerEntity(string connectionString)
            {
                mySqlConn = new MySqlConnection(connectionString);
            }

            /// <summary>
            /// Generates an insert SQL query with the needed parameters derived from the Object specified.
            /// </summary>
            /// <param name="O">Object of a type that has been registered in the internal type library of the DBHandler</param>
            /// <param name="cmdText">The string field to store the SQL Query string in</param>
            /// <returns>An array of SQL Parameters to be added to the SQLCommand</returns>
            public MySqlParameter[] GenerateParameters(CRUDCommand crudCmd, Object O, out string cmdText)
            {
                List<MySqlParameter> sqlParamColl = new List<MySqlParameter>();
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

                if (DataBaseHandler.RegisteredTypes.ContainsKey(O.GetType()))
                {
                    // Cast the object to the base class

                    DBHandlerEntity dbhe = (DBHandlerEntity)O;
                    Dictionary<string, object> data = dbhe.GetData;

                    // Get the specific column properties needed for the parameters to work
                    Dictionary<string, KeyValuePair<MySqlDbType, int>> ColumnSpecifics = GetColumnSpecifics(DataBaseHandler.RegisteredTypes[O.GetType()]);

                    // Iterate through all the keys/columns and generate all needed parameters
                    foreach (string key in data.Keys)
                    {
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

                        // Create the parameter and assign the appropiate value
                        MySqlParameter keyParam = new MySqlParameter(string.Format("@{0}", key), ColumnSpecifics[key].Key, ColumnSpecifics[key].Value, key) { Value = data[key] };

                        sqlParamColl.Add(keyParam);
                    }
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
            private Dictionary<string, KeyValuePair<MySqlDbType, int>> GetColumnSpecifics(string tableName)
            {
                Dictionary<string, KeyValuePair<MySqlDbType, int>> columnSpecifics = null;

                //select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'myTable'
                string commandText = string.Format("describe {0}", tableName);
                MySqlCommand sqlComm = new MySqlCommand(commandText, mySqlConn);
                DataTable resultsDT = null;

                try
                {
                    // Execute the command and load the results into a datatable for ease of usage
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
                catch (Exception ex)
                {
                    throw ex;
                }

                if (resultsDT != null)
                {
                    if (resultsDT.Rows.Count > 0)
                    {
                        columnSpecifics = new Dictionary<string, KeyValuePair<MySqlDbType, int>>();
                        foreach (DataRow dr in resultsDT.Rows)
                        {
                            string typeAndLength = dr["Type"].ToString();
                            int oBracket = typeAndLength.IndexOf('(');
                            int cBracket = typeAndLength.IndexOf(')');
                            int columnLength = int.Parse(typeAndLength.Substring(oBracket, (cBracket - oBracket)));

                            switch (typeAndLength.Substring(0, oBracket))
                            {
                                case "int":
                                    columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<MySqlDbType, int>(MySqlDbType.Int32, columnLength));
                                    break;
                                case "varchar":
                                    columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<MySqlDbType, int>(MySqlDbType.VarChar, columnLength));
                                    break;
                                case "nvarchar":
                                    columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<MySqlDbType, int>(MySqlDbType.VarString, columnLength));
                                    break;
                                case "decimal":
                                    columnSpecifics.Add(dr["Field"].ToString(), new KeyValuePair<MySqlDbType, int>(MySqlDbType.Decimal, columnLength));
                                    break;
                            }
                        }
                    }
                }
                return columnSpecifics;
            }

            #region Database Executables
            /// <summary>
            /// Executes the specified SQL Command as a stored procedure and returns the ROW ID of the row added/edited
            /// </summary>
            /// <param name="mySqlComm">The PREPPED SQL Command</param>
            /// <param name="rowID">The ROW ID returned by the query</param>
            /// <returns>Wether successful or not</returns>
            internal bool ExecuteStoredProcedure(MySqlCommand mySqlComm, out int rowID)
            {
                try
                {
                    int rowid = 0;
                    mySqlComm.Connection = mySqlConn;
                    mySqlComm.CommandType = System.Data.CommandType.StoredProcedure;

                    if (mySqlConn.State != System.Data.ConnectionState.Open)
                    {
                        mySqlConn.Open();
                        IsClosed = false;
                    }

                    rowid = (int)mySqlComm.ExecuteScalar();


                    if (mySqlConn.State != System.Data.ConnectionState.Closed)
                    {
                        mySqlConn.Close();
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
            /// <param name="mySqlComm">The PREPPED SQL Command</param>
            internal bool ExecuteStoredProcedure(MySqlCommand mySqlComm)
            {
                mySqlComm.Connection = mySqlConn;
                mySqlComm.CommandType = System.Data.CommandType.StoredProcedure;
                try
                {
                    if (mySqlConn.State != System.Data.ConnectionState.Open)
                    {
                        mySqlConn.Open();
                        IsClosed = false;
                    }

                    mySqlComm.ExecuteScalar();


                    if (mySqlConn.State != System.Data.ConnectionState.Closed)
                    {
                        mySqlConn.Close();
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
            internal DataTable ExecuteStoredProcedure(string StoredProcedureName)
            {
                MySqlCommand mySqlComm = new MySqlCommand(string.Format("SELECT * FROM {0}", StoredProcedureName));
                mySqlComm.Connection = mySqlConn;
                mySqlComm.CommandType = System.Data.CommandType.Text;
                DataTable dt = null;
                try
                {
                    if (mySqlConn.State != System.Data.ConnectionState.Open)
                    {
                        mySqlConn.Open();
                        IsClosed = false;
                    }

                    MySqlDataReader mySqlReader = mySqlComm.ExecuteReader();
                    if (mySqlReader.HasRows)
                    {
                        dt = new DataTable();
                        dt.Load(mySqlReader);
                    }


                    if (mySqlConn.State != System.Data.ConnectionState.Closed)
                    {
                        mySqlConn.Close();
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
            /// <param name="mySqlComm">The prepped SQLCommand to execute</param>
            internal DataTable ExecuteSqlCommand(string crudCmd, MySqlCommand mySqlComm)
            {
                mySqlComm.Connection = mySqlConn;
                DataTable dt = null;
                if (mySqlConn.State != System.Data.ConnectionState.Open)
                {
                    mySqlConn.Open();
                    IsClosed = false;
                }

                switch (crudCmd)
                {
                    case "select":
                        MySqlDataReader mySqlReader = mySqlComm.ExecuteReader();
                        dt = new DataTable();
                        dt.Load(mySqlReader);

                        break;
                    case "insert":

                        int rowsAffected = mySqlComm.ExecuteNonQuery();

                        break;
                    case "update":
                        rowsAffected = mySqlComm.ExecuteNonQuery();

                        break;
                    case "remove":
                        rowsAffected = mySqlComm.ExecuteNonQuery();

                        break;
                }

                if (mySqlConn.State != System.Data.ConnectionState.Closed)
                {
                    mySqlConn.Close();
                }
                return dt;
            }
            #endregion

        }

    }
}

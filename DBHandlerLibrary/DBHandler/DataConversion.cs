using System;
using System.Collections.Generic;
using System.Data;

namespace DBHandler
{
    public partial class DataBaseHandler
    {
        /// <summary>
        /// Contains all the DBHandler Conversion classes and methods
        /// </summary>
        public static class Convert
        {
            /// <summary>
            /// Convert an object to a DataTable and vice-versa
            /// </summary>
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

                    if (DataBaseHandler.RegisteredTypes.ContainsKey(objectType))
                    {
                        objToReturn = new Object();
                        DBHandlerEntity dbhe = (DBHandlerEntity)objToReturn;

                        if (dt.Rows.Count == 1)
                        {
                            Dictionary<string, object> objDetails = new Dictionary<string, object>();
                            foreach (DataColumn column in dt.Columns)
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
                /// Convert the specified object which is registered in the typelibrary of the DBHandler into a DataTable 
                /// </summary>
                /// <param name="o">The object to convert to a DataTable</param>
                /// <returns>DataTable with the inherited keys as column names and their respective values as a row</returns>
                public static DataTable ToDataTable(Object o)
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

                    foreach (string key in objectData.Keys)
                    {
                        dt.Columns.Add(key);
                    }

                    dt.Rows.Add(objectData.Values);

                    return dt;
                }

                /// <summary>
                /// Converts the specified array of objects which is registered in the typelibrary of the DBHandler into a DataTable  
                /// </summary>
                /// <param name="o">The array of objects to convert to a DataTable</param>
                /// <returns>DataTable with the inherited keys as column names and the object values as rows</returns>
                public static DataTable ToDataTable(Object[] o)
                {
                    List<DBHandlerEntity> dbheObjects = new List<DBHandlerEntity>();
                    if (DataBaseHandler.RegisteredTypes.ContainsKey(o.GetType()))
                    {
                        foreach(Object obj in o)
                        {
                            dbheObjects.Add((DBHandlerEntity)obj);
                        }
                    }
                    else
                    {
                        return null;
                    }
                    DataTable dt = new DataTable();
                    bool columnsAdded = false;

                    foreach(DBHandlerEntity dbhe in dbheObjects)
                    {
                        Dictionary<string, object> objectData = dbhe.GetData;
                        if (!columnsAdded)
                        {
                            foreach (string key in objectData.Keys)
                            {
                                dt.Columns.Add(key);
                            }
                            columnsAdded = true;
                        }

                        dt.Rows.Add(objectData.Values);
                    }

                    return dt;
                }

                /// <summary>
                /// Converts the specified object which is registered in the typelibrary of the DBHandler into a DataRow
                /// </summary>
                /// <param name="o">The object to convert to a DataRow</param>
                /// <returns>DataRow with the inherited keys as column names and the object valus as the row values</returns>
                public static DataRow ToDataRow(Object o)
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
                    DataTable dt = null;
                    
                    foreach (string key in objectData.Keys)
                    {
                        dt.Columns.Add(key);
                    }

                    return dt.Rows.Add(objectData.Values);
                }
            }
        }
    }
}

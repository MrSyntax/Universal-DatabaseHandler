using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHandler
{
    public partial class DataBaseHandler
    {
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

                    foreach (string key in objectData.Keys)
                    {
                        dt.Columns.Add(key);
                    }

                    dt.Rows.Add(objectData.Values);

                    return dt;
                }
                public static DataTable ToDataTable(int SYSID, Object[] o)
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

                public static DataRow ToDataRow(int SYSID, Object o)
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

                    // TODO fix protection level bug
                    //DataRow dr = new DataRow() { ItemArray = objectData.Values.ToArray() };

                    //return dr;
                    return null;
                }
            }
        }
    }
}

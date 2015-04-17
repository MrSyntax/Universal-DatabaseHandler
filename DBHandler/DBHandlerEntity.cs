using System.Collections.Generic;
using System;
using System.Linq;

// Added usings
using System.Data;
using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace DBHandler
{
    public abstract class DBHandlerEntity
    {
        /// <summary>
        /// Gets all the data that needs to be used with the linked DB Table. Note: Needs to be handled internally within the custom class. Keys should be equal to the DB column names.
        /// </summary>
        public abstract Dictionary<string, object> GetData { get; }

        /// <summary>
        /// Sets the specified data to the class. Note: Needs to be handled internally within the custom class. Keys should be equal to the DB column names.
        /// </summary>
        public abstract Dictionary<string, object> SetData { set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHandler
{
    public partial class DataBaseHandler
    {
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
    }
}

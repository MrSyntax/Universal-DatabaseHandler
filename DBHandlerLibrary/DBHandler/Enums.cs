
namespace DBHandler
{
    /// <summary>
    /// Result types of object Type registration within the DBHandler
    /// </summary>
    public enum TypeRegResult
    {
        Successfull = 1,
        Unsuccessfull = 2,
        TypeAlreadyExists = 3,
        DBTableAlreadyExists = 4,
        TypeNonExistant = 5,
        DBTableNameNonExistant = 6,
        Unknown
    }

    /// <summary>
    /// Database Manipulation Command Types
    /// </summary>
    public enum CRUDCommand
    {
        SELECT = 1,
        INSERT = 2,
        UPDATE = 3,
        REMOVE
    }

    /// <summary>
    /// Supported Database Engine Types
    /// </summary>
    public enum DatabaseType
    {
        MSSQL,
        MYSQL,
        POSTGRESQL,
        ORACLEDB,
        SQLITE,
        MONGODB,
        DB2,
        SYBASE,
        TERADATA,
        CASSANDRA
    }
}


namespace DBHandler
{
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

    public enum CRUDCommand
    {
        SELECT = 1,
        INSERT = 2,
        UPDATE = 3,
        REMOVE
    }

    public enum DatabaseType
    {
        MSSQL,
        MYSQL
    }
}

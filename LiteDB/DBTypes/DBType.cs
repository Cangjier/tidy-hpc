namespace TidyHPC.LiteDB.DBTypes;
internal interface DBType
{
    Task<ulong> GetHashCode(Database db);

    Task<bool> Equals(Database db,DBType other);
    
    int Read(byte[] buffer,int offset);

    int Write(byte[] buffer,int offset);

    //int GetSize();

    static abstract int GetSize();
}

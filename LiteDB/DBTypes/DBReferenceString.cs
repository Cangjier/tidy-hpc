using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;
internal struct DBReferenceString:DBType
{
    public DBReferenceString(long stringAddress)
    {
        StringAddress = stringAddress;
    }

    public long StringAddress;

    public readonly async Task<bool> Equals(Database db, DBType other)
    {
        if(other is DBReferenceString refString)
        {
            var stringA = await db.StringHashSet.Read(StringAddress);
            var stringB = await db.StringHashSet.Read(refString.StringAddress);
            return stringA == stringB;
        }
        return false;
    }

    public async Task<ulong> GetHashCode(Database db)
    {
        var stringValue =await db.StringHashSet.Read(StringAddress);
        return await HashService.GetHashCode(stringValue);
    }

    public int Read(byte[] buffer, int offset)
    {
        StringAddress = BitConverter.ToInt64(buffer, offset);
        return sizeof(long);
    }

    public int Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(StringAddress).CopyTo(buffer, offset);
        return sizeof(long);
    }

    public static int GetSize()
    {
        return sizeof(long);
    }

    public override string ToString()
    {
        return $"DBReferenceString: {StringAddress}";
    }
}
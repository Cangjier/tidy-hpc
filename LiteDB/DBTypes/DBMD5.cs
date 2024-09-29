
using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;
internal struct DBMD5 : DBType
{
    public DBMD5(byte[] value)
    {
        Value = value;
    }

    public byte[] Value;

    public async Task<bool> Equals(Database db, DBType other)
    {
        await Task.CompletedTask;
        if(other is DBMD5 md5)
        {
            return Value.SequenceEqual(md5.Value);
        }
        return false;
    }

    public async Task<ulong> GetHashCode(Database db)
    {
        return await HashService.GetHashCode(Value);
    }

    public int Read(byte[] buffer, int offset)
    {
        Value = buffer[offset..(offset + 16)];
        return 16;
    }

    public int Write(byte[] buffer, int offset)
    {
        Value.CopyTo(buffer, offset);
        return 16;
    }

    public static int GetSize()
    {
        return 16;
    }

    public override string ToString()
    {
        return Util.BytesToHexString(Value);
    }
}

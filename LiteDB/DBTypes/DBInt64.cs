using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;

internal struct DBInt64 : DBType
{
    public DBInt64(long value)
    {
        Value = value;
    }

    public long Value;

    public async Task<bool> Equals(Database db, DBType other)
    {
        await Task.CompletedTask;
        if (other is DBInt64 int64)
        {
            return Value == int64.Value;
        }
        return false;
    }

    public async Task<ulong> GetHashCode(Database db)
    {
        return await HashService.GetHashCode(Value);
    }

    public static int GetSize()
    {
        return sizeof(long);
    }

    public int Read(byte[] buffer, int offset)
    {
        Value = BitConverter.ToInt64(buffer, offset);
        return sizeof(long);
    }

    public int Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(Value).CopyTo(buffer, offset);
        return sizeof(long);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

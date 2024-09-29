using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;

internal struct DBDateTime : DBType
{
    public DBDateTime(long value)
    {
        Value = value;
    }

    public long Value;

    public async Task<bool> Equals(Database db, DBType other)
    {
        await Task.CompletedTask;
        if (other is DBDateTime guid)
        {
            return Value == guid.Value;
        }
        return false;
    }

    public async Task<ulong> GetHashCode(Database db)
    {
        return await HashService.GetHashCode(Value);
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

    public static int GetSize()
    {
        return sizeof(long);
    }

    override public string ToString()
    {
        return new DateTime(Value).ToString();
    }
}

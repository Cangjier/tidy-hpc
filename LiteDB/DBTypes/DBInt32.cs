using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;

internal struct DBInt32:DBType
{
    public DBInt32(int value)
    {
        Value = value;
    }

    public int Value;

    public async Task<bool> Equals(Database db, DBType other)
    {
        await Task.CompletedTask;
        if(other is DBInt32 int32)
        {
            return Value == int32.Value;
        }
        return false;
    }

    public async Task<ulong> GetHashCode(Database db)
    {
        return await HashService.GetHashCode(Value);
    }

    public static int GetSize()
    {
        return sizeof(int);
    }

    public int Read(byte[] buffer, int offset)
    {
        Value = BitConverter.ToInt32(buffer, offset);
        return sizeof(int);
    }

    public int Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(Value).CopyTo(buffer, offset);
        return sizeof(int);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

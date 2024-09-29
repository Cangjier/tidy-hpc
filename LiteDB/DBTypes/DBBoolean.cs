using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;
internal struct DBBoolean : DBType
{
    public DBBoolean(bool value)
    {
        Value = value;
    }

    public bool Value;

    public async Task<bool> Equals(Database db, DBType other)
    {
        await Task.CompletedTask;
        if (other is DBBoolean otherValue)
        {
            return Value == otherValue.Value;
        }
        return false;
    }

    public async Task<ulong> GetHashCode(Database db)
    {
        return await HashService.GetHashCode(Value);
    }

    public static int GetSize()
    {
        return sizeof(bool);
    }

    public int Read(byte[] buffer, int offset)
    {
        Value = BitConverter.ToBoolean(buffer, offset);
        return sizeof(bool);
    }

    public int Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(Value).CopyTo(buffer, offset);
        return sizeof(bool);
    }

    public override string ToString()
    {
        return Value ? "true" : "false";
    }
}

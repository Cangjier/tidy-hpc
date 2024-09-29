using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;
internal struct DBByte : DBType
{
    public DBByte(byte value)
    {
        Value = value;
    }

    public byte Value;

    public async Task<bool> Equals(Database db, DBType other)
    {
        await Task.CompletedTask;
        if (other is DBByte otherValue)
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
        return sizeof(byte);
    }

    public int Read(byte[] buffer, int offset)
    {
        Value = buffer[offset];
        return sizeof(byte);
    }

    public int Write(byte[] buffer, int offset)
    {
        buffer[offset] = Value;
        return sizeof(byte);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
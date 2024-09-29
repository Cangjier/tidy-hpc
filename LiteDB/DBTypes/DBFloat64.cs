using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;
internal struct DBFloat64 : DBType
{
    public DBFloat64(double value)
    {
        Value = value;
    }

    public double Value;

    public async Task<bool> Equals(Database db, DBType other)
    {
        await Task.CompletedTask;
        if (other is DBFloat64 value)
        {
            return Value == value.Value;
        }
        return false;
    }

    public async Task<ulong> GetHashCode(Database db)
    {
        return await HashService.GetHashCode(Value);
    }

    public static int GetSize()
    {
        return sizeof(double);
    }

    public int Read(byte[] buffer, int offset)
    {
        Value = BitConverter.ToDouble(buffer, offset);
        return sizeof(float);
    }

    public int Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(Value).CopyTo(buffer, offset);
        return sizeof(double);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
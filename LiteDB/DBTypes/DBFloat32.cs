using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;
internal struct DBFloat32 : DBType
{
    public DBFloat32(float value)
    {
        Value = value;
    }

    public float Value;

    public async Task<bool> Equals(Database db, DBType other)
    {
        await Task.CompletedTask;
        if (other is DBFloat32 value)
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
        return sizeof(float);
    }

    public int Read(byte[] buffer, int offset)
    {
        Value = BitConverter.ToSingle(buffer, offset);
        return sizeof(float);
    }

    public int Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(Value).CopyTo(buffer, offset);
        return sizeof(float);
    }

    override public string ToString()
    {
        return Value.ToString();
    }
}
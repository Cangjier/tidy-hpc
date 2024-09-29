using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;
internal struct DBString<TLength> : DBType
    where TLength : IInterger
{
    public DBString(string value)
    {
        Value = value;
    }

    public string Value;

    public async Task<bool> Equals(Database db, DBType other)
    {
        await Task.CompletedTask;
        if (other is DBString<TLength> str)
        {
            return Value == str.Value;
        }
        return false;
    }

    public async Task<ulong> GetHashCode(Database db)
    {
        return await HashService.GetHashCode(Value);
    }

    public int Read(byte[] buffer, int offset)
    {
        Value = buffer.DeserializeString(ref offset, GetSize());
        return GetSize();
    }

    public int Write(byte[] buffer, int offset)
    {
        buffer.SerializeRef(Value, ref offset, GetSize());
        return GetSize();
    }

    public static int GetSize()
    {
        return TLength.GetValue();
    }

    public override string ToString()
    {
        return Value;
    }
}

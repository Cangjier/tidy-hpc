using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.DBTypes;

internal struct DBGuid:DBType
{
    public DBGuid(Guid value)
    {
        Value = value;
    }

    public Guid Value;

    public async Task<bool> Equals(Database db, DBType other)
    {
        await Task.CompletedTask;
        if(other is DBGuid guid)
        {
            return Value == guid.Value;
        }
        return false;
    }

    public async Task<ulong> GetHashCode(Database db)
    {
        return await HashService.GetHashCode(Value.ToByteArray());
    }

    public int Read(byte[] buffer, int offset)
    {
        Value = new Guid(buffer[offset..(offset + 16)]);
        return 16;
    }

    public int Write(byte[] buffer, int offset)
    {
        Value.ToByteArray().CopyTo(buffer, offset);
        return 16;
    }

    public static int GetSize()
    {
        return 16;
    }

    override public string ToString()
    {
        return Value.ToString();
    }
}

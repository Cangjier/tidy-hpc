using TidyHPC.LiteDB;
using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.BasicValues;
/// <inheritdoc/>
public struct Int32Value : IValue<Int32Value>
{
    /// <inheritdoc/>
    public int Value;

    /// <inheritdoc/>
    public int Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(Value).CopyTo(buffer, offset);
        return sizeof(int);
    }

    /// <inheritdoc/>
    public int Read(byte[] buffer, int offset)
    {
        Value = BitConverter.ToInt32(buffer, offset);
        return sizeof(int);
    }

    /// <inheritdoc/>
    public void SetEmpty()
    {
        Value = 0;
    }

    /// <inheritdoc/>
    public bool IsEmpty()
    {
        return Value == 0;
    }

    /// <inheritdoc/>
    public static int GetSize()
    {
        return sizeof(int);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <inheritdoc/>
    public async Task<ulong> GetHashCode(Database database)
    {
        return await HashService.GetHashCode(Value);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Int32Value other && other.Value == Value;
    }

    /// <inheritdoc/>
    public static implicit operator Int32Value(int value)
    {
        return new Int32Value { Value = value };
    }

    /// <inheritdoc/>
    public static implicit operator int(Int32Value value)
    {
        return value.Value;
    }
}
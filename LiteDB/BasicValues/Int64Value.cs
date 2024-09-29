using TidyHPC.LiteDB;
using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.BasicValues;
/// <inheritdoc/>
public struct Int64Value : IValue<Int64Value>
{
    /// <summary>
    /// Value
    /// </summary>
    public long Value;

    /// <inheritdoc/>
    public int Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(Value).CopyTo(buffer, offset);
        return sizeof(long);
    }

    /// <inheritdoc/>
    public int Read(byte[] buffer, int offset)
    {
        Value = BitConverter.ToInt64(buffer, offset);
        return sizeof(long);
    }

    /// <inheritdoc/>
    public void SetEmpty()
    {
        Value = 0;
    }

    ///<inheritdoc/>
    public bool IsEmpty()
    {
        return Value == 0;
    }

    /// <inheritdoc/>
    public static int GetSize()
    {
        return sizeof(long);
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
        return obj is Int64Value other && other.Value == Value;
    }

    /// <summary>
    /// Implicit Convert to HashValue_Long
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Int64Value(long value)
    {
        return new Int64Value { Value = value };
    }

    /// <summary>
    /// Implicit Convert to long
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator long(Int64Value value)
    {
        return value.Value;
    }


    /// <summary>
    /// Convert to string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Value.ToString();
    }
}
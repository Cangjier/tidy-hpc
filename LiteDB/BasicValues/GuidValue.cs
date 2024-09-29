using TidyHPC.LiteDB;
using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.BasicValues;
/// <inheritdoc/>
public struct GuidValue : IValue<GuidValue>
{
    /// <summary>
    /// Value
    /// </summary>
    public Guid Value;

    /// <inheritdoc/>
    public int Write(byte[] buffer, int offset)
    {
        Value.ToByteArray().CopyTo(buffer, offset);
        return 16;
    }

    /// <inheritdoc/>
    public int Read(byte[] buffer, int offset)
    {
        Value = new Guid(buffer.AsSpan(offset, 16));
        return 16;
    }

    /// <inheritdoc/>
    public void SetEmpty()
    {
        Value = Guid.Empty;
    }

    ///<inheritdoc/>
    public bool IsEmpty()
    {
        return Value == Guid.Empty;
    }

    /// <inheritdoc/>
    public static int GetSize()
    {
        return 16;
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
        return obj is GuidValue other && other.Value == Value;
    }

    /// <summary>
    /// Implicit Convert to HashValue_Guid
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator GuidValue(Guid value)
    {
        return new GuidValue { Value = value };
    }

    /// <summary>
    /// Implicit Convert to Guid
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Guid(GuidValue value)
    {
        return value.Value;
    }
}
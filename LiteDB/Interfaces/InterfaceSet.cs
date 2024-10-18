using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.Blocks;
using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.Metas2;

/// <summary>
/// 接口集合
/// </summary>
public class InterfaceSet
{
    private HashTable<Int64Value> Target { get; } = new();

    /// <summary>
    /// 接口集合的大小
    /// </summary>
    public const int Size = 64 * 4 * sizeof(long);// 8192

    /// <summary>
    /// 接口集合
    /// </summary>
    /// <param name="address"></param>
    /// <param name="blockSize"></param>
    public InterfaceSet(long address)
    {
        Target.SetAddress(address, HashNode<Int64Value>.Size, Size);
    }

    public  async Task<bool> Contains(Database db,string fullName)
    {
        return await Target.Contains(db, await HashService.GetHashCode(fullName), async interfaceAddress =>
        {
            return await db.Cache.StatisticalBlockPool.Use(async block =>
             {
                 block.SetByRecordAddress(interfaceAddress, InterfaceRecord.Size);
                 return await block.RecordVisitor.Read<bool>(db, interfaceAddress, async bytes =>
                 {
                     return InterfaceRecord.Parse(bytes).FullName == fullName;
                 });
             });
        });
    }
}

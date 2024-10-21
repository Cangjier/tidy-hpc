using TidyHPC.Common;
using TidyHPC.LiteDB.Arrays;
using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.Layouts;
using TidyHPC.Loggers;

namespace TidyHPC.LiteDB.Hashes;

/// <summary>
/// 小型的Hash表
/// </summary>
/// <typeparam name="THashValue"></typeparam>
public class SmallHashTable<THashValue>:HashTable<THashValue>
    where THashValue : struct, IValue<THashValue>
{
    /// <inheritdoc/>
    public override void SetAddress(long address)
    {
        base.SetAddress(address, HashNode<THashValue>.Size, HashRecord<THashValue>.Size);
    }
}

/// <summary>
/// Hash Table
/// <para>hash code -> (hashCode%recordCount) record index</para>
/// <para>record:{hashCode:int,value:long,nextHashRecordAddress:long}</para>
/// </summary>
public class HashTable<THashValue> : ArrayLayout
     where THashValue : struct, IValue<THashValue>
{
    /// <summary>
    /// 设置必要信息
    /// </summary>
    /// <param name="address"></param>
    public virtual void SetAddress(long address)
    {
        base.SetAddress(address, HashNode<THashValue>.Size, Database.BlockSize);
    }

    private int GetNodeIndex(ulong globalIndex)
    {
        return (int)(globalIndex % (uint)RecordCount);
    }

    private ulong GetNextHashCode(ulong globalIndex)
    {
        return globalIndex / (uint)RecordCount;
    }

    /// <summary>
    /// 获取入口
    /// </summary>
    /// <param name="db"></param>
    /// <param name="globalIndex"></param>
    /// <returns></returns>
    private async Task<HashNode<THashValue>> GetEntry(Database db, ulong globalIndex)
    {
        int recordIndex = 0;
        try
        {
            HashNode<THashValue> result = new();
            recordIndex = GetNodeIndex(globalIndex);
            await RecordVisitor.ReadByIndex(db, recordIndex, buffer =>
            {
                result.Read(buffer, 0);
            });
            return result;
        }
        catch
        {
            Logger.Error($"recordIndex:{recordIndex},globalIndex:{globalIndex},BlockAddress:{Address},RecordCount:{RecordCount},RecordSize:{RecordSize}");
            throw;
        }
        
    }

    /// <summary>
    /// 清除掉Entry上的数据
    /// </summary>
    /// <param name="db"></param>
    /// <param name="globalIndex"></param>
    /// <returns></returns>
    private async Task DeleteEntry(Database db, ulong globalIndex)
    {

        int recordIndex = GetNodeIndex(globalIndex);
        try
        {
            await RecordVisitor.UpdateByIndex(db, recordIndex, buffer =>
            {
                HashNode<THashValue> result = new();
                result.Read(buffer, 0);
                result.HashCode = 0;
                result.Value.SetEmpty();
                result.Write(buffer, 0);
                return true;
            });
        }
        catch
        {
            Logger.Error($"RecordIndex:{recordIndex},globalIndex:{globalIndex},BlockAddress:{Address},RecordCount:{RecordCount},RecordSize:{RecordSize}");
            throw;
        }
    }

    /// <summary>
    /// 更新入口
    /// </summary>
    /// <param name="db"></param>
    /// <param name="globalIndex"></param>
    /// <param name="onNode"></param>
    /// <returns></returns>
    private async Task UpdateEntry(Database db, ulong globalIndex, Func<HashNode<THashValue>, Task<(bool, HashNode<THashValue>)>> onNode)
    {
        int recordIndex = GetNodeIndex(globalIndex);
        try
        {
            await RecordVisitor.UpdateByIndex(db, recordIndex, async buffer =>
            {
                var node = new HashNode<THashValue>();
                node.Read(buffer, 0);
                if ((await onNode(node)) is (true, HashNode<THashValue> newNode))
                {
                    newNode.Write(buffer, 0);
                    return true;
                }
                return false;
            });
        }
        catch
        {
            Logger.Error($"RecordIndex:{recordIndex},globalIndex={globalIndex},BlockAddress:{Address},RecordCount:{RecordCount},RecordSize:{RecordSize}");
            throw;
        }
    }

    /// <summary>
    /// 更新入口
    /// </summary>
    /// <param name="db"></param>
    /// <param name="globalIndex"></param>
    /// <param name="onNode"></param>
    /// <returns></returns>
    private async Task UpdateEntry(Database db, ulong globalIndex, Func<HashNode<THashValue>, (bool, HashNode<THashValue>)> onNode)
        => await UpdateEntry(db, globalIndex, item => Task.FromResult(onNode(item)));

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="equals"></param>
    /// <returns></returns>
    public async Task<HashResult<THashValue>> Get(Database db, ulong hashCode, Func<THashValue, Task<bool>> equals)
    {
        HashResult<THashValue> result = new();
        ulong globalIndex = hashCode;
        var node = await GetEntry(db, globalIndex);
        globalIndex = GetNextHashCode(globalIndex);

        if (node.HashCode == hashCode && await equals(node.Value))
        {
            result.Success = true;
            result.Value = node.Value;
            return result;
        }

        HashBlock<THashValue> hashBlock = new();
        while (true)
        {
            if (globalIndex == 0)
            {
                //如果是0，表示已经到了最后一个节点
                //直接读取数组
                if(node.ArrayRecordAddress == 0)
                {
                    break;
                }
                var arrayProcessor = new ArrayProcessor<THashValue>(db, node.ArrayRecordAddress);
                Ref<THashValue> outValue = new(default);
                if(await arrayProcessor.TryFind(equals, outValue))
                {
                    result.Success = true;
                    result.Value = outValue.Value;
                }
                break;
            }
            else
            {
                if (node.NextRecordAddress == 0)
                {
                    break;
                }
                var nextBlockAddress = Database.GetBlockAddress(node.NextRecordAddress);
                hashBlock.Set(nextBlockAddress);
                try
                {
                    await hashBlock.RecordVisitor.Read(db, node.NextRecordAddress, buffer =>
                    {
                        var hashRecord = new HashRecord<THashValue>(buffer, 0);
                        node = hashRecord.GetNode(globalIndex);
                        globalIndex /= HashNode<THashValue>.HashRecordNodeCount;
                    });
                }
                catch
                {
                    Logger.Error($"NextBlockAddress:{nextBlockAddress},Node.NextRecordAddress:{node.NextRecordAddress}," +
                        $"hashBlock.RecordSize:{hashBlock.RecordSize},hashBlock.RecordCount={hashBlock.RecordCount}");
                    throw;
                }
                if (node.HashCode == hashCode && await equals(node.Value))
                {
                    result.Success = true;
                    result.Value = node.Value;
                    break;
                }
                if (node.NextRecordAddress == 0)
                {
                    break;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="equals"></param>
    /// <returns></returns>
    public async Task<HashResult<THashValue>> Get(Database db, ulong hashCode, Func<THashValue, bool> equals)
        => await Get(db, hashCode, value => Task.FromResult(equals(value)));

    /// <summary>
    /// 是否包含某个值
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Contains(Database db, ulong hashCode, THashValue value)
    {
        var result = await Get(db, hashCode, item =>
        {
            return item == value;
        });
        return result.Success;
    }

    /// <summary>
    /// Contains
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="equals"></param>
    /// <returns></returns>
    public async Task<bool> Contains(Database db, ulong hashCode, Func<THashValue, Task<bool>> equals)
    {
        var result = await Get(db, hashCode, equals);
        return result.Success;
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="onNew"></param>
    /// <returns></returns>
    public async Task Add(Database db, ulong hashCode, Func<Task<THashValue>> onNew)
    {
        bool isAdded = false;
        ulong globalIndex = hashCode;
        ulong entryGlobalIndex = globalIndex;
        HashNode<THashValue> lastNode = new(); 
        await UpdateEntry(db, entryGlobalIndex, async item =>
        {
            if (item.HashCode == 0)
            {
                isAdded = true;
                item.HashCode = hashCode;
                item.Value = await onNew();
                lastNode = item;
                return (true, item);
            }
            else
            {
                lastNode = item;
                return (false, default);
            }
        });
        if (isAdded)
        {
            return;
        }

        globalIndex = GetNextHashCode(globalIndex);

        if (lastNode.NextRecordAddress == 0 || lastNode.ArrayRecordAddress == 0)
        {
            //在while循环中，无法在循环开始时申请完成后反写，所以对于EntryNode进行提前申请反写
            if(lastNode.NextRecordAddress == 0)
            {
                lastNode.NextRecordAddress = await db.AllocateHashRecord<THashValue>();
            }
            if(lastNode.ArrayRecordAddress == 0)
            {
                lastNode.ArrayRecordAddress = await db.AllocateArrayRecord<THashValue>();
            }
            try
            {
                await UpdateEntry(db, entryGlobalIndex, item =>
                {
                    item.NextRecordAddress = lastNode.NextRecordAddress;
                    item.ArrayRecordAddress = lastNode.ArrayRecordAddress;
                    return (true, item);
                });
            }
            catch(Exception e)
            {
                if(e is ArgumentOutOfRangeException)
                {
                    Logger.Error($"EntryGlobalIndex:{entryGlobalIndex},LastNode.NextRecordAddress:{lastNode.NextRecordAddress}," +
                        $"LastNode.ArrayRecordAddress:{lastNode.ArrayRecordAddress}");
                }
                throw;
            }
            
        }

        HashBlock<THashValue> hashBlock = new();
        while (true)
        {
            if (globalIndex == 0)
            {
                //如果是0，表示已经到了最后一个节点
                // 此时不会判断ArrayRecordAddress是否为0，因为在EntryNode中或者在上一次循环中已经申请过了
                var arrayProcessor = new ArrayProcessor<THashValue>(db, lastNode.ArrayRecordAddress);
                await arrayProcessor.Add(await onNew());
                break;
            }
            else
            {
                var currentHashRecordAddress = lastNode.NextRecordAddress;
                hashBlock.SetByRecordAddress(currentHashRecordAddress);
                var nodeOffset = HashRecord<THashValue>.GetNodeOffset(globalIndex);
                try
                {
                    await hashBlock.RecordVisitor.UpdateSpan(db, currentHashRecordAddress, nodeOffset, HashNode<THashValue>.Size, async buffer =>
                    {
                        HashNode<THashValue> node = new();
                        node.Read(buffer, 0);
                        if (node.HashCode == 0)
                        {
                            isAdded = true;
                            node.HashCode = hashCode;
                            node.Value = await onNew();
                            node.Write(buffer, 0);
                            lastNode = node;
                            return true;
                        }
                        else
                        {
                            lastNode = node;
                            return false;
                        }
                    });
                }
                catch
                {
                    Logger.Error($"HashBlock:{hashBlock.Address},CurrentHashRecordAddress:{currentHashRecordAddress}," +
                        $"NodeOffset:{nodeOffset},HashNode.Size:{HashNode<THashValue>.Size}");
                    throw;
                }
                if (isAdded)
                {
                    break;
                }
                if (lastNode.NextRecordAddress == 0||lastNode.ArrayRecordAddress==0)
                {
                    //需要新建HashRecord或者ArrayRecord，由下一次循环处理
                    if(lastNode.NextRecordAddress == 0)
                    {
                        lastNode.NextRecordAddress = await db.AllocateHashRecord<THashValue>();
                    }
                    if(lastNode.ArrayRecordAddress == 0)
                    {
                        lastNode.ArrayRecordAddress = await db.AllocateArrayRecord<THashValue>();
                    }
                    //更新节点
                    try
                    {
                        await hashBlock.RecordVisitor.UpdateSpan(db, currentHashRecordAddress, nodeOffset, HashNode<THashValue>.Size, buffer =>
                        {
                            HashNode<THashValue> node = new();
                            node.Read(buffer, 0);
                            node.NextRecordAddress = lastNode.NextRecordAddress;
                            node.ArrayRecordAddress = lastNode.ArrayRecordAddress;
                            node.Write(buffer, 0);
                            return true;
                        });
                    }
                    catch
                    {
                        Logger.Error($"HashBlock:{hashBlock.Address},CurrentHashRecordAddress:{currentHashRecordAddress}," +
                                $"NodeOffset:{nodeOffset},HashNode.Size:{HashNode<THashValue>.Size}");
                        throw;
                    }
                }
                globalIndex /= HashNode<THashValue>.HashRecordNodeCount;
            }
            
        }
    }

    /// <summary>
    /// 替换
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="equals"></param>
    /// <param name="onUpdate"></param>
    /// <returns></returns>
    public async Task Replace(Database db, ulong hashCode, Func<THashValue, Task<bool>> equals, Func<THashValue, Task<THashValue>>? onUpdate)
    {
        ulong globalIndex = hashCode;
        ulong entryGlobalIndex = globalIndex;
        bool isReplaced = false;
        HashNode<THashValue> lastNode = new();
        await UpdateEntry(db, entryGlobalIndex, async item =>
        {
            if (item.HashCode == hashCode && await equals(item.Value))
            {
                //目标值已经存在
                isReplaced = true;
                if (onUpdate != null)
                {
                    //更新目标值，所以返回true，表示已经更新，将数据写入
                    var oldValue = item.Value;
                    item.Value = await onUpdate(item.Value);
                    lastNode = item;
                    return (oldValue != item.Value, item);
                }
                else
                {
                    //不需要更新，所以返回false，不写入数据
                    lastNode = item;
                    return (false, default);
                }
            }
            else
            {
                lastNode = item;
                return (false, default);
            }
        });
        if (isReplaced == true)
        {
            return;
        }
        globalIndex = GetNextHashCode(globalIndex);
        HashBlock<THashValue> hashBlock = new();
        while (true)
        {
            if(globalIndex == 0)
            {
                if(lastNode.ArrayRecordAddress == 0)
                {
                    break;
                }
                var arrayProcessor = new ArrayProcessor<THashValue>(db, lastNode.ArrayRecordAddress);
                await arrayProcessor.Replace(equals, onUpdate);
                break;
            }
            else
            {
                if (lastNode.NextRecordAddress == 0)
                {
                    break;
                }
                var currentHashRecordAddress = lastNode.NextRecordAddress;
                hashBlock.SetByRecordAddress(currentHashRecordAddress);
                try
                {
                    await hashBlock.RecordVisitor.UpdateSpan(db, currentHashRecordAddress, HashRecord<THashValue>.GetNodeOffset(globalIndex), HashNode<THashValue>.Size, async buffer =>
                    {
                        var node = new HashNode<THashValue>();
                        node.Read(buffer, 0);
                        if (node.HashCode == hashCode && await equals(node.Value))
                        {
                            isReplaced = true;
                            if (onUpdate != null)
                            {
                                var oldValue = node.Value;
                                node.Value = await onUpdate(node.Value);
                                if (oldValue != node.Value)
                                {
                                    node.Write(buffer, 0);
                                    lastNode = node;
                                    return true;
                                }
                                else
                                {
                                    lastNode = node;
                                    return false;
                                }
                            }
                            else
                            {
                                lastNode = node;
                                return false;
                            }
                        }
                        else
                        {
                            lastNode = node;
                            return false;
                        }
                    });
                }
                catch
                {
                    Logger.Error($"HashBlock:{hashBlock.Address},CurrentHashRecordAddress:{currentHashRecordAddress}," +
                        $"NodeOffset:{HashRecord<THashValue>.GetNodeOffset(globalIndex)},HashNode.Size:{HashNode<THashValue>.Size}");
                    throw;
                }
                
                if (isReplaced)
                {
                    break;
                }
                globalIndex /= HashNode<THashValue>.HashRecordNodeCount;
            }
        }
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="equals"></param>
    /// <param name="onNew"></param>
    /// <param name="onUpdate"></param>
    /// <returns></returns>
    public async Task Update(Database db, ulong hashCode, Func<THashValue, Task<bool>> equals, Func<Task<THashValue>> onNew, Func<THashValue, Task<THashValue>>? onUpdate)
    {
        if(await Contains(db, hashCode, equals))
        {
            await Replace(db, hashCode, equals, onUpdate);
        }
        else
        {
            await Add(db, hashCode, onNew);
        }
    }

    /// <summary>
    /// 尝试添加
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="equals"></param>
    /// <param name="onNew"></param>
    /// <returns></returns>
    public async Task<bool> TryAdd(Database db, ulong hashCode, Func<THashValue, Task<bool>> equals, Func<Task<THashValue>> onNew)
    {
        if (await Contains(db, hashCode, equals))
        {
            return false;
        }
        await Add(db, hashCode, onNew);
        return true;
    }

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="equals"></param>
    /// <returns></returns>
    public async Task Remove(Database db, ulong hashCode, Func<THashValue, Task<bool>> equals)
    {
        ulong globalIndex = hashCode;
        var node = await GetEntry(db, globalIndex);
        if (node.HashCode == hashCode && await equals(node.Value))
        {
            await DeleteEntry(db, globalIndex);
            return;
        }
        globalIndex = GetNextHashCode(globalIndex);
        HashBlock<THashValue> hashBlock = new();
        while (true)
        {
            if (globalIndex == 0)
            {
                if(node.ArrayRecordAddress==0)
                {
                    break;
                }
                var arrayProcessor = new ArrayProcessor<THashValue>(db, node.ArrayRecordAddress);
                await arrayProcessor.Remove(equals);
                break;
            }
            else
            {
                if (node.NextRecordAddress == 0)
                {
                    break;
                }
                var currentHashRecordAddress = node.NextRecordAddress;
                hashBlock.SetByRecordAddress(currentHashRecordAddress);
                await hashBlock.RecordVisitor.Read(db, currentHashRecordAddress, buffer =>
                {
                    var hashRecord = new HashRecord<THashValue>(buffer, 0);
                    node = hashRecord.GetNode(globalIndex);
                });
                if (node.HashCode == hashCode && await equals(node.Value))
                {
                    await hashBlock.RecordVisitor.Update(db, currentHashRecordAddress, buffer =>
                    {
                        var hashRecord = new HashRecord<THashValue>(buffer, 0);
                        node.Value.SetEmpty();
                        node.HashCode = 0;
                        hashRecord.SetNode(globalIndex, node);
                        return true;
                    });
                    break;
                }
                globalIndex /= HashNode<THashValue>.HashRecordNodeCount;
            }
        }
    }

    private async Task GetSubNodeValues(Database db, HashNode<THashValue> node, Action<THashValue> onNode)
    {
        if (node.ArrayRecordAddress != 0)
        {
            var arrayProcessor = new ArrayProcessor<THashValue>(db, node.ArrayRecordAddress);
            await arrayProcessor.Get(item =>
            {
                onNode(item);
            });
        }
        if (node.NextRecordAddress != 0)
        {
            var nextBlockAddress = Database.GetBlockAddress(node.NextRecordAddress);
            HashBlock<THashValue> hashBlock = new();
            hashBlock.Set(nextBlockAddress);
            List<HashNode<THashValue>> children = [];
            await hashBlock.RecordVisitor.Read(db, node.NextRecordAddress, buffer =>
            {
                var hashRecord = new HashRecord<THashValue>(buffer, 0);
                hashRecord.GetNodes(item =>
                {
                    if (item.HashCode != 0) children.Add(item);
                });
            });
            foreach (var item in children)
            {
                onNode(item.Value);
            }
            foreach (var item in children)
            {
                if (item.NextRecordAddress != 0) await GetSubNodeValues(db, item, onNode);
            }
        }
    }

    /// <summary>
    /// 获取所有值
    /// </summary>
    /// <param name="db"></param>
    /// <param name="onItem"></param>
    /// <returns></returns>
    public async Task GetValues(Database db,Action<THashValue> onItem)
    {
        var entryCount = RecordCount;
        var hashNodeSize = RecordSize;
        List<HashNode<THashValue>> entryNodes = [];
        await RecordRegionVisitor.Read(db, buffer =>
        {
            for (int i = 0; i < entryCount; i++)
            {
                var node = new HashNode<THashValue>();
                node.Read(buffer, i * hashNodeSize);
                if (node.HashCode != 0)
                {
                    entryNodes.Add(node);
                    onItem(node.Value);
                }
            }
        });
        foreach (var item in entryNodes)
        {
            await GetSubNodeValues(db, item, onItem);
        }
    }

    /// <summary>
    /// 设置，警告，HashCode相同的数据会被覆盖
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task Set(Database db, ulong hashCode, THashValue value)
    {
        await Update(db, hashCode, async item =>
        {
            await Task.CompletedTask;
            return true;
        }, async () =>
        {
            await Task.CompletedTask;
            return value;
        }, async item =>
        {
            await Task.CompletedTask;
            return value;
        });
    }

    /// <summary>
    /// 获取值，警告，仅仅比较HashCode
    /// </summary>
    /// <param name="db"></param>
    /// <param name="hashCode"></param>
    /// <param name="onItem"></param>
    /// <returns></returns>
    public async Task Get(Database db,ulong hashCode, Action<THashValue> onItem)
    {
        await Get(db, hashCode, item =>
        {
            onItem(item);
            return Task.FromResult(true);
        });
    }
}

/// <summary>
/// Hask记录块
/// </summary>
public class HashBlock<THashValue> : StatisticalLayout
    where THashValue : struct, IValue<THashValue>
{
    /// <summary>
    /// 设置必要信息
    /// </summary>
    /// <param name="address"></param>
    public void Set(long address)
    {
        SetAddress(address, HashRecord<THashValue>.Size, Database.BlockSize);
    }

    /// <summary>
    /// 根据记录地址设置必要信息
    /// </summary>
    /// <param name="address"></param>
    public void SetByRecordAddress(long address)
    {
        SetByRecordAddress(address, HashRecord<THashValue>.Size);
    }
}

/// <summary>
/// Hash节点结果
/// </summary>
/// <typeparam name="THashValue"></typeparam>
public struct HashResult<THashValue>
    where THashValue : struct, IValue<THashValue>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public THashValue Value;
}

/// <summary>
/// Hash记录
/// </summary>
public struct HashRecord<THashValue>(byte[] buffer, int offset)
    where THashValue : struct, IValue<THashValue>
{
    /// <summary>
    /// Record Data
    /// </summary>
    public byte[] Buffer = buffer;

    /// <summary>
    /// Data Offset
    /// </summary>
    public int Offset = offset;

    /// <summary>
    /// Get Hash Node
    /// </summary>
    /// <param name="globalIndex"></param>
    /// <returns></returns>
    public readonly HashNode<THashValue> GetNode(ulong globalIndex)
    {
        var index = (int)(globalIndex % HashNode<THashValue>.HashRecordNodeCount);
        var result = new HashNode<THashValue>();
        result.Read(Buffer, Offset + index * HashNode<THashValue>.Size);
        return result;
    }

    /// <summary>
    /// Set Hash Node
    /// </summary>
    /// <param name="globalIndex"></param>
    /// <param name="node"></param>
    public readonly void SetNode(ulong globalIndex, HashNode<THashValue> node)
    {
        var index = (int)(globalIndex % HashNode<THashValue>.HashRecordNodeCount);
        var nodeOffset = Offset + index * HashNode<THashValue>.Size;
        node.Write(Buffer, nodeOffset);
    }

    /// <summary>
    /// Size of Hash Record
    /// </summary>
    public static int Size = (int)(HashNode<THashValue>.HashRecordNodeCount * HashNode<THashValue>.Size);

    /// <summary>
    /// 接口名称
    /// </summary>
    public static string InterfaceName = $"__HASH_RECORD_{typeof(THashValue).Name}__";

    /// <summary>
    /// 获取Node索引
    /// </summary>
    /// <param name="globalIndex"></param>
    /// <returns></returns>
    public static int GetNodeIndex(ulong globalIndex)
    {
        return (int)(globalIndex % HashNode<THashValue>.HashRecordNodeCount);
    }

    /// <summary>
    /// 获取Node偏移
    /// </summary>
    /// <param name="globalIndex"></param>
    /// <returns></returns>
    public static int GetNodeOffset(ulong globalIndex)
    {
        return GetNodeIndex(globalIndex) * HashNode<THashValue>.Size;
    }

    /// <summary>
    /// 获取所有节点
    /// </summary>
    /// <param name="onNode"></param>
    /// <returns></returns>
    public void GetNodes(Action<HashNode<THashValue>> onNode)
    {
        for (int i = 0; i < HashNode<THashValue>.HashRecordNodeCount; i++)
        {
            var node = new HashNode<THashValue>();
            node.Read(Buffer, Offset + i * HashNode<THashValue>.Size);
            onNode(node);
        }
    }
}

/// <summary>
/// Hash 节点
/// </summary>
/// <typeparam name="TValue"></typeparam>
public struct HashNode<TValue>
    where TValue : struct, IValue<TValue>
{
    /// <summary>
    /// Hash Code
    /// </summary>
    public ulong HashCode;

    /// <summary>
    /// Value
    /// </summary>
    public TValue Value;

    /// <summary>
    /// Next Hash Record Address
    /// </summary>
    public long NextRecordAddress;

    /// <summary>
    /// 数组记录地址，当HashCode完全命中时，即HashCode一样时，通过数组来存储数据
    /// </summary>
    public long ArrayRecordAddress;

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public int Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(HashCode).CopyTo(buffer, offset);
        offset += sizeof(ulong);
        offset += Value.Write(buffer, offset);
        BitConverter.GetBytes(NextRecordAddress).CopyTo(buffer, offset);
        offset += sizeof(long);
        BitConverter.GetBytes(ArrayRecordAddress).CopyTo(buffer, offset);
        offset += sizeof(long);
        return Size;
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public int Read(byte[] buffer, int offset)
    {
        HashCode = BitConverter.ToUInt64(buffer, offset);
        offset += sizeof(ulong);
        Value = new TValue();
        offset += Value.Read(buffer, offset);
        NextRecordAddress = BitConverter.ToInt64(buffer, offset);
        offset += sizeof(long);
        ArrayRecordAddress = BitConverter.ToInt64(buffer, offset);
        return Size;
    }

    /// <summary>
    /// 当前节点的大小
    /// </summary>
    public static int Size = sizeof(ulong) + TValue.GetSize() + sizeof(long) * 2;

    /// <summary>
    /// Hash Record Node Count
    /// </summary>
    public const uint HashRecordNodeCount = 64;
}










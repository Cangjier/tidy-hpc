using TidyHPC.LiteDB.Layouts.Visitors;

namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 一块记录区域
/// <para>Block数据大小是固定的</para>
/// <para>[short:记录当前已使用数量][统计区域][数据区域]</para>
/// </summary>
public class StatisticalLayout:Layout
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public StatisticalLayout()
    {
        UsedCountVisitor = new(this);
        StatisticalRegionVisitor = new(this);
    }

    #region Const

    /// <summary>
    /// 已使用数量地址
    /// </summary>
    public long UsedCountAddress { get; private set; }

    /// <summary>
    /// 已使用数量的大小
    /// </summary>
    public const int UsedCountSize = sizeof(int);

    /// <summary>
    /// 统计区域地址
    /// </summary>
    public long StatisticalRegionAddress { get; private set; }

    /// <summary>
    /// 统计区域大小
    /// </summary>
    public int StatisticalRegionSize { get; private set; }

    /// <summary>
    /// 缓存大小
    /// </summary>
    public int CacheSize { get; private set; }

    #endregion

    #region 设置/初始化

    /// <summary>
    /// 设置块的信息
    /// </summary>
    /// <param name="address">块的首地址</param>
    /// <param name="recordSize">单个记录的大小</param>
    /// <param name="blockSize">块的大小</param>
    public override void SetAddress(long address, int recordSize, int blockSize)
    {
        Address = address;
        RecordSize = recordSize;
        BlockSize = blockSize;

        RecordCount = LayoutUtil.FindMaxRecordCount(recordSize, blockSize, UsedCountSize);

        UsedCountAddress = address;
        StatisticalRegionAddress = UsedCountAddress + UsedCountSize;
        StatisticalRegionSize = (int)Math.Ceiling(RecordCount * 0.125);
        FirstRecordAddress = StatisticalRegionAddress + StatisticalRegionSize;
        RecordRegionSize = RecordCount * RecordSize;
        CacheSize = UsedCountSize + StatisticalRegionSize;

        Check(address);
    }

    /// <summary>
    /// 根据记录地址设置这块区域
    /// </summary>
    /// <param name="recordAddress"></param>
    /// <param name="recordSize"></param>
    /// <returns></returns>
    public StatisticalLayout SetByRecordAddress(long recordAddress,int recordSize)
    {
        var blockAddress = Database.GetBlockAddress(recordAddress);
        SetAddress(blockAddress, recordSize, Database.BlockSize);
        return this;
    }

    /// <summary>
    /// 设置必要信息
    /// </summary>
    /// <param name="address"></param>
    /// <param name="recordSize"></param>
    /// <returns></returns>
    public StatisticalLayout Set(long address, int recordSize)
    {
        SetAddress(address, recordSize, Database.BlockSize);
        return this;
    }

    /// <summary>
    /// 初始化这块区域
    /// </summary>
    /// <returns></returns>
    public override async Task Initialize(Database table)
    {
        await UsedCountVisitor.Write(table, () => 0);
        await StatisticalRegionVisitor.Write(table, (buffer, offset) =>
        {
            for (int i = 0; i < StatisticalRegionSize; i++)
            {
                buffer[offset + i] = 0;
            }
        });
    }
    #endregion

    #region Visitor

    /// <summary>
    /// 已使用数量的访问器
    /// </summary>
    public UsedCountVisitor UsedCountVisitor { get; }

    /// <summary>
    /// 统计区域的访问器
    /// </summary>
    public StatisticalRegionVisitor StatisticalRegionVisitor { get; }
    #endregion

    #region 辅助方法

    /// <summary>
    /// 取消使用统计区域目标位
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private async Task Unuse(Database table, int index)
    {
        bool used = false;
        await StatisticalRegionVisitor.Update(table, (buffer, offset) =>
        {
            if (!LayoutUtil.IsUsed(buffer, offset, index)) return false;
            LayoutUtil.Unuse(buffer, offset, index);
            used = true;
            return true;
        });
        if (used)
        {
            await UsedCountVisitor.Update(table, usedCount =>
            {
                usedCount--;
                return (usedCount, true);
            });
        }
    }
    #endregion

    #region 对外的业务方法

    /// <summary>
    /// 申请一个可用的记录地址
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public async Task<long?> AllocateRecord(Database table)
    {
        int index = -1;
        await StatisticalRegionVisitor.Update(table, (buffer, offset) =>
        {
            index = LayoutUtil.GetFirstUnused(buffer, offset, RecordCount);
            if (index != -1)
            {
                LayoutUtil.Use(buffer, offset, index);
                return true;
            }
            return false;
        });
        if (index != -1)
        {
            await UsedCountVisitor.Update(table, usedCount =>
            {
                usedCount++;
                return (usedCount, true);
            });
        }
        else
        {
            return null;
        }
        return FirstRecordAddress + index * RecordSize;
    }

    /// <summary>
    /// 取消使用统计区域目标位
    /// </summary>
    /// <param name="db"></param>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task UnuseByAddress(Database db, long address)
    {
        var index = (address - FirstRecordAddress) / RecordSize;
        await Unuse(db, (int)index);
    }

    /// <summary>
    /// 是否包含可用的位
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task<bool> ContainsUnused(Database db)
    {
        bool result = true;
        await UsedCountVisitor.Read(db, usedCount =>
        {
            result = usedCount < RecordCount;
        });
        return result;
    }

    /// <summary>
    /// 获取所有已使用的索引
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<int[]> GetUsedIndexs(Database db)
    {
        int[]? result = null;
        await StatisticalRegionVisitor.Read(db, (buffer, offset) =>
        {
            result = LayoutUtil.GetAllNoZeroIndex(buffer, offset, RecordCount).ToArray();
        });
        if (result == null)
        {
            throw new Exception("GetUsedIndexs error");
        }
        return result;
    }

    /// <summary>
    /// 获取已使用数量
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task<int> GetUsedCount(Database db)
    {
        int result = 0;
        await UsedCountVisitor.Read(db, usedCount =>
        {
            result = usedCount;
        });
        return result;
    }
    #endregion

    internal void Check(long address)
    {
        var cacaheOffsetLong = UsedCountAddress - Address;
        var cacaheOffset = (int)cacaheOffsetLong;
        if (cacaheOffset < 0 || cacaheOffset >= CacheSize)
        {
            throw new Exception($"UsedCountVisitor Read Error,`{UsedCountAddress} - {Address}` = {cacaheOffsetLong},{cacaheOffset}, address={address}");
        }
    }
}





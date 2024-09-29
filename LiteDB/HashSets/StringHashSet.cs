using System.Text;
using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.Hashes;
using TidyHPC.Loggers;

namespace TidyHPC.LiteDB.HashSets;

/// <summary>
/// String Hash Set
/// </summary>
internal class StringHashSet
{
    /// <summary>
    /// 是否启用调试模式，该模式下会输出更多的日志
    /// </summary>
    public static bool Debug { get; set; } = false;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="hashTableAddress"></param>
    /// <param name="database"></param>
    public StringHashSet(Database database, long hashTableAddress)
    {
        HashTable = new();
        HashTable.SetAddress(hashTableAddress);
        Database = database;
    }

    internal HashTable<Int64Value> HashTable { get; }

    /// <summary>
    /// Database
    /// </summary>
    internal Database Database { get; }

    /// <summary>
    /// 新建字符串，如果字符串已经存在，则引用计数加1，否则新建字符串，引用计数为1
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<long> New(string value)
    {
        long result = 0;
        var hashCode = await HashService.GetHashCode(value);
        int currentReferenceCount = 0;
        await HashTable.Update(Database, hashCode, async recordAddress =>
        {
            var nodeString = await GetString(recordAddress.Value, false);
            var result = value == nodeString;
            if (Debug)
            {
                LiteJson.Json self = LiteJson.Json.NewObject();
                self["recordAddress"] = recordAddress.Value;
                self["value"] = value;
                self["nodeString"] = nodeString;
                Database.Logger.WriteLine($"// string match {(result ? "true" : "false")} {self.ToString(false)}");
            }
            return result;
        }, async () =>
        {
            result = await AllocateString(value);
            currentReferenceCount = 1;
            return result;
        }, async recordAddress =>
        {
            result = recordAddress;
            await ChangeReferenceCount(recordAddress, count =>
            {
                currentReferenceCount = count + 1;
                return count + 1;
            });
            return recordAddress;
        });
        if (Debug)
        {
            LiteJson.Json self = LiteJson.Json.NewObject();
            self["recordAddress"] = result;
            self["currentReferenceCount"] = currentReferenceCount;
            self["value"] = value;
            Database.Logger.WriteLine($"// string new {self.ToString(false)}");
        }
        return result;
    }

    /// <summary>
    /// 借用字符串，如果字符串存在，引用计数不变，否则新建字符串，引用计数为0
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<long> Borrow(string value)
    {
        long result = 0;
        var hashCode = await HashService.GetHashCode(value);
        int currentReferenceCount = 0;
        await HashTable.Update(Database, hashCode, async recordAddress =>
        {
            return value == await GetString(recordAddress, false);
        }, async () =>
        {
            result = await AllocateString(value, 0);
            currentReferenceCount = 0;
            return result;
        }, async recordAddress =>
        {
            var record = await GetRecord(recordAddress);
            result = recordAddress;
            currentReferenceCount = record.ReferenceCount;
            return recordAddress;
        });
        if (Debug)
        {
            LiteJson.Json self = LiteJson.Json.NewObject();
            self["recordAddress"] = result;
            self["currentReferenceCount"] = currentReferenceCount;
            self["value"] = value;
            Database.Logger.WriteLine($"// string borrow {self.ToString(false)}");
        }
        return result;
    }

    public async Task Increase(long recordAddress)
    {
        int currentReferenceCount = 0;
        await ChangeReferenceCount(recordAddress, count =>
        {
            currentReferenceCount = count + 1;
            return count + 1;
        });
        if (Debug)
        {
            LiteJson.Json self = LiteJson.Json.NewObject();
            self["recordAddress"] = recordAddress;
            self["currentReferenceCount"] = currentReferenceCount;
            Database.Logger.WriteLine($"// string increase {self.ToString(false)}");
        }
    }

    public async Task Release(long recordAddress)
    {
        int currentReferenceCount = 0;
        bool toRelease = false;
        await Database.Cache.StatisticalBlockPool.Use(async block =>
        {
            block.SetByRecordAddress(recordAddress, StringRecord.Size);
            await block.RecordVisitor.Update(Database, recordAddress, buffer =>
            {
                var result = new StringRecord(buffer, 0);
                result.ReferenceCount--;
                currentReferenceCount = result.ReferenceCount;
                toRelease = result.ReferenceCount == 0;
                result.Write(buffer, 0);
                return true;
            });
        });
        if (toRelease)
        {
            //获取所有的记录地址
            List<long> toReleaseRecordAddresses = [];
            string recordString = await GetString(recordAddress, false, (recordAddress, record) =>
            {
                toReleaseRecordAddresses.Add(recordAddress);
            });
            var hashCode = await HashService.GetHashCode(recordString);
            //将HashNode释放
            await HashTable.Remove(Database, hashCode, async node =>
            {
                await Task.CompletedTask;
                return node == recordAddress;
            });
            //将记录释放
            await Database.Cache.StatisticalBlockPool.Use(async block =>
            {
                foreach (var item in toReleaseRecordAddresses)
                {
                    block.SetByRecordAddress(item, StringRecord.Size);
                    await block.UnuseByAddress(Database, item);
                }
            });
        }
       
        if (Debug)
        {
            LiteJson.Json self = LiteJson.Json.NewObject();
            self["recordAddress"] = recordAddress;
            self["currentReferenceCount"] = currentReferenceCount;
            Database.Logger.WriteLine($"// string release {self.ToString(false)}");
        }
    }

    

    public async Task<string> Read(long recordAddress)
    {
        return await GetString(recordAddress, Debug);
    }

    /// <summary>
    /// 根据地址获取记录
    /// </summary>
    /// <param name="recordAddress"></param>
    /// <returns></returns>
    internal async Task<StringRecord> GetRecord(long recordAddress)
    {
        StringRecord result = new();
        await Database.Cache.StatisticalBlockPool.Use(async block =>
        {
            block.SetByRecordAddress(recordAddress, StringRecord.Size);
            await block.RecordVisitor.Read(Database, recordAddress, buffer =>
            {
                result = new StringRecord(buffer, 0);
            });
        });
        return result;
    }

    internal async Task ChangeReferenceCount(long recordAddress,Func<int,int> onChangeReferenceCount)
    {
        await Database.Cache.StatisticalBlockPool.Use(async block =>
        {
            block.SetByRecordAddress(recordAddress, StringRecord.Size);
            await block.RecordVisitor.Update(Database, recordAddress, buffer =>
            {
                var result = new StringRecord(buffer, 0);
                var oldReferenceCount = result.ReferenceCount;
                result.ReferenceCount = onChangeReferenceCount(result.ReferenceCount);
                result.Write(buffer, 0);
                if (Debug)
                {
                    Database.Logger.WriteLine($"// string changeReferenceCount {recordAddress} {oldReferenceCount}->{result.ReferenceCount} {result.ToString(false)}");
                }
                return true;
            });
        });
    }

    /// <summary>
    /// 将记录写入数据库
    /// </summary>
    /// <param name="recordAddress"></param>
    /// <param name="record"></param>
    /// <returns></returns>
    internal async Task SetRecord(long recordAddress,StringRecord record)
    {
        await Database.Cache.StatisticalBlockPool.Use(async block =>
        {
            block.SetByRecordAddress(recordAddress, StringRecord.Size);
            await block.RecordVisitor.Write(Database, recordAddress, buffer =>
            {
                record.Write(buffer, 0);
            });
        });
    }

    /// <summary>
    /// 分配字符串
    /// </summary>
    /// <param name="value"></param>
    /// <param name="referenceCount"></param>
    /// <returns></returns>
    internal async Task<long> AllocateString(string value,int referenceCount=1)
    {
        byte[] bytes = Util.UTF8.GetBytes(value);
        int recordCount = bytes.Length / 256;//if length is zero, recordCount is zero
        if (bytes.Length % 256 != 0)
        {
            recordCount++;
        }
        else if (recordCount == 0)
        {
            recordCount = 1;//if length is zero, recordCount is one
        }
        long[] recordAddresses = new long[recordCount];
        for (int i = 0; i < recordCount; i++)
        {
            var recordAddress =await Database.AllocateRecord("__STRING_RECORD__");
            recordAddresses[i] = recordAddress;
        }
        int nextLength = bytes.Length;
        for (int i = 0; i < recordCount; i++)
        {
            var record = new StringRecord
            {
                Length = bytes.Length,
                Value = new byte[256],
                ReferenceCount = referenceCount
            };
            bytes.Skip(i * 256).Take(nextLength > 256 ? 256 : nextLength).ToArray().CopyTo(record.Value, 0);
            nextLength -= 256;
            if (i != recordCount - 1)
            {
                record.NextRecordAddress = recordAddresses[i + 1];
            }
            else
            {
                record.NextRecordAddress = 0;
            }
            await SetRecord(recordAddresses[i], record);
        }
        if (Debug)
        {
            LiteJson.Json self = LiteJson.Json.NewObject();
            self["recordAddress"] = recordAddresses[0];
            self["currentReferenceCount"] = referenceCount;
            self["value"] = value;
            Database.Logger.WriteLine($"// string allocate {self.ToString(false)}");
        }
        return recordAddresses[0];
    }

    /// <summary>
    /// 根据地址获取字符串
    /// </summary>
    /// <param name="recordAddress"></param>
    /// <param name="debug"></param>
    /// <param name="onRecord"></param>
    /// <returns></returns>
    internal async Task<string> GetString(long recordAddress, bool debug, Action<long, StringRecord>? onRecord = null)
    {
        StringBuilder builder = new();
        long nextRecordAddress = recordAddress;
        var record = await GetRecord(nextRecordAddress);
        onRecord?.Invoke(nextRecordAddress, record);    
        int currentReferenceCount = record.ReferenceCount;
        var nextLength = record.Length;
        try
        {
            builder.Append(Util.UTF8.GetString(record.Value, 0, nextLength > 256 ? 256 : nextLength));
        }
        catch
        {
            throw;
        }
        nextLength -= 256;
        nextRecordAddress = record.NextRecordAddress;
        while (true)
        {
            if (nextLength <= 0)
            {
                break;
            }
            record = await GetRecord(nextRecordAddress);
            onRecord?.Invoke(nextRecordAddress, record);
            builder.Append(Util.UTF8.GetString(record.Value, 0, nextLength > 256 ? 256 : nextLength));
            nextLength -= 256;
            nextRecordAddress = record.NextRecordAddress;
        }
        if (debug)
        {
            LiteJson.Json self = LiteJson.Json.NewObject();
            self["recordAddress"] = recordAddress;
            self["currentReferenceCount"] = currentReferenceCount;
            self["value"] = builder.ToString();
            Database.Logger.WriteLine($"// string get {self.ToString(false)}");
        }
        return builder.ToString();
    }

    /// <summary>
    /// 初始化块数据
    /// </summary>
    /// <returns></returns>
    internal async Task Initialize()
    {
        await HashTable.Initialize(Database);
    }
}

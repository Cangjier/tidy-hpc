using TidyHPC.LiteDB.Metas;

namespace TidyHPC.LiteDB;

internal class ObjectInterfaceRuntime: ObjectInterface
{
    /// <summary>
    /// 映射地址
    /// </summary>
    internal long[]? MappingAddress { get; set; }

    internal async Task Parse(Database db, MetaRecord record, MetaDefineRecord defineRecord)
    {
        MappingAddress = new long[defineRecord.FieldMapAddresses.Length];
        defineRecord.FieldMapAddresses.CopyTo(MappingAddress, 0);
        FullName = record.TypeName;
        for (int i = 0; i < defineRecord.FieldCount; i++)
        {
            Fields.Add(new Field
            {
                Name = await db.StringHashSet.Read(defineRecord.FieldNames[i]),
                Type = (FieldType)defineRecord.FieldTypes[i],
                ArrayLength = defineRecord.FieldArrayLengths[i],
                MapType = (FieldMapType)defineRecord.FieldMapTypes[i]
            });
        }
    }

    internal async Task<RecordRuntime> DeserializeFromAddress(Database db, long address)
    {
        RecordRuntime result = NewRecordRuntime();
        var block = await db.Cache.StatisticalBlockPool.Dequeue();
        block.SetByRecordAddress(address, GetSize());
        await block.RecordVisitor.Read(db, address, buffer =>
        {
            result.DeserializeFromBuffer(buffer, 0);
        });
        db.Cache.StatisticalBlockPool.Enqueue(block);
        return result;
    }

    internal RecordRuntime NewRecordRuntime()
    {
        return new RecordRuntime()
        {
            Fields = Fields.Select(item => new FieldRuntime() { Define = item }).ToArray()
        };
    }
}

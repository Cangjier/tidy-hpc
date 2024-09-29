using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteJson;

namespace TidyHPC.LiteDB.Debuggers;

/// <summary>
/// 调试器
/// </summary>
public class Debugger
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="database"></param>
    public Debugger(Database database)
    {
        Database = database;
    }

    /// <summary>
    /// Database
    /// </summary>
    public Database Database { get; }

    /// <summary>
    /// Get the size of the database
    /// </summary>
    /// <returns></returns>
    public async Task<long> GetSize()
    {
        return await Database.FileStream.ReadLongAsync(Database.DatabaseSizeAddress);
    }

    /// <summary>
    /// Get is initialized
    /// </summary>
    /// <returns></returns>
    public async Task<bool> GetIsInitialized()
    {
        return await Database.FileStream.ReadBooleanAsync(Database.FlagAddress);
    }

    /// <summary>
    /// Get the object interfaces
    /// </summary>
    /// <returns></returns>
    public async Task<Json> GetObjectInterfaces()
    {
        var root = Json.NewObject();
        var objectInterfacesJson = root.GetOrCreateArray("ObjectInterfaces");
        var typeNames = await Database.GetInterfaceNames();
        foreach (var typeName in typeNames)
        {
            var objectInterface = await Database.GetObjectInterface(typeName);
            if (objectInterface == null) continue;
            var itemJson = objectInterfacesJson.AddObject();
            itemJson.Set("FullName", typeName);
            var fieldsJson = itemJson.GetOrCreateArray("Fields");
            foreach (var field in objectInterface.Fields)
            {
                var fieldJson = fieldsJson.AddObject();
                fieldJson.Set("Name", field.Name);
                fieldJson.Set("Type", field.Type.ToString());
                fieldJson.Set("MapType", field.MapType.ToString());
            }
        }
        return root;
    }

    /// <summary>
    /// Get the type names
    /// </summary>
    /// <returns></returns>
    public async Task<string[]> GetTypeNames()
    {
        return await Database.GetInterfaceNames();
    }

    /// <summary>
    /// Scan the type table
    /// </summary>
    /// <returns></returns>
    public async Task<Json> ScanTypeTable()
    {
        Json result = Json.NewObject();
        await Task.CompletedTask;
        //var nodesJson = result.GetOrCreateArray("HashNodes");
        //var hashTable = await Database.Cache.HashTablePool.Dequeue();
        //var metaBlock = await Database.Cache.MetaBlockPool.Dequeue();
        //hashTable.SetAddress(Database.InterfaceTableAddress);
        //await hashTable.Scan(Database,async node =>
        //{
        //    var nodeJson = nodesJson.AddObject();
        //    nodeJson.Set("HashCode", node.HashCode);
        //    nodeJson.Set("Value", node.Value);
        //    nodeJson.Set("NextHashRecordAddress", node.NextHashRecordAddress);
        //    if(node.Value != 0)
        //    {
        //        metaBlock.SetByRecordAddress(node.Value);
        //        await metaBlock.RecordVisitor.Read(Database, node.Value, buffer =>
        //        {
        //            var metaRecord = new MetaRecord();
        //            metaRecord.Read(buffer, 0);
        //            nodeJson.Set("TypeName", metaRecord.TypeName ?? "");
        //        });
        //    }
        //});
        //Database.Cache.HashTablePool.Enqueue(hashTable);
        //Database.Cache.MetaBlockPool.Enqueue(metaBlock);
        return result;
    }

    /// <summary>
    /// Print the type table
    /// </summary>
    /// <returns></returns>
    public async Task PrintTypeTable()
    {
        Console.WriteLine($"TypeTable={(await ScanTypeTable()).ToString(true)}");
    }

    /// <summary>
    /// Print the block used status
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public async Task PrintBlockUsedStatus(string typeName)
    {
        var metaAllocater = await Database.GetMetaAllocater(typeName);
        var objectInterface = await Database.GetObjectInterface(typeName);
        if (objectInterface == null)
        {
            Console.WriteLine($"ObjectInterface {typeName} not found");
            return;
        }
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        var blockAddresses = await metaAllocater.GetBlockAddresses();
        List<long> fullBlocks = new();
        foreach (var blockAddress in blockAddresses)
        {
            block.Set(blockAddress, objectInterface.GetSize());
            var usedCount = await block.GetUsedCount(Database);
            if(usedCount==block.RecordCount)
            {
                fullBlocks.Add(blockAddress);
            }
            else
            {
                Console.WriteLine($"{$"{blockAddress}",-32},{$"Used={usedCount}/{block.RecordCount}",-32}");
            }
        }
        Console.WriteLine($"{"FullBlocks",-32},{fullBlocks.Count}");
        Database.Cache.StatisticalBlockPool.Enqueue(block);
    }

    /// <summary>
    /// Print the meta allocater status
    /// </summary>
    /// <returns></returns>
    public async Task PrintMetaAllocaterStatus()
    {
        var metaAllocaters = Database.MetaAllocaters;
        var sum = 0;
        foreach (var metaAllocater in metaAllocaters.Values)
        {
            var blockAddresses = await metaAllocater.GetBlockAddresses();
            sum+=blockAddresses.Length;
            Console.WriteLine($"{$"{metaAllocater.TypeName}",-32},{$"BlocksCount={blockAddresses.Length}",-32}");
        }
        Console.WriteLine($"{"Total",-32},{$"BlocksCount={sum}",-32}");
    }

    /// <summary>
    /// Print the database status
    /// </summary>
    /// <returns></returns>
    public async Task PrintDatabaseStatus()
    {
        var size = await Database.FileStream.ReadLongAsync(Database.DatabaseSizeAddress);
        Console.WriteLine($"{$"TotalSize={(size - Database.HeaderSize) / Database.BlockSize}MB",-32}");
        
    }

    /// <summary>
    /// 申请一个HashTable地址
    /// </summary>
    /// <returns></returns>
    public async Task<long> AllocateHashTable() => await Database.AllocateHashTable();

    /// <summary>
    /// 申请一个HashRecord地址
    /// </summary>
    /// <returns></returns>
    public async Task<long> AllocateHashRecord<IValue>()
        where IValue : struct, IValue<IValue>
        => await Database.AllocateHashRecord<IValue>();
}

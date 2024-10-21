using TidyHPC.LiteDB.Layouts;

namespace TidyHPC.LiteDB.Metas;
internal class MetaBlock : StatisticalLayout
{
    public MetaBlock Set(long address)
    {
        base.SetAddress(address, MetaRecord.Size, Database.BlockSize);
        return this;
    }

    public MetaBlock SetByRecordAddress(long recordAddress)
    {
        Set(Database.GetBlockAddress(recordAddress));
        return this;
    }
}

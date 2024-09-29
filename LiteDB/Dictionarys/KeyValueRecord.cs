using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.DBTypes;
using TidyHPC.Loggers;

namespace TidyHPC.LiteDB.Dictionarys;

internal struct KeyValueRecord<T>
    where T : DBType, new()
{
    public T Key;

    public long Value;

    public void Read(byte[] buffer, int offset)
    {
        Key = new T();
        offset += Key.Read(buffer, offset);
        Value = BitConverter.ToInt64(buffer, offset);
        //Logger.Info($"Key:{Key} Get >>> Value:{Value}");
    }

    public void Write(byte[] buffer, int offset)
    {
        offset += Key.Write(buffer, offset);
        BitConverter.GetBytes(Value).CopyTo(buffer, offset);
        //Logger.Info($"Set Key:{Key} Set <<< Value:{Value}");
    }

    public static int GetSize()
    {
        int keySize = 0;
        keySize += T.GetSize();
        return keySize + sizeof(long);
    }

    private static string? _FullName = null;

    public static string GetFullName()
    {
        if(_FullName==null)
        {
            if(typeof(T).IsGenericType)
            {
                var genericNames = typeof(T).GenericTypeArguments.Select(x => x.Name);
                _FullName = "__KEY_VALUE_RECORD_" + typeof(T).Name + "[" + genericNames.Join(",") + "]__";
            }
            else
            {
                _FullName = "__KEY_VALUE_RECORD_" + typeof(T).Name + "__";
            }
        }
        return _FullName;
    }
}



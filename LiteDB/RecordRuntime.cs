using TidyHPC.LiteDB.Metas;
using TidyHPC.LiteJson;

namespace TidyHPC.LiteDB;

/// <summary>
/// 序列化信息
/// </summary>
internal class RecordRuntime:IDisposable
{
    public bool Success { get; set; } = false;

    /// <summary>
    /// 主键，目前只支持Guid
    /// </summary>
    public Guid Master;

    public FieldRuntime[]? Fields;

    public async Task<Json> SerializeToJson(Database db)
    {
        if(Fields is null)
        {
            throw new Exception("Fields is null");
        }
        var result = Json.NewObject();
        foreach (var fieldRuntime in Fields)
        {
            var type = fieldRuntime.Define.Type;
            if (fieldRuntime.Define.IsArray)
            {
                if (type == FieldType.Char)
                {
                    result.Set(fieldRuntime.Define.Name, (string)fieldRuntime.Value);
                }
                else
                {
                    var array = Json.NewArray();
                    result.Set(fieldRuntime.Define.Name, array);
                    if (type == FieldType.Byte)
                    {
                        foreach (var item in (byte[])fieldRuntime.Value)
                        {
                            array.Add(item);
                        }
                    }
                    else if (type == FieldType.Boolean)
                    {
                        foreach (var item in (bool[])fieldRuntime.Value)
                        {
                            array.Add(item);
                        }
                    }
                    else if (type == FieldType.Int32)
                    {
                        foreach (var item in (int[])fieldRuntime.Value)
                        {
                            array.Add(item);
                        }
                    }
                    else if (type == FieldType.Int64)
                    {
                        foreach (var item in (long[])fieldRuntime.Value)
                        {
                            array.Add(item);
                        }
                    }
                    else if (type == FieldType.Float)
                    {
                        foreach (var item in (float[])fieldRuntime.Value)
                        {
                            array.Add(item);
                        }
                    }
                    else if (type == FieldType.Double)
                    {
                        foreach (var item in (double[])fieldRuntime.Value)
                        {
                            array.Add(item);
                        }
                    }
                    else if (type == FieldType.Guid)
                    {
                        foreach (var item in (Guid[])fieldRuntime.Value)
                        {
                            array.Add(item.ToString());
                        }
                    }
                    else if (type == FieldType.DateTime)
                    {
                        foreach (var item in (long[])fieldRuntime.Value)
                        {
                            array.Add(new DateTime(item));
                        }
                    }
                    else if (type == FieldType.MD5)
                    {
                        foreach (var item in (byte[][])fieldRuntime.Value)
                        {
                            array.Add(Util.BytesToHexString(item));
                        }
                    }
                    else if (type == FieldType.ReferneceString)
                    {
                        foreach (var item in (long[])fieldRuntime.Value)
                        {
                            if (item == 0) continue;
                            array.Add(await db.StringHashSet.Read(item));
                        }
                    }
                    else
                    {
                        throw new Exception("不支持的类型");
                    }
                }
            }
            else
            {
                if (type == FieldType.Byte)
                {
                    result.Set(fieldRuntime.Define.Name, (byte)fieldRuntime.Value);
                }
                else if (type == FieldType.Boolean)
                {
                    result.Set(fieldRuntime.Define.Name, (bool)fieldRuntime.Value);
                }
                else if (type == FieldType.Char)
                {
                    result.Set(fieldRuntime.Define.Name, (char)fieldRuntime.Value);
                }
                else if (type == FieldType.Int32)
                {
                    result.Set(fieldRuntime.Define.Name, (int)fieldRuntime.Value);
                }
                else if (type == FieldType.Int64)
                {
                    result.Set(fieldRuntime.Define.Name, (long)fieldRuntime.Value);
                }
                else if (type == FieldType.Float)
                {
                    result.Set(fieldRuntime.Define.Name, (float)fieldRuntime.Value);
                }
                else if (type == FieldType.Double)
                {
                    result.Set(fieldRuntime.Define.Name, (double)fieldRuntime.Value);
                }
                else if (type == FieldType.Guid)
                {
                    result.Set(fieldRuntime.Define.Name, ((Guid)fieldRuntime.Value).ToString());
                }
                else if (type == FieldType.DateTime)
                {
                    result.Set(fieldRuntime.Define.Name, new DateTime((long)fieldRuntime.Value));
                }
                else if (type == FieldType.MD5)
                {
                    result.Set(fieldRuntime.Define.Name, Util.BytesToHexString((byte[])fieldRuntime.Value));
                }
                else if (type == FieldType.ReferneceString)
                {
                    result.Set(fieldRuntime.Define.Name, await db.StringHashSet.Read((long)fieldRuntime.Value));
                }
                else
                {
                    throw new Exception("不支持的类型");
                }
            }
        }
        return result;
    }

    public async Task DeserializeFromNewJson(Database db,Json record,bool isBorrowString)
    {
        if (Fields == null)
        {
            throw new Exception("Fields is null");
        }
        var masterField = Fields.First(item => item.Define.MapType == FieldMapType.Master);
        Master = Guid.Empty;
        if (record.TryGet(masterField.Define.Name, out var masterElement))
        {
            Master = masterElement.AsGuid;
        }
        if (Master.Equals(Guid.Empty))
        {
            Master = Guid.NewGuid();
        }
        for (int i = 0; i < Fields.Length; i++)
        {
            if (Fields[i].Define.MapType == FieldMapType.Master)
            {
                Fields[i].Value = Master;
                continue;
            }
            await Fields[i].DeserializeFromJson(db, record.Get(Fields[i].Define.Name,Json.Null), isBorrowString);
        }
    }

    public async Task DeserializeFromOldJson(Database db,Json document,bool isBorrowString)
    {
        if (Fields == null)
        {
            throw new Exception("Fields is null");
        }
        var masterField = Fields.First(item => item.Define.MapType == FieldMapType.Master);
        if(document.TryGet(masterField.Define.Name, out var masterElement))
        {
            Master = masterElement.AsGuid;
        }
        else
        {
            throw new Exception("master is not found");
        }
        for(int i = 0; i < Fields.Length; i++)
        {
            if (Fields[i].Define.MapType == FieldMapType.Master)
            {
                Fields[i].Value = Master;
                continue;
            }
            await Fields[i].DeserializeFromJson(db, document.Get(Fields[i].Define.Name), isBorrowString);
        }
    }

    public void DeserializeFromBuffer(byte[] buffer, int offset)
    {
        if (Fields == null)
        {
            throw new Exception("Fields is null");
        }
        for (int i = 0; i < Fields.Length; i++)
        {
            offset += Fields[i].DeserializeFromBuffer(buffer, offset);
        }
    }

    public void SerializeToBuffer(byte[] buffer, int offset)
    {
        if (Fields == null)
        {
            throw new Exception("Fields is null");
        }
        for (int i = 0; i < Fields.Length; i++)
        {
            offset += Fields[i].SerializeToBuffer(buffer, offset);
        }
    }

    /// <summary>
    /// 释放引用
    /// <para>现在主要是引用字符串</para>
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task ReleaseReference(Database db)
    {
        if (Fields == null)
        {
            throw new Exception("Fields is null");
        }
        foreach (var fieldRuntime in Fields)
        {
            await fieldRuntime.ReleaseReference(db);
        }
    }

    public void Dispose()
    {
        if (Fields != null)
        {
            foreach (var fieldRuntime in Fields)
            {
                fieldRuntime.Dispose();
            }
            Fields = null;
        }
    }

    public string ToString(bool indented)
    {
        Json self = Json.NewObject();
        self["Master"] = Master.ToString();
        var fieldsJson = self.GetOrCreateArray("Fields");
        if (Fields != null)
        {
            foreach (var fieldRuntime in Fields)
            {
                fieldRuntime.SerializeToJson(fieldsJson.AddObject());
            }
        }
        return self.ToString(indented);
    }
}

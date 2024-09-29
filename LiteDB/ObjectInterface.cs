using System.Text.Json;
using TidyHPC.LiteDB.Metas;
using TidyHPC.LiteJson;

namespace TidyHPC.LiteDB;

/// <summary>
/// 对象定义
/// </summary>
public class ObjectInterface
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ObjectInterface()
    {
        FullName = string.Empty;
        Fields = new();
    }

    /// <summary>
    /// 名称
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// 字段组
    /// </summary>
    public List<Field> Fields { get; }

    /// <summary>
    /// 添加字段
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="arrayLength"></param>
    /// <param name="mapType">映射类型</param>
    internal void AddField(string name, FieldType type,int arrayLength,FieldMapType mapType)
    {
        Fields.Add(new Field
        {
            Name = name,
            Type = type,
            ArrayLength = arrayLength,
            MapType = mapType
        });
    }

    /// <summary>
    /// 添加主键
    /// </summary>
    /// <param name="name"></param>
    public void AddMasterField(string name)
    {
        Fields.Add(new Field
        {
            Name = name,
            Type = FieldType.Guid,
            ArrayLength = 1,
            MapType = FieldMapType.Master
        });
    }

    /// <summary>
    /// 添加索引，支持的字段类型有限，单字段目前只支持ReferneceString，Guid，DateTime，MD5
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    public void AddIndexField(string name,FieldType type)
    {
        Fields.Add(new Field
        {
            Name = name,
            Type = type,
            ArrayLength = 1,
            MapType = FieldMapType.Index
        });
    }

    /// <summary>
    /// 添加字段
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    public void AddField(string name, FieldType type)
    {
        Fields.Add(new Field
        {
            Name = name,
            Type = type,
            ArrayLength = 1,
            MapType = FieldMapType.None
        });
    }

    /// <summary>
    /// 添加字段
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="arrayLength"></param>
    public void AddField(string name, FieldType type, int arrayLength)
    {
        Fields.Add(new Field
        {
            Name = name,
            Type = type,
            ArrayLength = arrayLength,
            MapType = FieldMapType.None
        });
    }

    /// <summary>
    /// 添加字段
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="arrayLength"></param>
    public void AddArrayField(string name, FieldType type, int arrayLength)
    {
        Fields.Add(new Field
        {
            Name = name,
            Type = type,
            ArrayLength = arrayLength,
            MapType = FieldMapType.None
        });
    }

    /// <summary>
    /// 添加索引，支持的字段类型有限，数组字段目前只支持Char[8],Char[16],Char[32],Char[64],Char[128],Char[256]
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="arrayLength"></param>
    public void AddIndexField(string name, FieldType type, int arrayLength)
    {
        Fields.Add(new Field
        {
            Name = name,
            Type = type,
            ArrayLength = arrayLength,
            MapType = FieldMapType.Index
        });
    }

    /// <summary>
    /// 添加字段
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="mapType">映射类型</param>
    public void AddField(string name, FieldType type, FieldMapType mapType)
    {
        Fields.Add(new Field
        {
            Name = name,
            Type = type,
            ArrayLength = 1,
            MapType = mapType
        });
    }

    /// <summary>
    /// 添加字段
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="arrayLength"></param>
    /// <param name="mapType">映射类型</param>
    public void AddField(string name, FieldType type, FieldMapType mapType, int arrayLength)
    {
        Fields.Add(new Field
        {
            Name = name,
            Type = type,
            ArrayLength = arrayLength,
            MapType = mapType
        });
    }

    /// <summary>
    /// 处理自身
    /// </summary>
    /// <param name="onSelf"></param>
    /// <returns></returns>
    public ObjectInterface Initialize(Action<ObjectInterface> onSelf)
    {
        onSelf(this);
        return this;
    }

    /// <summary>
    /// 获取大小
    /// </summary>
    /// <returns></returns>
    public int GetSize()
    {
        int sum = 0;
        foreach (var field in Fields)
        {
            sum += field.GetSize();
        }
        return sum;
    }

    /// <summary>
    /// 校验数据类型
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    public void Validate(Json record)
    {
        bool result = true;
        List<string> invalidMessages = new();
        //如果字段映射类型是Master，则要求字段类型必须是Guid
        var masterFields = Fields.Where(f => f.MapType == FieldMapType.Master);
        foreach (var field in masterFields)
        {
            if (field.Type != FieldType.Guid || field.ArrayLength > 1)
            {
                invalidMessages.Add($"{field.Name}字段映射类型是Master，字段类型必须是Guid，ArrayLength必须是1");
            }
        }
        //校验字段数量，Master字段为系统字段，不参与校验
        var validateFields = Fields.Where(f => f.MapType != FieldMapType.Master).ToList();
        //校验字段类型
        foreach (var field in validateFields)
        {
            if (!record.TryGet(field.Name, out var property))
            {
                invalidMessages.Add($"{field.Name}字段不存在");
                continue;
            }
            if (field.ArrayLength > 1)
            {
                if (field.Type == FieldType.Char)
                {
                    if (property.IsString==false)
                    {
                        invalidMessages.Add($"{field.Name}字段类型错误，应为String");
                        result = false;
                        continue;
                    }
                    else
                    {
                        var value = property.AsString;
                        if(value == null)
                        {
                            throw new Exception($"{field.Name}字段值不能为null");
                        }
                        if (value.Length > field.ArrayLength)
                        {
                            invalidMessages.Add($"{field.Name}字段长度错误，应小于等于{field.ArrayLength}");
                            result = false;
                            continue;
                        }
                    }
                }
                else
                {
                    if (property.IsArray==false)
                    {
                        invalidMessages.Add($"{field.Name}字段类型错误，应为Array<{field.Type}>");
                        result = false;
                        continue;
                    }
                    else
                    {
                        if (property.Count > field.ArrayLength)
                        {
                            invalidMessages.Add($"{field.Name}字段长度错误，应小于等于{field.ArrayLength}");
                            result = false;
                            continue;
                        }
                        foreach (var item in property.GetArrayEnumerable())
                        {
                            if (!CheckJsonElementIsMatchFieldType(field, item, invalidMessages))
                            {
                                result = false;
                            }
                        }
                    }
                }
            }
            else
            {
                if (!CheckJsonElementIsMatchFieldType(field, property, invalidMessages))
                {
                    result = false;
                }
            }
        }
        if (!result)
        {
            throw new Exception(string.Join(",", invalidMessages));
        }
    }

    /// <summary>
    /// 从JsonDocument中获取Master字段值
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Guid GetMasterByJsonDocument(Json record)
    {
        foreach(var field in Fields)
        {
            if (field.MapType == FieldMapType.Master)
            {
                if (!record.TryGet(field.Name, out var property))
                {
                    throw new Exception($"{field.Name}字段不存在");
                }
                if (field.Type != FieldType.Guid || field.ArrayLength > 1)
                {
                    throw new Exception($"{field.Name}字段映射类型是Master，字段类型必须是Guid，ArrayLength必须是1");
                }
                return property.AsGuid;
            }
        }
        throw new Exception("未找到Master字段");
    }

    /// <summary>
    /// 映射FieldType到JsonValueKind
    /// </summary>
    public static Dictionary<FieldType, JsonValueKind[]> MapFieldTypeToJsonValueKinds { get; } = new()
    {
        {FieldType.Byte,[JsonValueKind.Number] },
        {FieldType.Boolean,[JsonValueKind.True,JsonValueKind.False] },
        {FieldType.Char,[JsonValueKind.String] },
        {FieldType.Int32,[JsonValueKind.Number] },
        {FieldType.Float,[JsonValueKind.Number] },
        {FieldType.Double,[JsonValueKind.Number] },
        {FieldType.ReferneceString,[JsonValueKind.String] },
        {FieldType.Guid,[JsonValueKind.String] },
        {FieldType.MD5,[JsonValueKind.String] },
        {FieldType.DateTime,[JsonValueKind.String] },
        {FieldType.Int64,[JsonValueKind.Number] },
    };

    /// <summary>
    /// 判断JsonElement是否匹配字段类型
    /// </summary>
    /// <param name="field"></param>
    /// <param name="property"></param>
    /// <param name="invalidMessages"></param>
    /// <returns></returns>
    public static bool CheckJsonElementIsMatchFieldType(Field field,Json property, List<string> invalidMessages)
    {
        if(MapFieldTypeToJsonValueKinds.TryGetValue(field.Type,out var valueKinds))
        {
            if(!valueKinds.Contains(property.GetValueKind()))
            {
                if (field.ArrayLength > 1)
                {
                    invalidMessages.Add($"{field.Name}字段类型错误，应为Array<{string.Join("/", valueKinds)}>");
                }
                else
                {
                    invalidMessages.Add($"{field.Name}字段类型错误，应为{string.Join("/", valueKinds)}");
                }
                
                return false;
            }
        }
        else
        {
            invalidMessages.Add($"{field.Name}字段类型未知");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 序列化到Json
    /// </summary>
    /// <param name="self"></param>
    public void SerializeToJson(LiteJson.Json self)
    {
        self["FullName"] = FullName;
        var fieldsJson = self.GetOrCreateArray("Fields");
        foreach (var field in Fields)
        {
            var fieldJson = fieldsJson.AddObject();
            field.SerializeToJson(fieldJson);
        }
    }

    /// <summary>
    /// 从Json中反序列化
    /// </summary>
    /// <param name="self"></param>
    public void DeserializeFromJson(LiteJson.Json self)
    {
        FullName = self.Read("FullName", string.Empty);
        var fieldsJson = self.GetOrCreateArray("Fields");
        foreach (var fieldJson in fieldsJson)
        {
            var field = new Field();
            field.DeserializeFromJson(fieldJson);
            Fields.Add(field);
        }
    }

    /// <summary>
    /// Convert to string with json format
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        LiteJson.Json self=LiteJson.Json.NewObject();
        SerializeToJson(self);
        return self.ToString(true);
    }

    /// <summary>
    /// Convert to string with json format
    /// </summary>
    /// <param name="indent"></param>
    /// <returns></returns>
    public string ToString(bool indent)
    {
        LiteJson.Json self = LiteJson.Json.NewObject();
        SerializeToJson(self);
        return self.ToString(indent);
    }

}



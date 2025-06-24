using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TidyHPC.Routers.Urls;

/// <summary>
/// TypeScript接口属性
/// </summary>
public class TypeScriptInterfaceAttribute:Attribute
{
    /// <summary>
    /// TypeScript接口定义
    /// </summary>
    public string? TypeScriptInterface { get; }

    /// <summary>
    /// TypeScript接口属性构造函数
    /// </summary>
    /// <param name="typeScriptInterface"></param>
    public TypeScriptInterfaceAttribute(string typeScriptInterface)
    {
        TypeScriptInterface = typeScriptInterface;
    }
}

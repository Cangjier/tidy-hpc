using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TidyHPC.LiteDB.BasicValues;

namespace TidyHPC.Routers.Args;

/// <summary>
/// 参数索引
/// </summary>
public class ArgsIndexAttribute : Attribute
{
    /// <summary>
    /// 索引位置
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// 参数索引
    /// </summary>
    /// <param name="index"></param>
    public ArgsIndexAttribute(int index)
    {
        Index = index;
    }

    /// <summary>
    /// 参数索引
    /// </summary>
    public ArgsIndexAttribute()
    {
        Index = -1;
    }
}

/// <summary>
/// 全量的参数
/// </summary>
public class ArgsAttribute : Attribute
{
}

/// <summary>
/// 剩余的参数
/// </summary>
public class SubArgsAttribute : Attribute
{
}

/// <summary>
/// 参数使用说明
/// </summary>
public class ArgsUsageAttribute : Attribute
{
    /// <summary>
    /// 使用说明
    /// </summary>
    public string Usage { get; }

    /// <summary>
    /// 参数使用说明
    /// </summary>
    /// <param name="usage"></param>
    public ArgsUsageAttribute(string usage)
    {
        Usage = usage;
    }
}

/// <summary>
/// 参数入口
/// </summary>
public class ArgsEntryAttribute : Attribute
{
}

internal class ArgsUtils
{
    public static bool IsBooleanParameter(string arg, ParameterInfo[] parameters)
    {
        foreach (var parameter in parameters)
        {
            var aliases = parameter.GetCustomAttribute<ArgsAliasesAttribute>()?.Aliases;
            if (aliases != null)
            {
                if (aliases.Contains(arg))
                {
                    return parameter.ParameterType == typeof(bool);
                }
            }
        }
        return false;
    }
}

/// <summary>
/// 参数访问器
/// </summary>
/// <param name="Arguments"></param>
public record ArgsVisitor(string[] Arguments)
{
    /// <summary>
    /// 第一个参数是否为命令
    /// </summary>
    public bool IsFirstCommand { get; set; }

    /// <summary>
    /// 命令
    /// </summary>
    public string? Command
    {
        get
        {
            if (Arguments.Length == 0)
            {
                return null;
            }
            if (Arguments[0].StartsWith('-'))
            {
                return null;
            }
            return Arguments[0];
        }
    }

    /// <summary>
    /// 尝试获取参数
    /// </summary>
    /// <param name="optionAliases"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetBoolean(string[] optionAliases, out bool value)
    {
        value = false;
        var index = Array.FindIndex(Arguments, x => optionAliases.Contains(x));
        if (index == -1)
        {
            return false;
        }
        if (index + 1 < Arguments.Length)
        {
            var argumentValue = Arguments[index + 1];
            if (argumentValue == "true")
            {
                value = true;
            }
            else if (argumentValue == "false")
            {
                value = false;
            }
            else
            {
                value = true;
            }
        }
        else
        {
            value = true;
        }
        return true;
    }

    /// <summary>
    /// 尝试获取参数
    /// </summary>
    /// <param name="optionAliases"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetString(string[] optionAliases, out string value)
    {
        value = string.Empty;
        var index = Array.FindIndex(Arguments, x => optionAliases.Contains(x));
        if (index == -1)
        {
            return false;
        }
        if (index + 1 < Arguments.Length)
        {
            value = Arguments[index + 1];
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取抛除命令以及--标记的参数
    /// </summary>
    /// <returns></returns>
    public string[] GetSubArgs(int index, ParameterInfo[] parameters)
    {
        List<string> result = [];
        var currentIndex = 0;
        var startIndex = IsFirstCommand ? 1 : 0;
        for (int i = startIndex; i < Arguments.Length; i++)
        {
            var item = Arguments[i];
            if (item.StartsWith('-'))
            {
                if (ArgsUtils.IsBooleanParameter(item, parameters))
                {
                    if (i + 1 < Arguments.Length)
                    {
                        var argumentValue = Arguments[i + 1];
                        if (argumentValue == "true" || argumentValue == "false")
                        {
                            // 如果当前参数是boolean，并且下一个参数是true或者false，那么下一个参数就是值，跳过
                            i++;
                        }
                        else
                        {
                            // 如果当前参数是boolean，并且下一个参数不是true或者false，下一个参数是新参数
                        }
                    }
                }
                else
                {
                    // 如果当前参数不是boolean，那么下一个参数就是值，跳过
                    i++;
                }
                continue;
            }
            if (currentIndex >= index)
            {
                result.Add(item);
            }
            currentIndex++;
        }
        return [.. result];
    }

    /// <summary>
    /// 获取指定索引的参数
    /// </summary>
    /// <param name="index"></param>
    /// <param name="parameters"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetAt(int index, ParameterInfo[] parameters, out string value)
    {
        var currentIndex = 0;
        var startIndex = IsFirstCommand ? 1 : 0;
        for (int i = startIndex; i < Arguments.Length; i++)
        {
            var item = Arguments[i];
            if (item.StartsWith('-'))
            {
                if (ArgsUtils.IsBooleanParameter(item, parameters))
                {
                    if (i + 1 < Arguments.Length)
                    {
                        var argumentValue = Arguments[i + 1];
                        if (argumentValue == "true" || argumentValue == "false")
                        {
                            // 如果当前参数是boolean，并且下一个参数是true或者false，那么下一个参数就是值，跳过
                            i++;
                        }
                        else
                        {
                            // 如果当前参数是boolean，并且下一个参数不是true或者false，下一个参数是新参数
                        }
                    }
                }
                else
                {
                    // 如果当前参数不是boolean，那么下一个参数就是值，跳过
                    i++;
                }
                continue;
            }
            if (currentIndex == index)
            {
                value = item;
                return true;
            }
            currentIndex++;
        }
        value = string.Empty;
        return false;
    }
}

/// <summary>
/// 参数路由记录
/// </summary>
/// <param name="CommandPattern"></param>
/// <param name="CommandRegex"></param>
/// <param name="Handler"></param>
public record ArgsRouterRecord(string CommandPattern, Regex CommandRegex, Func<ArgsVisitor, Task> Handler);

/// <summary>
/// 参数路由，如命令行参数
/// </summary>
public class ArgsRouter
{
    private ConcurrentDictionary<string, ArgsRouterRecord> RealMap { get; } = new();

    private ConcurrentDictionary<string, ArgsRouterRecord> HotMap { get; } = new();

    /// <summary>
    /// 注册路由
    /// </summary>
    /// <param name="commandPattern"></param>
    /// <param name="handler"></param>
    /// <exception cref="Exception"></exception>
    public void RegisterNative(string commandPattern, Func<ArgsVisitor, Task> handler)
    {
        if (RealMap.ContainsKey(commandPattern))
        {
            RealMap[commandPattern] = new ArgsRouterRecord(commandPattern, new Regex(commandPattern), handler);
        }
        else
        {
            throw new Exception($"路由{commandPattern}已经存在");
        }
    }

    /// <summary>
    /// 注册默认路由
    /// </summary>
    /// <param name="handler"></param>
    public void RegisterNative(Func<ArgsVisitor, Task> handler)
    {
        RegisterNative(string.Empty, handler);
    }

    /// <summary>
    /// 通过反射注册路由
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="onInstance"></param>
    public void RegisterClass(Type handler, Func<object> onInstance)
    {
        var commandAttribute = handler.GetCustomAttribute<ArgsAliasesAttribute>();
        if (commandAttribute == null)
        {
            throw new Exception("未找到命令");
        }
        var commandAliases = commandAttribute.Aliases;
        var commandPattern = $"^({string.Join('|', commandAliases)})$";
        var commandRegex = new Regex(commandPattern);
        var method = handler.GetMethods().FirstOrDefault(x => x.GetCustomAttribute<ArgsEntryAttribute>() != null);
        if (method == null)
        {
            throw new Exception("未找到入口");
        }
        RealMap.TryAdd(commandPattern, new ArgsRouterRecord(commandPattern, commandRegex, async visitor =>
        {
            var instance = onInstance();
            var properties = handler.GetProperties().Where(x => x.GetCustomAttribute<ArgsAliasesAttribute>() != null);
            // 校验参数
            foreach (var property in properties)
            {
                var aliases = property.GetCustomAttribute<ArgsAliasesAttribute>()!.Aliases;
                var isOptional = property.GetCustomAttribute<OptionalAttribute>() != null;
                if (property.PropertyType == typeof(bool))
                {
                    if (visitor.TryGetBoolean(aliases, out var value))
                    {
                        property.SetValue(instance, value);
                    }
                    else
                    {
                        if (isOptional == false) throw new Exception($"参数{string.Join(',', aliases)}未找到");
                    }
                }
                else
                {
                    if (visitor.TryGetString(aliases, out var value))
                    {
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(instance, value);
                        }
                        else if (property.PropertyType == typeof(int))
                        {
                            if (int.TryParse(value, out var intValue))
                            {
                                property.SetValue(instance, intValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为整数");
                            }
                        }
                        else if (property.PropertyType == typeof(double))
                        {
                            if (double.TryParse(value, out var doubleValue))
                            {
                                property.SetValue(instance, doubleValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为浮点数");
                            }
                        }
                        else if (property.PropertyType == typeof(float))
                        {
                            if (float.TryParse(value, out var floatValue))
                            {
                                property.SetValue(instance, floatValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为浮点数");
                            }
                        }
                        else if (property.PropertyType == typeof(decimal))
                        {
                            if (decimal.TryParse(value, out var decimalValue))
                            {
                                property.SetValue(instance, decimalValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为浮点数");
                            }
                        }
                        else if (property.PropertyType == typeof(long))
                        {
                            if (long.TryParse(value, out var longValue))
                            {
                                property.SetValue(instance, longValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为整数");
                            }
                        }
                        else if (property.PropertyType == typeof(short))
                        {
                            if (short.TryParse(value, out var shortValue))
                            {
                                property.SetValue(instance, shortValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为整数");
                            }
                        }
                        else if (property.PropertyType == typeof(byte))
                        {
                            if (byte.TryParse(value, out var byteValue))
                            {
                                property.SetValue(instance, byteValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为整数");
                            }
                        }
                        else if (property.PropertyType == typeof(char))
                        {
                            if (char.TryParse(value, out var charValue))
                            {
                                property.SetValue(instance, charValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为字符");
                            }
                        }
                        else if (property.PropertyType == typeof(DateTime))
                        {
                            if (DateTime.TryParse(value, out var dateTimeValue))
                            {
                                property.SetValue(instance, dateTimeValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为日期");
                            }
                        }
                        else if (property.PropertyType == typeof(Guid))
                        {
                            if (Guid.TryParse(value, out var guidValue))
                            {
                                property.SetValue(instance, guidValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为GUID");
                            }
                        }
                        else if (property.PropertyType == typeof(TimeSpan))
                        {
                            if (TimeSpan.TryParse(value, out var timeSpanValue))
                            {
                                property.SetValue(instance, timeSpanValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为时间间隔");
                            }
                        }
                        else if (property.PropertyType.IsEnum)
                        {
                            if (Enum.TryParse(property.PropertyType, value, out var enumValue))
                            {
                                property.SetValue(instance, enumValue);
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为枚举");
                            }
                        }
                        else
                        {
                            throw new Exception($"参数`{string.Join(',', aliases)}`无法转换的类型{property.PropertyType}");
                        }
                    }
                    else
                    {
                        if (isOptional == false) throw new Exception($"参数`{string.Join(',', aliases)}`未找到");
                    }
                }
            }
            var result = method.Invoke(instance, null);
            if (result is Task task)
            {
                await task;
            }
        }));
    }

    /// <summary>
    /// 通过反射注册路由
    /// </summary>
    /// <param name="handler"></param>
    public void RegisterClass(Type handler)
    {
        RegisterClass(handler, () => Activator.CreateInstance(handler)!);
    }

    /// <summary>
    /// 通过反射注册路由
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void RegisterClass<T>() where T : class
    {
        RegisterClass(typeof(T));
    }

    /// <summary>
    /// 通过反射注册路由
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instanceHandler"></param>
    public void RegisterClass<T>(T instanceHandler) where T : class
    {
        RegisterClass(instanceHandler.GetType(), () => instanceHandler);
    }

    /// <summary>
    /// 通过反射注册路由
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instanceHandler"></param>
    public void RegisterClass<T>(Func<T> instanceHandler) where T : class
    {
        RegisterClass(typeof(T), () => instanceHandler());
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="method"></param>
    /// <param name="onInstance"></param>
    /// <param name="onCommandPattern"></param>
    public void Register(MethodInfo method, Func<string> onCommandPattern, Func<object?> onInstance)
    {
        var commandPattern = onCommandPattern();
        var commandRegex = new Regex(commandPattern);
        var parameters = method.GetParameters();
        RealMap.TryAdd(commandPattern, new ArgsRouterRecord(commandPattern, commandRegex, async visitor =>
        {
            if (commandPattern != string.Empty)
            {
                visitor.IsFirstCommand = true;
            }
            var argsIndexAttributeCount = 0;
            var instance = onInstance();
            var arguments = new object?[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var aliases = parameter.GetCustomAttribute<ArgsAliasesAttribute>()?.Aliases;
                var isOptional = parameter.HasDefaultValue;
                var containsArgsAttribute = parameter.GetCustomAttribute<ArgsAttribute>() != null;
                var containsSubArgsAttribute = parameter.GetCustomAttribute<SubArgsAttribute>() != null;
                if (aliases == null)
                {
                    if (containsArgsAttribute)
                    {
                        arguments[i] = visitor.Arguments;
                    }
                    else if (containsSubArgsAttribute)
                    {
                        arguments[i] = visitor.GetSubArgs(argsIndexAttributeCount, parameters);
                    }
                    else
                    {
                        var index = parameter.GetCustomAttribute<ArgsIndexAttribute>()?.Index;
                        if (index == null)
                        {
                            throw new Exception("参数未指定索引");
                        }
                        argsIndexAttributeCount++;
                        if (index == -1)
                        {
                            index = argsIndexAttributeCount - 1;
                        }
                        if (visitor.TryGetAt(index.Value, parameters, out string indexValue) == false)
                        {
                            if (isOptional == false) 
                                throw new Exception($"参数{indexValue}未找到");
                            arguments[i] = parameter.DefaultValue;
                            continue;
                        }
                        if (indexValue.TryConvertTo(parameter.ParameterType, out var parameterValue))
                        {
                            arguments[i] = parameterValue;
                        }
                        else
                        {
                            throw new Exception($"参数{indexValue}无法转换为{parameter.ParameterType}");
                        }
                    }
                }
                else
                {
                    if (parameter.ParameterType == typeof(bool))
                    {
                        if (visitor.TryGetBoolean(aliases, out var value))
                        {
                            arguments[i] = value;
                        }
                        else
                        {
                            if (isOptional == false) throw new Exception($"参数{string.Join(',', aliases)}未找到");
                            arguments[i] = parameter.DefaultValue;
                            continue;
                        }
                    }
                    else
                    {
                        if (visitor.TryGetString(aliases, out var value))
                        {
                            if (value.TryConvertTo(parameter.ParameterType, out var parameterValue))
                            {
                                arguments[i] = parameterValue;
                            }
                            else
                            {
                                throw new Exception($"参数{string.Join(',', aliases)}无法转换为{parameter.ParameterType}");
                            }
                        }
                        else
                        {
                            if (isOptional == false) throw new Exception($"参数`{string.Join(',', aliases)}`未找到");
                            arguments[i] = parameter.DefaultValue;
                        }
                    }
                }
            }
            var result = method.Invoke(instance, arguments);
            if (result is Task task)
            {
                await task;
            }
        }));
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="commandAliases"></param>
    /// <param name="func"></param>
    public void Register(string[] commandAliases,Delegate func)
    {
        Register(func.Method, () =>
        {
            var commandPattern = $"^({string.Join('|', commandAliases)})$";
            return commandPattern;
        }, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="commandAliases"></param>
    /// <param name="method"></param>
    public void Register(string[] commandAliases, MethodInfo method)
    {
        Register(method, () =>
        {
            var commandPattern = $"^({string.Join('|', commandAliases)})$";
            return commandPattern;
        }, () => method.DeclaringType != null ? Activator.CreateInstance(method.DeclaringType) : null);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="commandAliases"></param>
    /// <param name="onInstance"></param>
    /// <param name="method"></param>
    public void Register(string[] commandAliases, MethodInfo method, Func<object?> onInstance)
    {
        Register(method, () =>
        {
            var commandPattern = $"^({string.Join('|', commandAliases)})$";
            return commandPattern;
        }, onInstance);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="func"></param>
    public void Register(Delegate func)
    {
        Register(func.Method, () =>
        {
            return string.Empty;
        }, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="method"></param>
    public void Register(MethodInfo method)
    {
        Register(method, () =>
        {
            var commandAttribute = method.GetCustomAttribute<ArgsAliasesAttribute>();
            if (commandAttribute != null)
            {
                var commandAliases = commandAttribute.Aliases;
                var commandPattern = $"^({string.Join('|', commandAliases)})$";
                return commandPattern;
            }
            else
            {
                return string.Empty;
            }
        }, () => method.DeclaringType != null ? Activator.CreateInstance(method.DeclaringType) : null);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="method"></param>
    /// <param name="onInstance"></param>
    public void Register(MethodInfo method, Func<object?> onInstance)
    {
        Register(method, () =>
        {
            var commandAttribute = method.GetCustomAttribute<ArgsAliasesAttribute>();
            if (commandAttribute != null)
            {
                var commandAliases = commandAttribute.Aliases;
                var commandPattern = $"^({string.Join('|', commandAliases)})$";
                return commandPattern;
            }
            else
            {
                return string.Empty;
            }
        }, onInstance);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="func"></param>
    /// <exception cref="Exception"></exception>
    public void Register(Func<Task> func)
    {
        Register(func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="func"></param>
    /// <exception cref="Exception"></exception>
    public void Register<T>(Func<Task> func)
    {
        Register(func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="func"></param>
    public void Register<T1, T2>(Func<T1, T2, Task> func)
    {
        Register(func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="func"></param>
    public void Register<T1, T2, T3>(Func<T1, T2, T3, Task> func)
    {
        Register(func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> func)
    {
        Register(func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> func)
    {
        Register(func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> func)
    {
        Register(func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, Task> func)
    {
        Register(func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> func)
    {
        Register(func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="commandAliases"></param>
    /// <param name="func"></param>
    public void Register(string[] commandAliases, Func<Task> func)
    {
        Register(commandAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="commandAliases"></param>
    /// <param name="func"></param>
    public void Register<T>(string[] commandAliases, Func<T, Task> func)
    {
        Register(commandAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="commandAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2>(string[] commandAliases, Func<T1, T2, Task> func)
    {
        Register(commandAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="commandAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3>(string[] commandAliases, Func<T1, T2, T3, Task> func)
    {
        Register(commandAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <param name="commandAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4>(string[] commandAliases, Func<T1, T2, T3, T4, Task> func)
    {
        Register(commandAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <param name="commandAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5>(string[] commandAliases, Func<T1, T2, T3, T4, T5, Task> func)
    {
        Register(commandAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <param name="commandAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5, T6>(string[] commandAliases, Func<T1, T2, T3, T4, T5, T6, Task> func)
    {
        Register(commandAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <param name="commandAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5, T6, T7>(string[] commandAliases, Func<T1, T2, T3, T4, T5, T6, T7, Task> func)
    {
        Register(commandAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <param name="commandAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5, T6, T7, T8>(string[] commandAliases, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> func)
    {
        Register(commandAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 路由
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task Route(string[] args)
    {
        var visitor = new ArgsVisitor(args);
        var command = visitor.Command ?? string.Empty;
        if (HotMap.ContainsKey(command))
        {
            await HotMap[command].Handler(visitor);
        }
        else
        {
            foreach (var item in RealMap)
            {
                if (item.Value.CommandRegex.IsMatch(command))
                {
                    HotMap.TryAdd(command, item.Value);
                    await item.Value.Handler(visitor);
                    return;
                }
            }
            if (RealMap.TryGetValue(string.Empty, out var defaultHandler))
            {
                await defaultHandler.Handler(visitor);
            }
            else
            {
                throw new Exception($"未找到路由{command}");
            }
        }
    }
}

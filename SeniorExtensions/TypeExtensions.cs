using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TidyHPC.SeniorExtensions;

/// <summary>
/// Type Extensions
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// 得分方法
    /// </summary>
    public struct ScoreMethodInfo
    {
        /// <summary>
        /// 方法信息
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// 得分
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 声明类型
        /// </summary>
        public Type Declare { get; set; }
    }

    /// <summary>
    /// 得分构造函数信息
    /// </summary>
    public struct ScoreConstructorInfo
    {
        /// <summary>
        /// 构造函数信息
        /// </summary>
        public ConstructorInfo ConstructorInfo { get; set; }

        /// <summary>
        /// 得分
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 申明类型
        /// </summary>
        public Type Declare { get; set; }
    }

    /// <summary>
    /// Find Method
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static MethodInfo? FindStaticMethod(this Type self, string methodName)
    {
        var lastType = self;
        while (true)
        {
            MethodInfo[] methods = lastType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (method.Name.Split('.').Last() == methodName)
                {
                    return method;
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }

    /// <summary>
    /// Find Method
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindStaticMethod(this Type self, string methodName, out MethodInfo? result)
    {
        result = FindStaticMethod(self, methodName);
        return result != null;
    }

    /// <summary>
    /// Find Method
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodName"></param>
    /// <param name="returnType"></param>
    /// <param name="parameterTypes"></param>
    /// <returns></returns>
    public static MethodInfo? FindStaticMethod(this Type self, string methodName, Type? returnType, Type[]? parameterTypes)
    {
        var lastType = self;
        while (true)
        {
            MethodInfo[] methods = lastType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var method in methods)
            {
                if (method.Name.Split('.').Last() == methodName)
                {
                    if ((returnType == null || returnType == method.ReturnType) && (parameterTypes == null || Is(parameterTypes, method.GetParameters())))
                    {
                        return method;
                    }
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }

    /// <summary>
    /// Find Method
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNameRegex"></param>
    /// <param name="returnType"></param>
    /// <param name="parameterTypes"></param>
    /// <returns></returns>
    public static MethodInfo? FindStaticMethod(this Type self, Regex methodNameRegex, Type? returnType, Type[]? parameterTypes)
    {
        var lastType = self;
        while (true)
        {
            MethodInfo[] methods = lastType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (methodNameRegex.IsMatch(method.Name.Split('.').Last()))
                {
                    if ((returnType == null || returnType == method.ReturnType) && (parameterTypes == null || Is(parameterTypes, method.GetParameters())))
                    {
                        return method;
                    }
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }

    /// <summary>
    /// Find Method
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodName"></param>
    /// <param name="returnType"></param>
    /// <param name="parameterTypes"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindStaticMethod(this Type self, string methodName, Type? returnType, Type[]? parameterTypes, [NotNullWhen(true)] out MethodInfo? result)
    {
        result = FindStaticMethod(self, methodName, returnType, parameterTypes);
        return result != null;
    }
    /// <summary>
    /// Find Method
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNameRegex"></param>
    /// <param name="returnType"></param>
    /// <param name="parameterTypes"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindStaticMethod(this Type self, Regex methodNameRegex, Type? returnType, Type[]? parameterTypes, [NotNullWhen(true)] out MethodInfo? result)
    {
        result = FindStaticMethod(self, methodNameRegex, returnType, parameterTypes);
        return result != null;
    }

    /// <summary>
    /// 查找实例方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <returns></returns>
    public static MethodInfo? FindStaticMethod(this Type self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate)
    {
        var lastType = self;
        while (true)
        {
            MethodInfo[] methods = lastType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (methodNames.Contains(method.Name.Split('.').Last()))
                {
                    if (!onReturnTypePredicate(method.ReturnType))
                    {
                        continue;
                    }
                    var parameters = method.GetParameters();
                    if (!onParametersPredicate(parameters))
                    {
                        continue;
                    }
                    return method;
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }

    /// <summary>
    /// 查找静态方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static bool TryFindStaticMethod(this Type self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, [NotNullWhen(true)] out MethodInfo? method)
    {
        method = FindStaticMethod(self, methodNames, onReturnTypePredicate, onParametersPredicate);
        return method != null;
    }

    /// <summary>
    /// 查找实例方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static MethodInfo? FindStaticMethod(this IEnumerable<Type> self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, ref Type? finalType)
    {
        foreach (var type in self)
        {
            var method = FindStaticMethod(type, methodNames, onReturnTypePredicate, onParametersPredicate);
            if (method != null)
            {
                finalType = type;
                return method;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找静态方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="method"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static bool TryFindStaticMethod(this IEnumerable<Type> self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, [NotNullWhen(true)] out MethodInfo? method, ref Type? finalType)
    {
        method = FindStaticMethod(self, methodNames, onReturnTypePredicate, onParametersPredicate, ref finalType);
        return method != null;
    }

    /// <summary>
    /// 查找静态方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodName"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <returns></returns>
    public static MethodInfo[] FindStaticMethods(this Type self, string methodName, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate)
    {
        List<MethodInfo> result = [];
        var lastType = self;
        while (true)
        {
            MethodInfo[] methods = lastType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (method.Name.Split('.').Last() == methodName)
                {
                    if (!onReturnTypePredicate(method.ReturnType))
                    {
                        continue;
                    }
                    var parameters = method.GetParameters();
                    if (!onParametersPredicate(parameters))
                    {
                        continue;
                    }
                    result.Add(method);
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return result.ToArray();
    }

    /// <summary>
    /// 查找静态方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodName"></param>
    /// <param name="onParametersPredicate"></param>
    /// <returns></returns>
    public static ScoreMethodInfo[] FindStaticMethodsByScore(this Type self, string methodName, Func<ParameterInfo[], int> onParametersPredicate)
    {
        return FindMethodsByScore([self], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, [methodName], x => 0, x => 0, onParametersPredicate);
    }

    /// <summary>
    /// 实例
    /// </summary>
    public const BindingFlags Instance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// 静态
    /// </summary>
    public const BindingFlags Static = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    /// <summary>
    /// 查找方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="bindingFlags"></param>
    /// <param name="methodNames"></param>
    /// <param name="onMethod"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <returns></returns>
    public static ScoreMethodInfo[] FindMethodsByScore(this Type[] self, BindingFlags bindingFlags, string[] methodNames, Func<MethodInfo, int> onMethod, Func<Type, int> onReturnTypePredicate, Func<ParameterInfo[], int> onParametersPredicate)
    {
        List<ScoreMethodInfo> result = [];
        foreach (var type in self)
        {
            var lastType = type;
            while (true)
            {
                MethodInfo[] methods = lastType.GetMethods(bindingFlags);
                foreach (var method in methods)
                {
                    if (methodNames.Contains(method.Name.Split('.').Last()))
                    {
                        var methodScore = onMethod(method);
                        if (methodScore == int.MaxValue)
                        {
                            continue;
                        }
                        var returnScore = onReturnTypePredicate(method.ReturnType);
                        if (returnScore == int.MaxValue)
                        {
                            continue;
                        }
                        var parameters = method.GetParameters();
                        var score = onParametersPredicate(parameters);
                        if (score == int.MaxValue)
                        {
                            continue;
                        }
                        result.Add(new()
                        {
                            Score = score + returnScore + methodScore,
                            MethodInfo = method,
                            Declare = type
                        });
                    }
                }
                lastType = lastType.BaseType;
                if (lastType == null) break;
            }
        }

        result.Sort((a, b) => a.Score.CompareTo(b.Score));
        return result.ToArray();
    }

    /// <summary>
    /// 查找方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="bindingFlags"></param>
    /// <param name="onParametersPredicate"></param>
    /// <returns></returns>
    public static ScoreConstructorInfo[] FindConstructorsByScore(this Type[] self, BindingFlags bindingFlags, Func<ParameterInfo[], int> onParametersPredicate)
    {
        List<ScoreConstructorInfo> result = [];
        foreach (var type in self)
        {
            ConstructorInfo[] methods = type.GetConstructors(bindingFlags);
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                var score = onParametersPredicate(parameters);
                if (score == int.MaxValue)
                {
                    continue;
                }
                result.Add(new()
                {
                    Score = score,
                    ConstructorInfo = method,
                    Declare = type
                });
            }
        }
        result.Sort((a, b) => a.Score.CompareTo(b.Score));
        return result.ToArray();
    }

    /// <summary>
    /// 查询构造函数
    /// </summary>
    /// <param name="self"></param>
    /// <param name="onParametersPredicate"></param>
    /// <returns></returns>
    public static ConstructorInfo? FindConstructorByScore(this Type self, Func<ParameterInfo[], int> onParametersPredicate)
    {
        var scoreConstructors = FindConstructorsByScore([self], BindingFlags.Public | BindingFlags.Instance, onParametersPredicate);
        return scoreConstructors.Length > 0 ? scoreConstructors[0].ConstructorInfo : null;
    }

    /// <summary>
    /// 查找方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="bindingFlags"></param>
    /// <param name="methodNames"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <returns></returns>
    public static ScoreMethodInfo[] FindMethodsByScore(this Type self, BindingFlags bindingFlags, string[] methodNames, Func<Type, int> onReturnTypePredicate, Func<ParameterInfo[], int> onParametersPredicate)
    {
        return FindMethodsByScore([self], bindingFlags, methodNames, x => 0, onReturnTypePredicate, onParametersPredicate);
    }

    /// <summary>
    /// 查找方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="bindingFlags"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static MethodInfo? FindMethodByScore(this Type[] self, BindingFlags bindingFlags, string[] methodNames, Func<Type, int> onReturnTypePredicate, Func<ParameterInfo[], int> onParametersPredicate, ref Type? finalType)
    {
        var result = FindMethodsByScore(self, bindingFlags, methodNames, x => 0, onReturnTypePredicate, onParametersPredicate);
        if (result.Length == 0) return null;
        finalType = result[0].Declare;
        return result[0].MethodInfo;
    }

    /// <summary>
    /// 查找方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="bindingFlags"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <returns></returns>
    public static MethodInfo? FindMethodByScore(this Type self, BindingFlags bindingFlags, string[] methodNames, Func<Type, int> onReturnTypePredicate, Func<ParameterInfo[], int> onParametersPredicate)
    {
        var result = FindMethodsByScore(self, bindingFlags, methodNames, onReturnTypePredicate, onParametersPredicate);
        return result.Length > 0 ? result[0].MethodInfo : null;
    }

    /// <summary>
    /// 查找方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static MethodInfo? FindMethodByScore(this Type[] self, string[] methodNames, Func<Type, int> onReturnTypePredicate, Func<ParameterInfo[], int> onParametersPredicate, ref Type? finalType)
    {
        var result = FindMethodsByScore(self, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, methodNames, x => 0, onReturnTypePredicate, onParametersPredicate);
        if (result.Length == 0) return null;
        finalType = result[0].Declare;
        return result[0].MethodInfo;
    }

    /// <summary>
    /// 查找静态方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodName"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindStaticMethods(this Type self, string methodName, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, [NotNullWhen(true)] out MethodInfo[] result)
    {
        result = FindStaticMethods(self, methodName, onReturnTypePredicate, onParametersPredicate);
        return result.Length > 0;
    }

    /// <summary>
    /// 查找实例方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <returns></returns>
    public static MethodInfo? FindMethod(this Type self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate)
    {
        List<Type> types = [];
        var lastType = self;
        while (true)
        {
            types.Add(lastType);
            types.AddRange(lastType.GetInterfaces());
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        foreach (var type in types)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (methodNames.Contains(method.Name.Split('.').Last()))
                {
                    if (!onReturnTypePredicate(method.ReturnType))
                    {
                        continue;
                    }
                    var parameters = method.GetParameters();
                    if (!onParametersPredicate(parameters))
                    {
                        continue;
                    }
                    return method;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 查找方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static bool TryFindMethod(this Type self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, [NotNullWhen(true)] out MethodInfo? method)
    {
        method = FindMethod(self, methodNames, onReturnTypePredicate, onParametersPredicate);
        return method != null;
    }

    /// <summary>
    /// 查找方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static MethodInfo? FindMethod(this IEnumerable<Type> self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, ref Type? finalType)
    {
        foreach (var type in self)
        {
            var method = FindMethod(type, methodNames, onReturnTypePredicate, onParametersPredicate);
            if (method != null)
            {
                finalType = type;
                return method;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="method"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static bool TryFindMethod(this IEnumerable<Type> self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, [NotNullWhen(true)] out MethodInfo? method, ref Type? finalType)
    {
        method = FindMethod(self, methodNames, onReturnTypePredicate, onParametersPredicate, ref finalType);
        return method != null;
    }

    /// <summary>
    /// 查找实例方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <returns></returns>
    public static MethodInfo? FindInstanceMethod(this Type self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate)
    {
        var lastType = self;
        while (true)
        {
            MethodInfo[] methods = lastType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (methodNames.Contains(method.Name.Split('.').Last()))
                {
                    if (!onReturnTypePredicate(method.ReturnType))
                    {
                        continue;
                    }
                    var parameters = method.GetParameters();
                    if (!onParametersPredicate(parameters))
                    {
                        continue;
                    }
                    return method;
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }


    /// <summary>
    /// 查找实例方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static bool TryFindInstanceMethod(this Type self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, [NotNullWhen(true)] out MethodInfo? method)
    {
        method = FindInstanceMethod(self, methodNames, onReturnTypePredicate, onParametersPredicate);
        return method != null;
    }

    /// <summary>
    /// 查找实例方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static MethodInfo? FindInstanceMethod(this IEnumerable<Type> self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, ref Type? finalType)
    {
        foreach (var type in self)
        {
            var method = FindInstanceMethod(type, methodNames, onReturnTypePredicate, onParametersPredicate);
            if (method != null)
            {
                finalType = type;
                return method;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找实例方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodNames"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="method"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static bool TryFindInstanceMethod(this IEnumerable<Type> self, string[] methodNames, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, [NotNullWhen(true)] out MethodInfo? method, ref Type? finalType)
    {
        method = FindInstanceMethod(self, methodNames, onReturnTypePredicate, onParametersPredicate, ref finalType);
        return method != null;
    }

    /// <summary>
    /// 查找实例方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodName"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <returns></returns>
    public static MethodInfo[] FindInstanceMethods(this Type self, string methodName, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate)
    {
        List<MethodInfo> result = [];
        var lastType = self;
        while (true)
        {
            MethodInfo[] methods = lastType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.Name.Split('.').Last() == methodName)
                {
                    if (!onReturnTypePredicate(method.ReturnType))
                    {
                        continue;
                    }
                    var parameters = method.GetParameters();
                    if (!onParametersPredicate(parameters))
                    {
                        continue;
                    }
                    result.Add(method);
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return result.ToArray();
    }

    /// <summary>
    /// 查找实例方法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodName"></param>
    /// <param name="onReturnTypePredicate"></param>
    /// <param name="onParametersPredicate"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindInstanceMethods(this Type self, string methodName, Func<Type, bool> onReturnTypePredicate, Func<ParameterInfo[], bool> onParametersPredicate, [NotNullWhen(true)] out MethodInfo[] result)
    {
        result = FindInstanceMethods(self, methodName, onReturnTypePredicate, onParametersPredicate);
        return result.Length > 0;
    }

    /// <summary>
    /// 查找静态字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static FieldInfo? FindStaticField(this Type self, string fieldName)
    {
        var lastType = self;
        while (true)
        {
            foreach (var field in self.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (field.Name == fieldName)
                {
                    return field;
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }

    /// <summary>
    /// 查找静态字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindStaticField(this Type self, string fieldName, [NotNullWhen(true)] out FieldInfo? result)
    {
        result = FindStaticField(self, fieldName);
        return result != null;
    }

    /// <summary>
    /// 查找静态字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static FieldInfo? FindStaticField(this IEnumerable<Type> self, string fieldName, ref Type? finalType)
    {
        foreach (var type in self)
        {
            var field = FindStaticField(type, fieldName);
            if (field != null)
            {
                finalType = type;
                return field;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找静态字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <param name="result"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static bool TryFindStaticField(this IEnumerable<Type> self, string fieldName, [NotNullWhen(true)] out FieldInfo? result, ref Type? finalType)
    {
        result = FindStaticField(self, fieldName, ref finalType);
        return result != null;
    }

    /// <summary>
    /// 查找实例字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static FieldInfo? FindInstanceField(this Type self, string fieldName)
    {
        var lastType = self;
        while (true)
        {
            foreach (var field in self.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.Name == fieldName)
                {
                    return field;
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }

    /// <summary>
    /// 查找实例字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindInstanceField(this Type self, string fieldName, [NotNullWhen(true)] out FieldInfo? result)
    {
        result = FindInstanceField(self, fieldName);
        return result != null;
    }

    /// <summary>
    /// 查找实例字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static FieldInfo? FindInstanceField(this IEnumerable<Type> self, string fieldName, ref Type? finalType)
    {
        foreach (var type in self)
        {
            var field = FindInstanceField(type, fieldName);
            if (field != null)
            {
                finalType = type;
                return field;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找实例字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <param name="result"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static bool TryFindInstanceField(this IEnumerable<Type> self, string fieldName, out FieldInfo? result, ref Type? finalType)
    {
        result = FindInstanceField(self, fieldName, ref finalType);
        return result != null;
    }

    /// <summary>
    /// 查找字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static FieldInfo? FindField(this Type self, string fieldName)
    {
        var lastType = self;
        while (true)
        {
            foreach (var field in self.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (field.Name == fieldName)
                {
                    return field;
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }

    /// <summary>
    /// 查找字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindField(this Type self, string fieldName, out FieldInfo? result)
    {
        result = FindField(self, fieldName);
        return result != null;
    }

    /// <summary>
    /// 查找字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static FieldInfo? FindField(this IEnumerable<Type> self, string fieldName, ref Type? finalType)
    {
        foreach (var type in self)
        {
            var field = FindField(type, fieldName);
            if (field != null)
            {
                finalType = type;
                return field;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找字段
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fieldName"></param>
    /// <param name="result"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static bool TryFindField(this IEnumerable<Type> self, string fieldName, [NotNullWhen(true)] out FieldInfo? result, ref Type? finalType)
    {
        result = FindField(self, fieldName, ref finalType);
        return result != null;
    }

    /// <summary>
    /// 查找静态属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo? FindStaticProperty(this Type self, string propertyName)
    {
        var lastType = self;
        while (true)
        {
            foreach (var property in self.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (property.Name == propertyName)
                {
                    return property;
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }

    /// <summary>
    /// 查找静态属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindStaticProperty(this Type self, string propertyName, [NotNullWhen(true)] out PropertyInfo? result)
    {
        result = FindStaticProperty(self, propertyName);
        return result != null;
    }

    /// <summary>
    /// 查找静态属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static PropertyInfo? FindStaticProperty(this IEnumerable<Type> self, string propertyName, ref Type? finalType)
    {
        foreach (var type in self)
        {
            var property = FindStaticProperty(type, propertyName);
            if (property != null)
            {
                finalType = type;
                return property;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找静态属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <param name="result"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static bool TryFindStaticProperty(this IEnumerable<Type> self, string propertyName, [NotNullWhen(true)] out PropertyInfo? result, ref Type? finalType)
    {
        result = FindStaticProperty(self, propertyName, ref finalType);
        return result != null;
    }

    /// <summary>
    /// 查找实例属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo? FindInstanceProperty(this Type self, string propertyName)
    {
        var lastType = self;
        while (true)
        {
            foreach (var property in self.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (property.Name == propertyName)
                {
                    return property;
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }

    /// <summary>
    /// 查找实例属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindInstanceProperty(this Type self, string propertyName, [NotNullWhen(true)] out PropertyInfo? result)
    {
        result = FindInstanceProperty(self, propertyName);
        return result != null;
    }

    /// <summary>
    /// 查找实例属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static PropertyInfo? FindInstanceProperty(this IEnumerable<Type> self, string propertyName, ref Type? finalType)
    {
        foreach (var type in self)
        {
            var property = FindInstanceProperty(type, propertyName);
            if (property != null)
            {
                finalType = type;
                return property;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找实例属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <param name="result"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static bool TryFindInstanceProperty(this IEnumerable<Type> self, string propertyName, [NotNullWhen(true)] out PropertyInfo? result, ref Type? finalType)
    {
        result = FindInstanceProperty(self, propertyName, ref finalType);
        return result != null;
    }

    /// <summary>
    /// 查找属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo? FindProperty(this Type self, string propertyName)
    {
        var lastType = self;
        while (true)
        {
            foreach (var property in self.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (property.Name == propertyName)
                {
                    return property;
                }
            }
            lastType = lastType.BaseType;
            if (lastType == null) break;
        }
        return null;
    }

    /// <summary>
    /// 查找属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryFindProperty(this Type self, string propertyName, [NotNullWhen(true)] out PropertyInfo? result)
    {
        result = FindProperty(self, propertyName);
        return result != null;
    }

    /// <summary>
    /// 查找属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static PropertyInfo? FindProperty(this IEnumerable<Type> self, string propertyName, ref Type? finalType)
    {
        foreach (var type in self)
        {
            var property = FindProperty(type, propertyName);
            if (property != null)
            {
                finalType = type;
                return property;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找属性
    /// </summary>
    /// <param name="self"></param>
    /// <param name="propertyName"></param>
    /// <param name="result"></param>
    /// <param name="finalType"></param>
    /// <returns></returns>
    public static bool TryFindProperty(this IEnumerable<Type> self, string propertyName, [NotNullWhen(true)] out PropertyInfo? result, ref Type? finalType)
    {
        result = FindProperty(self, propertyName, ref finalType);
        return result != null;
    }

    /// <summary>
    /// Check the type is generic 
    /// </summary>
    /// <param name="self"></param>
    /// <param name="genericDefinitionType"></param>
    /// <returns></returns>
    public static bool IsGeneric(this Type self, Type genericDefinitionType)
    {
        return self.IsGenericType && self.GetGenericTypeDefinition() == genericDefinitionType;
    }

    /// <summary>
    /// class a{}
    /// class b:a{}
    /// <para>b.Is(a)</para>
    /// </summary>
    /// <param name="from">b</param>
    /// <param name="to">a</param>
    /// <returns></returns>
    public static bool Is(this Type from, Type to)
    {
        return to.IsAssignableFrom(from);
    }

    /// <summary>
    /// Check the types is same with the parameters
    /// </summary>
    /// <param name="froms"></param>
    /// <param name="tos"></param>
    /// <returns></returns>
    public static bool Is(this Type[] froms, ParameterInfo[] tos)
    {
        if (froms.Length != tos.Length)
        {
            return false;
        }
        for (int i = 0; i < froms.Length; i++)
        {
            if (!Is(froms[i], tos[i].ParameterType))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check the arguments is same with the parameters
    /// </summary>
    /// <param name="argumentTypes"></param>
    /// <param name="parameterInfos"></param>
    /// <returns></returns>
    public static bool IsAssignTo(this Type[] argumentTypes, ParameterInfo[] parameterInfos)
    {
        var scope = GetAssignScope(argumentTypes, parameterInfos);
        return scope != int.MaxValue;
    }

    /// <summary>
    /// 获取赋值分
    /// </summary>
    /// <param name="argumentTypes"></param>
    /// <param name="parameterInfos"></param>
    /// <returns></returns>
    public static int GetAssignScope(this Type[] argumentTypes, ParameterInfo[] parameterInfos)
    {
        int scope = 0;
        if (argumentTypes.Length < parameterInfos.Length)
        {
            for (int i = 0; i < argumentTypes.Length; i++)
            {
                var argumentType = argumentTypes[i];
                var parameterType = parameterInfos[i].ParameterType;
                if (argumentType.TryIncreaseAssignScope(parameterType, ref scope))
                {
                    continue;
                }
                else
                {
                    return int.MaxValue;
                }
            }
            var lastParameterInfo = parameterInfos[parameterInfos.Length - 1];
            if (lastParameterInfo.GetCustomAttribute(typeof(ParamArrayAttribute)) != null)
            {
                for (int i = argumentTypes.Length; i < parameterInfos.Length - 1; i++)
                {
                    if (parameterInfos[i].HasDefaultValue)
                    {
                        continue;
                    }
                    else
                    {
                        return int.MaxValue;
                    }
                }
            }
            else
            {
                for (int i = argumentTypes.Length; i < parameterInfos.Length; i++)
                {
                    if (parameterInfos[i].HasDefaultValue)
                    {
                        continue;
                    }
                    else
                    {
                        return int.MaxValue;
                    }
                }
            }
        }
        else
        {
            var frontParameterTypes = parameterInfos.Length >= 1 ? new Type[parameterInfos.Length - 1] : [];
            var lastParameterInfo = parameterInfos.Length >= 1 ? parameterInfos[parameterInfos.Length - 1] : null;
            for (int i = 0; i < frontParameterTypes.Length; i++)
            {
                frontParameterTypes[i] = parameterInfos[i].ParameterType;
            }
            for (int i = 0; i < frontParameterTypes.Length; i++)
            {
                var argumentType = argumentTypes[i];
                var parameterType = frontParameterTypes[i];
                if (argumentType.TryIncreaseAssignScope(parameterType, ref scope))
                {
                    continue;
                }
                else
                {
                    return int.MaxValue;
                }
            }
            if (lastParameterInfo != null)
            {
                var paramArrayLength = argumentTypes.Length - frontParameterTypes.Length;
                var lastParameterType = lastParameterInfo.ParameterType;
                if (lastParameterInfo.GetCustomAttribute(typeof(ParamArrayAttribute)) != null)
                {
                    //params特性
                    var elementType = lastParameterType.GetElementType()!;
                    for (int i = frontParameterTypes.Length; i < argumentTypes.Length; i++)
                    {
                        var argumentType = argumentTypes[i];
                        if (argumentType.TryIncreaseAssignScope(elementType, ref scope))
                        {
                            continue;
                        }
                        else
                        {
                            return int.MaxValue;
                        }
                    }
                }
                else
                {
                    if (argumentTypes.Length != parameterInfos.Length) return int.MaxValue;
                    var argumentType = argumentTypes[argumentTypes.Length - 1];
                    if (argumentType.TryIncreaseAssignScope(lastParameterType, ref scope))
                    {
                        return scope;
                    }
                    else
                    {
                        return int.MaxValue;
                    }
                }
            }
            else
            {
                return argumentTypes.Length == 0 ? scope : int.MaxValue;
            }
        }
        return scope;
    }

    /// <summary>
    /// Try Assign To the parameters with the arguments
    /// </summary>
    /// <param name="argumentTypes"></param>
    /// <param name="arguments"></param>
    /// <param name="parameterInfos"></param>
    /// <param name="parameterArguments"></param>
    /// <returns></returns>
    public static bool TryAssignTo(this Type?[] argumentTypes, object?[] arguments, ParameterInfo[] parameterInfos, out object?[] parameterArguments)
    {
        parameterArguments = new object?[parameterInfos.Length];
        if (argumentTypes.Length < parameterInfos.Length)
        {
            //存在默认值，也可能是空的params
            for (int i = 0; i < argumentTypes.Length; i++)
            {
                var argumentType = argumentTypes[i];
                var parameterType = parameterInfos[i].ParameterType;
                if (argumentType == null)
                {
                    continue;
                }
                if (argumentType.TryAssignTo(parameterType, out var implicitMethod))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            var lastParameterInfo = parameterInfos[parameterInfos.Length - 1];
            if (lastParameterInfo.GetCustomAttribute(typeof(ParamArrayAttribute)) != null)
            {
                for (int i = argumentTypes.Length; i < parameterInfos.Length - 1; i++)
                {
                    if (parameterInfos[i].HasDefaultValue)
                    {

                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = argumentTypes.Length; i < parameterInfos.Length; i++)
                {
                    if (parameterInfos[i].HasDefaultValue)
                    {
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        else
        {
            var frontParameterTypes = parameterInfos.Length >= 1 ? new Type[parameterInfos.Length - 1] : [];
            var lastParameterInfo = parameterInfos.Length >= 1 ? parameterInfos[parameterInfos.Length - 1] : null;
            for (int i = 0; i < frontParameterTypes.Length; i++)
            {
                frontParameterTypes[i] = parameterInfos[i].ParameterType;
            }
            for (int i = 0; i < frontParameterTypes.Length; i++)
            {
                var argumentType = argumentTypes[i];
                var parameterType = frontParameterTypes[i];
                if (argumentType == null)
                {
                    parameterArguments[i] = arguments[i];
                    continue;
                }
                if (argumentType.TryAssignTo(arguments[i], parameterType, TryGetImplicitMethodSettings.Default, out var parameterValue))
                {
                    parameterArguments[i] = parameterValue;
                    continue;
                }
                return false;
            }
            if (lastParameterInfo != null)
            {
                var paramArrayLength = argumentTypes.Length - frontParameterTypes.Length;
                var lastParameterType = lastParameterInfo.ParameterType;
                if (lastParameterInfo.GetCustomAttribute(typeof(ParamArrayAttribute)) != null)
                {
                    //params特性
                    var elementType = lastParameterType.GetElementType()!;
                    var paramsValue = Array.CreateInstance(elementType, paramArrayLength);
                    parameterArguments[parameterArguments.Length - 1] = paramsValue;
                    for (int i = frontParameterTypes.Length; i < argumentTypes.Length; i++)
                    {
                        var argumentType = argumentTypes[i];
                        if (argumentType == null)
                        {
                            paramsValue.SetValue(arguments[i], i - frontParameterTypes.Length);
                            continue;
                        }
                        if (argumentType.TryAssignTo(arguments[i], elementType, TryGetImplicitMethodSettings.Default, out var parameterValue))
                        {
                            paramsValue.SetValue(parameterValue, i - frontParameterTypes.Length);
                            continue;
                        }
                        return false;
                    }
                }
                else
                {
                    if (argumentTypes.Length != parameterInfos.Length) return false;
                    var argumentType = argumentTypes[argumentTypes.Length - 1];
                    if (argumentType == null)
                    {
                        parameterArguments[arguments.Length - 1] = arguments[arguments.Length - 1];
                        return true;
                    }
                    if (argumentType.TryAssignTo(arguments[arguments.Length - 1], lastParameterType, TryGetImplicitMethodSettings.Default, out var parameterValue))
                    {
                        parameterArguments[arguments.Length - 1] = parameterValue;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Try Assign To the parameters with the arguments
    /// </summary>
    /// <param name="argumentTypes"></param>
    /// <param name="parameterInfos"></param>
    /// <param name="implicitMethods"></param>
    /// <param name="implicitReturnTypes"></param>
    /// <param name="defaultParameters"></param>
    /// <param name="isParams"></param>
    /// <param name="paramsElementType"></param>
    /// <returns></returns>
    public static bool TryAssignTo(this Type[] argumentTypes, ParameterInfo[] parameterInfos,
        out MethodInfo?[] implicitMethods,
        out Type?[] implicitReturnTypes,
        out ParameterInfo[] defaultParameters,
        out bool isParams,
        out Type? paramsElementType)
    {
        isParams = false;
        paramsElementType = null;
        implicitMethods = new MethodInfo?[argumentTypes.Length];
        implicitReturnTypes = new Type?[argumentTypes.Length];
        defaultParameters = [];
        List<ParameterInfo> defaultParametersList = new();
        if (argumentTypes.Length < parameterInfos.Length)
        {
            //存在默认值，也可能是空的params
            for (int i = 0; i < argumentTypes.Length; i++)
            {
                var argumentType = argumentTypes[i];
                var parameterType = parameterInfos[i].ParameterType;
                if (argumentType.TryAssignTo(parameterType, out var implicitMethod))
                {
                    implicitMethods[i] = implicitMethod;
                    implicitReturnTypes[i] = parameterType;
                    continue;
                }
                else
                {
                    return false;
                }
            }
            var lastParameterInfo = parameterInfos[parameterInfos.Length - 1];
            if (lastParameterInfo.GetCustomAttribute(typeof(ParamArrayAttribute)) != null)
            {
                for (int i = argumentTypes.Length; i < parameterInfos.Length - 1; i++)
                {
                    if (parameterInfos[i].HasDefaultValue)
                    {
                        defaultParametersList.Add(parameterInfos[i]);
                    }
                    else
                    {
                        return false;
                    }
                }
                isParams = true;
                paramsElementType = lastParameterInfo.ParameterType.GetElementType() ?? throw new NullReferenceException($"{nameof(paramsElementType)}");
            }
            else
            {
                for (int i = argumentTypes.Length; i < parameterInfos.Length; i++)
                {
                    if (parameterInfos[i].HasDefaultValue)
                    {
                        defaultParametersList.Add(parameterInfos[i]);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        else
        {
            var frontParameterTypes = parameterInfos.Length >= 1 ? new Type[parameterInfos.Length - 1] : [];
            var lastParameterInfo = parameterInfos.Length >= 1 ? parameterInfos[parameterInfos.Length - 1] : null;
            for (int i = 0; i < frontParameterTypes.Length; i++)
            {
                frontParameterTypes[i] = parameterInfos[i].ParameterType;
            }
            for (int i = 0; i < frontParameterTypes.Length; i++)
            {
                var argumentType = argumentTypes[i];
                var parameterType = frontParameterTypes[i];
                if (argumentType.TryAssignTo(parameterType, out var implicitMethod))
                {
                    implicitMethods[i] = implicitMethod;
                    implicitReturnTypes[i] = parameterType;
                    continue;
                }
                else
                {
                    return false;
                }
            }
            if (lastParameterInfo != null)
            {
                var lastParameterType = lastParameterInfo.ParameterType;
                if (lastParameterInfo.GetCustomAttribute(typeof(ParamArrayAttribute)) != null)
                {
                    isParams = true;
                    var elementType = lastParameterType.GetElementType()!;
                    paramsElementType = elementType;
                    for (int i = frontParameterTypes.Length; i < argumentTypes.Length; i++)
                    {
                        var argumentType = argumentTypes[i];
                        if (argumentType.TryAssignTo(elementType, out var implicitMethod))
                        {
                            implicitMethods[i] = implicitMethod;
                            implicitReturnTypes[i] = elementType;
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (argumentTypes.Length != parameterInfos.Length) return false;
                    var argumentType = argumentTypes[argumentTypes.Length - 1];
                    if (argumentType.TryAssignTo(lastParameterType, out var implicitMethod))
                    {
                        implicitMethods[argumentTypes.Length - 1] = implicitMethod;
                        implicitReturnTypes[argumentTypes.Length - 1] = lastParameterType;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        defaultParameters = defaultParametersList.ToArray();
        return true;
    }

    /// <summary>
    /// Check the type is assignable to the to type
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static bool IsAssignToWithoutImplicit(this Type fromType, Type toType)
    {
        if (fromType.IsPrimitive && toType.IsPrimitive)
        {
            TypeCode typeCodeFrom = Type.GetTypeCode(fromType);
            TypeCode typeCodeTo = Type.GetTypeCode(toType);

            if (typeCodeFrom == typeCodeTo)
                return true;

            if (typeCodeFrom == TypeCode.Char)
                switch (typeCodeTo)
                {
                    case TypeCode.UInt16: return true;
                    case TypeCode.UInt32: return true;
                    case TypeCode.Int32: return true;
                    case TypeCode.UInt64: return true;
                    case TypeCode.Int64: return true;
                    case TypeCode.Single: return true;
                    case TypeCode.Double: return true;
                    default: return false;
                }
            // Possible conversions from Byte follow.

            if (typeCodeFrom == TypeCode.Byte)
                switch (typeCodeTo)
                {
                    case TypeCode.Char: return true;
                    case TypeCode.UInt16: return true;
                    case TypeCode.Int16: return true;
                    case TypeCode.UInt32: return true;
                    case TypeCode.Int32: return true;
                    case TypeCode.UInt64: return true;
                    case TypeCode.Int64: return true;
                    case TypeCode.Single: return true;
                    case TypeCode.Double: return true;
                    default: return false;
                }
            // Possible conversions from SByte follow.

            if (typeCodeFrom == TypeCode.SByte)
                switch (typeCodeTo)
                {
                    case TypeCode.Int16: return true;
                    case TypeCode.Int32: return true;
                    case TypeCode.Int64: return true;
                    case TypeCode.Single: return true;
                    case TypeCode.Double: return true;
                    default: return false;
                }
            // Possible conversions from UInt16 follow.

            if (typeCodeFrom == TypeCode.UInt16)
                switch (typeCodeTo)
                {
                    case TypeCode.UInt32: return true;
                    case TypeCode.Int32: return true;
                    case TypeCode.UInt64: return true;
                    case TypeCode.Int64: return true;
                    case TypeCode.Single: return true;
                    case TypeCode.Double: return true;
                    default: return false;
                }
            // Possible conversions from Int16 follow.

            if (typeCodeFrom == TypeCode.Int16)
                switch (typeCodeTo)
                {
                    case TypeCode.Int32: return true;
                    case TypeCode.Int64: return true;
                    case TypeCode.Single: return true;
                    case TypeCode.Double: return true;
                    default: return false;
                }
            // Possible conversions from UInt32 follow.

            if (typeCodeFrom == TypeCode.UInt32)
                switch (typeCodeTo)
                {
                    case TypeCode.UInt64: return true;
                    case TypeCode.Int64: return true;
                    case TypeCode.Single: return true;
                    case TypeCode.Double: return true;
                    default: return false;
                }
            // Possible conversions from Int32 follow.

            if (typeCodeFrom == TypeCode.Int32)
                switch (typeCodeTo)
                {
                    case TypeCode.Int64: return true;
                    case TypeCode.Single: return true;
                    case TypeCode.Double: return true;
                    default: return false;
                }
            // Possible conversions from UInt64 follow.

            if (typeCodeFrom == TypeCode.UInt64)
                switch (typeCodeTo)
                {
                    case TypeCode.Single: return true;
                    case TypeCode.Double: return true;
                    default: return false;
                }
            // Possible conversions from Int64 follow.

            if (typeCodeFrom == TypeCode.Int64)
                switch (typeCodeTo)
                {
                    case TypeCode.Single: return true;
                    case TypeCode.Double: return true;
                    default: return false;
                }
            // Possible conversions from Single follow.

            if (typeCodeFrom == TypeCode.Single)
                switch (typeCodeTo)
                {
                    case TypeCode.Double: return true;
                    default: return false;
                }
        }
        else if (Is(fromType, toType))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check the from type is assignable to the to type
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static bool IsAssignTo(this Type fromType, Type toType)
    {
        if (IsAssignToWithoutImplicit(fromType, toType))
        {
            return true;
        }
        else if (TryGetImplicitMethod(fromType, toType, TryGetImplicitMethodSettings.Default, out _))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取赋值分，0表示可以直接赋值，1表示可以通过隐式转换赋值，int.MaxValue表示不能赋值
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static int GetAssignScope(this Type fromType, Type toType)
    {
        if (fromType == toType)
        {
            return 0;
        }
        else if (IsAssignToWithoutImplicit(fromType, toType))
        {
            if (toType == typeof(object))
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
        else if (fromType == typeof(object))
        {
            return 4;
        }
        else if (TryGetImplicitMethod(fromType, toType, TryGetImplicitMethodSettings.Default, out _))
        {
            return 2;
        }
        return int.MaxValue;
    }

    /// <summary>
    /// 尝试增加赋值分
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    public static bool TryIncreaseAssignScope(this Type fromType, Type toType, ref int scope)
    {
        var itemScope = GetAssignScope(fromType, toType);
        if (itemScope == int.MaxValue)
        {
            return false;
        }
        scope += itemScope;
        return true;
    }

    /// <summary>
    /// Try Assign To the from value to the to type
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="fromValue"></param>
    /// <param name="toType"></param>
    /// <param name="settings"></param>
    /// <param name="toValue"></param>
    /// <returns></returns>
    public static bool TryAssignTo(this Type fromType, object? fromValue, Type toType, TryGetImplicitMethodSettings settings, out object? toValue)
    {
        if (fromType.IsPrimitive && toType.IsPrimitive)
        {
            TypeCode typeCodeFrom = Type.GetTypeCode(fromType);
            TypeCode typeCodeTo = Type.GetTypeCode(toType);

            if (typeCodeFrom == typeCodeTo)
            {
                toValue = fromValue;
                return true;
            }

            if (typeCodeFrom == TypeCode.Char)
                switch (typeCodeTo)
                {
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.Int32:
                    case TypeCode.UInt64:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        {
                            toValue = Convert.ChangeType(fromValue, toType);
                            return true;
                        }
                    default:
                        {
                            toValue = null;
                            return false;
                        }
                }
            // Possible conversions from Byte follow.

            if (typeCodeFrom == TypeCode.Byte)
                switch (typeCodeTo)
                {
                    case TypeCode.Char:
                    case TypeCode.UInt16:
                    case TypeCode.Int16:
                    case TypeCode.UInt32:
                    case TypeCode.Int32:
                    case TypeCode.UInt64:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        {
                            toValue = Convert.ChangeType(fromValue, toType);
                            return true;
                        }
                    default:
                        {
                            toValue = null;
                            return false;
                        }
                }
            // Possible conversions from SByte follow.

            if (typeCodeFrom == TypeCode.SByte)
                switch (typeCodeTo)
                {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        {
                            toValue = Convert.ChangeType(fromValue, toType);
                            return true;
                        }
                    default:
                        {
                            toValue = null;
                            return false;
                        }
                }
            // Possible conversions from UInt16 follow.

            if (typeCodeFrom == TypeCode.UInt16)
                switch (typeCodeTo)
                {
                    case TypeCode.UInt32:
                    case TypeCode.Int32:
                    case TypeCode.UInt64:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        {
                            toValue = Convert.ChangeType(fromValue, toType);
                            return true;
                        }
                    default:
                        {
                            toValue = null;
                            return false;
                        }
                }
            // Possible conversions from Int16 follow.

            if (typeCodeFrom == TypeCode.Int16)
                switch (typeCodeTo)
                {
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        {
                            toValue = Convert.ChangeType(fromValue, toType);
                            return true;
                        }
                    default:
                        {
                            toValue = null;
                            return false;
                        }
                }
            // Possible conversions from UInt32 follow.

            if (typeCodeFrom == TypeCode.UInt32)
                switch (typeCodeTo)
                {
                    case TypeCode.UInt64:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        {
                            toValue = Convert.ChangeType(fromValue, toType);
                            return true;
                        }
                    default:
                        {
                            toValue = null;
                            return false;
                        }
                }
            // Possible conversions from Int32 follow.

            if (typeCodeFrom == TypeCode.Int32)
                switch (typeCodeTo)
                {
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        {
                            toValue = Convert.ChangeType(fromValue, toType);
                            return true;
                        }
                    default:
                        {
                            toValue = null;
                            return false;
                        }
                }
            // Possible conversions from UInt64 follow.

            if (typeCodeFrom == TypeCode.UInt64)
                switch (typeCodeTo)
                {
                    case TypeCode.Single:
                    case TypeCode.Double:
                        {
                            toValue = Convert.ChangeType(fromValue, toType);
                            return true;
                        }
                    default:
                        {
                            toValue = null;
                            return false;
                        }
                }
            // Possible conversions from Int64 follow.

            if (typeCodeFrom == TypeCode.Int64)
                switch (typeCodeTo)
                {
                    case TypeCode.Single:
                    case TypeCode.Double:
                        {
                            toValue = Convert.ChangeType(fromValue, toType);
                            return true;
                        }
                    default:
                        {
                            toValue = null;
                            return false;
                        }
                }
            // Possible conversions from Single follow.

            if (typeCodeFrom == TypeCode.Single)
                switch (typeCodeTo)
                {
                    case TypeCode.Double:
                        {
                            toValue = Convert.ChangeType(fromValue, toType);
                            return true;
                        }
                    default:
                        {
                            toValue = null;
                            return false;
                        }
                }
        }
        else if (Is(fromType, toType))
        {
            toValue = fromValue;
            return true;
        }
        else if (fromType.TryGetImplicitMethod(toType, settings, out var implicitMethod))
        {
            if (implicitMethod.GetParameters().Length == 1)
            {
                toValue = implicitMethod!.Invoke(null, [fromValue]);
            }
            else
            {
                toValue = implicitMethod!.Invoke(null, [fromValue, toType]);
            }
            return true;
        }
        toValue = null;
        return false;
    }

    /// <summary>
    /// Try Assign To the from value to the to type
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <param name="implicitMethod"></param>
    /// <returns></returns>
    public static bool TryAssignTo(this Type fromType, Type toType, out MethodInfo? implicitMethod)
    {
        implicitMethod = null;
        if (IsAssignToWithoutImplicit(fromType, toType))
        {
            return true;
        }
        else if (fromType.TryGetImplicitMethod(toType, TryGetImplicitMethodSettings.Default, out implicitMethod))
        {
            return true;
        }
        else if (fromType == typeof(object))
        {
            return true;
        }
        return false;
    }

    private static Regex op_ImplicitTo_Regex = new("op_ImplicitTo_(.*)");

    /// <summary>
    /// 尝试获取隐式转换方法的设置
    /// </summary>
    public struct TryGetImplicitMethodSettings
    {
        /// <summary>
        /// 是否启用 op_ImplicitTo_value_toType
        /// </summary>
        public bool Enable_op_ImplicitTo_value_toType;

        /// <summary>
        /// 默认设置
        /// </summary>
        public static TryGetImplicitMethodSettings Default { get; } = new()
        {
            Enable_op_ImplicitTo_value_toType = true,
        };
    }

    /// <summary>
    /// 获取隐式转换方法
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <param name="implicitMethod"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static bool TryGetImplicitMethod(this Type fromType, Type toType, TryGetImplicitMethodSettings settings, [NotNullWhen(true)] out MethodInfo? implicitMethod)
    {
        if (fromType.BaseType == typeof(MulticastDelegate) && toType.BaseType == typeof(MulticastDelegate))
        {
            implicitMethod = null;
            return false;
        }
        else if (fromType.GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public, null, [fromType], null) is MethodInfo method1 && method1.ReturnType == toType)
        {
            implicitMethod = method1;
            return true;
        }
        else if (toType.GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public, null, [fromType], null) is MethodInfo method2)
        {
            implicitMethod = method2;
            return true;
        }
        else if (toType.GetMethod("op_ImplicitFrom", BindingFlags.Static | BindingFlags.Public, null, [fromType], null) is MethodInfo method3)
        {
            implicitMethod = method3;
            return true;
        }
        else if (settings.Enable_op_ImplicitTo_value_toType && fromType.GetMethod("op_ImplicitTo", BindingFlags.Static | BindingFlags.Public, null, [fromType, typeof(Type)], null) is MethodInfo method5)
        {
            implicitMethod = method5;
            return true;
        }
        else if (fromType.GetMethod("op_ImplicitTo", BindingFlags.Static | BindingFlags.Public, null, [fromType], null) is MethodInfo method4)
        {
            implicitMethod = method4;
            return true;
        }
        else if (fromType.TryFindStaticMethod(op_ImplicitTo_Regex, toType, [fromType], out var method6))
        {
            implicitMethod = method6;
            return true;
        }
        else if (settings.Enable_op_ImplicitTo_value_toType && fromType.TryFindStaticMethod("op_ImplicitTo", typeof(object), [fromType, typeof(Type)], out var method7))
        {
            implicitMethod = method7;
            return true;
        }
        implicitMethod = null;
        return false;
    }

    /// <summary>
    /// Create a array
    /// </summary>
    /// <param name="values"></param>
    /// <param name="arrayType"></param>
    /// <returns></returns>
    public static object GenerateArray(this List<object> values, Type arrayType)
    {
        var constructorMethod = arrayType.GetConstructor([typeof(int)])!;
        var result = constructorMethod.Invoke([values.Count]);
        var setValue = arrayType.FindStaticMethod("SetValue", null, [typeof(object), typeof(int)])!;
        for (int i = 0; i < values.Count; i++)
        {
            setValue.Invoke(result, new object[] { values[i], i });
        }
        return result;
    }

    /// <summary>
    /// Create a empty array
    /// </summary>
    /// <param name="arrayType"></param>
    /// <returns></returns>
    public static object GenerateEmptyArray(this Type arrayType)
    {
        var constructorMethod = arrayType.GetConstructor(new Type[] { typeof(int) })!;
        var result = constructorMethod.Invoke(new object[] { 0 });
        return result;
    }

    /// <summary>
    /// Get the parameters length of the invoke method
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static int GetInvokeParametersLength(this Type self)
    {
        var invokeMethod = self.GetMethod("Invoke");
        if (invokeMethod == null) throw new Exception("Invoke Method not found");
        var parameters = invokeMethod.GetParameters();
        return parameters.Length;
    }

    /// <summary>
    /// Get the parameters names of the invoke method
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static string?[] GetInvokeParameterNames(this Type self)
    {
        var invokeMethod = self.GetMethod("Invoke");
        if (invokeMethod == null) throw new Exception("Invoke Method not found");
        var parameters = invokeMethod.GetParameters();
        return parameters.Select(x => x.Name).ToArray();
    }


}

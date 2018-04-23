using System;
using System.Reflection;

namespace JQCore.Utils
{
    /// <summary>
    /// 类名：MethoUtil.cs
    /// 类功能描述：
    /// 创建标识：yjq 2018/4/23 15:02:23
    /// </summary>
    public static class MethoUtil
    {
        public static T GetMethodAttribute<T>(this MethodInfo methodInfo) where T : Attribute
        {
            return methodInfo.GetCustomAttribute<T>();
        }
    }
}
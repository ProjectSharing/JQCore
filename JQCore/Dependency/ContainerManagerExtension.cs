using Autofac;
using JQCore.Cache;
using JQCore.Lock;
using JQCore.Serialization;

namespace JQCore.Dependency
{
    /// <summary>
    /// Copyright (C) 2018 备胎 版权所有。
    /// 类名：ContainerManagerExtension.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2018/2/6 14:30:33
    /// </summary>
    public static class ContainerManagerExtension
    {
        public static ContainerBuilder UseDefaultConfig(this ContainerBuilder builder)
        {
            builder.AddScoped<ICache, LocalCache>()
                    .UseLocalLock()
                    .UseJsonNet()
                    .UseDefaultBinarySerailizer();
            return builder;
        }
    }
}
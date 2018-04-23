using Castle.DynamicProxy;
using JQCore.Cache;
using JQCore.Extensions;
using JQCore.Serialization;
using JQCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace JQCore.Dependency.AutofacContainer.Intercept
{
    /// <summary>
    /// 类名：CacheIntercept.cs
    /// 类功能描述：
    /// 创建标识：yjq 2018/4/23 14:23:41
    /// </summary>
    public class CacheIntercept : BaseIntercept, IInterceptor
    {
        /// <summary>
        /// 异步方法处理
        /// </summary>
        private static readonly MethodInfo _HandleAddAsyncMethodInfo = typeof(CacheIntercept).GetMethod("ExcuteAddAsyncFunction", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo _HandleRemoveAsyncMethodInfo = typeof(CacheIntercept).GetMethod("ExcuteRemoveAsyncFunction", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly ICache _cache;
        private readonly IJsonSerializer _jsonSerializer;

        public CacheIntercept(ICache cache, IJsonSerializer jsonSerializer)
        {
            _cache = cache;
            _jsonSerializer = jsonSerializer;
        }

        public void Intercept(IInvocation invocation)
        {
            if (_cache == null)
            {
                invocation.Proceed();
                return;
            }
            var cacheAttribute = GetCacheAttribute(invocation);
            if (cacheAttribute == null)
            {
                invocation.Proceed();
                return;
            }
            var delegateType = GetDelegateType(invocation);
            if (cacheAttribute.CacheAction == CacheAction.Remove)
            {
                switch (delegateType)
                {
                    case MethodType.AsyncAction:
                        invocation.ReturnValue = ExcuteRemoveAsync(invocation, cacheAttribute);
                        break;

                    case MethodType.Synchronous:
                    case MethodType.SynchronousVoid:
                        ExcuteRemove(invocation, cacheAttribute);
                        break;

                    case MethodType.AsyncFunction:
                        ExecuteHandleAsyncWithResultUsingReflection(invocation, cacheAttribute);
                        break;
                }
            }
            else
            {
                if (delegateType == MethodType.AsyncAction || MethodType.SynchronousVoid == delegateType)
                {
                    invocation.Proceed();
                    return;
                }
                if (_cache.SimpleIsExistCache(cacheAttribute.CacheName))
                {
                    var cacheValue = _cache.SimpleGetCache<string>(cacheAttribute.CacheName);
                    if (delegateType == MethodType.Synchronous)
                    {
                        invocation.ReturnValue = _jsonSerializer.Deserialize(cacheValue, invocation.Method.ReturnType);
                    }
                    else
                    {
                        invocation.ReturnValue = _jsonSerializer.DeserializeAsync(cacheValue, invocation.Method.ReturnType.GetGenericArguments()[0]);
                    }
                }
                else
                {
                    if (delegateType == MethodType.Synchronous)
                    {
                        invocation.Proceed();
                        AddCache(invocation.ReturnValue, cacheAttribute);
                    }
                    else
                    {
                        ExecuteHandleAsyncWithResultUsingReflection(invocation, cacheAttribute);
                    }
                }
            }
        }

        /// <summary>
        /// 执行异步方法
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="cacheAttribute">缓存标记</param>
        /// <param name="functionType">1 新增或修改 2 移除</param>
        private void ExecuteHandleAsyncWithResultUsingReflection(IInvocation invocation, CacheAttribute cacheAttribute)
        {
            invocation.Proceed();
            var resultType = invocation.Method.ReturnType.GetGenericArguments()[0];
            if (cacheAttribute.CacheAction == CacheAction.Remove)
            {
                var method = _HandleRemoveAsyncMethodInfo.MakeGenericMethod(resultType);
                invocation.ReturnValue = method.Invoke(this, new[] { invocation.ReturnValue, cacheAttribute });
            }
            else if (cacheAttribute.CacheAction == CacheAction.AddOrUpdate)
            {
                var mi = _HandleAddAsyncMethodInfo.MakeGenericMethod(resultType);
                invocation.ReturnValue = mi.Invoke(this, new[] { invocation.ReturnValue, cacheAttribute });
            }
        }

        public void ExcuteRemove(IInvocation invocation, CacheAttribute cacheAttribute)
        {
            try
            {
                invocation.Proceed();
                _cache.SimpleRemoveCache(cacheAttribute.CacheName);
            }
            catch
            {
                _cache.SimpleRemoveCache(cacheAttribute.CacheName);
                throw;
            }
        }

        private async Task ExcuteRemoveAsync(IInvocation invocation, CacheAttribute cacheAttribute)
        {
            try
            {
                invocation.Proceed();
                await (Task)invocation.ReturnValue;
                _cache.SimpleRemoveCache(cacheAttribute.CacheName);
            }
            catch
            {
                _cache.SimpleRemoveCache(cacheAttribute.CacheName);
                throw;
            }
        }

        private async Task<T> ExcuteRemoveAsyncFunction<T>(Task<T> task, CacheAttribute cacheAttribute)
        {
            try
            {
                var result = await task;
                _cache.SimpleRemoveCache(cacheAttribute.CacheName);
                return result;
            }
            catch
            {
                _cache.SimpleRemoveCache(cacheAttribute.CacheName);
                throw;
            }
        }

        private async Task<T> ExcuteAddAsyncFunction<T>(Task<T> task, CacheAttribute cacheAttribute)
        {
            var result = await task;
            AddCache(result, cacheAttribute);
            return result;
        }

        private void AddCache(object value, CacheAttribute cacheAttribute)
        {
            var cacheValue = _jsonSerializer.Serialize(value);
            if (cacheAttribute.CacheType == 0)
            {
                _cache.SimpleAddSlidingCache(cacheAttribute.CacheName, cacheValue, TimeSpan.FromMilliseconds(cacheAttribute.Millisecond));
            }
            else
            {
                _cache.SimpleAddAbsoluteCache(cacheAttribute.CacheName, cacheValue, DateTime.Now.AddMilliseconds(cacheAttribute.Millisecond));
            }
        }

        private static readonly ConcurrentDictionary<RuntimeMethodHandle, CacheAttribute> _CacheAttributeDic = new ConcurrentDictionary<RuntimeMethodHandle, CacheAttribute>();

        private CacheAttribute GetCacheAttribute(IInvocation invocation)
        {
            return _CacheAttributeDic.GetValue(invocation.Method.MethodHandle, () =>
            {
                return invocation.Method.GetMethodAttribute<CacheAttribute>();
            });
        }
    }

    /// <summary>
    /// 使用缓存标记
    /// </summary>
    public class CacheAttribute : Attribute
    {
        public CacheAttribute(string cacheName, CacheAction cacheAction)
        {
            CacheName = cacheName;
            CacheAction = cacheAction;
        }

        /// <summary>
        /// 缓存名字
        /// </summary>
        public string CacheName { get; set; }

        /// <summary>
        /// 移除或者新增更新
        /// </summary>
        public CacheAction CacheAction { get; set; }

        /// <summary>
        /// 0相对缓存 1 绝对缓存
        /// </summary>
        public int CacheType { get; set; } = 0;

        /// <summary>
        /// 缓存时间 毫秒单位
        /// </summary>
        public int Millisecond { get; set; } = 15 * 1000 * 60;
    }

    public enum CacheAction
    {
        /// <summary>
        /// 添加或者更新
        /// </summary>
        AddOrUpdate = 1,

        /// <summary>
        /// 移除
        /// </summary>
        Remove = 2
    }
}
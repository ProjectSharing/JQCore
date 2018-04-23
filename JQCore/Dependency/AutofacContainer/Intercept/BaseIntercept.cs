using Castle.DynamicProxy;
using System;
using System.Threading.Tasks;

namespace JQCore.Dependency.AutofacContainer.Intercept
{
    /// <summary>
    /// 类名：BaseIntercept.cs
    /// 类功能描述：
    /// 创建标识：yjq 2018/4/23 14:22:25
    /// </summary>
    public abstract class BaseIntercept
    {
        /// <summary>
        /// 异步类型(无返回值)
        /// </summary>
        protected static readonly Type _AsyncActionType = typeof(Task);

        /// <summary>
        /// 同步方法无返回值
        /// </summary>
        protected static readonly Type _VoidActionType = typeof(void);

        /// <summary>
        /// 异步方法类型(有返回值)
        /// </summary>
        protected static readonly Type _AsyncFunctionType = typeof(Task<>);

        /// <summary>
        /// 获取方法类型
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        protected MethodType GetDelegateType(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            if (returnType == _AsyncActionType)
                return MethodType.AsyncAction;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == _AsyncFunctionType)
                return MethodType.AsyncFunction;
            if (returnType == _VoidActionType)
                return MethodType.SynchronousVoid;
            return MethodType.Synchronous;
        }

        /// <summary>
        /// 方法类型
        /// </summary>
        protected enum MethodType
        {
            /// <summary>
            /// 同步方法（有返回值）
            /// </summary>
            Synchronous,

            /// <summary>
            /// 同步无返回值
            /// </summary>
            SynchronousVoid,

            /// <summary>
            /// 异步(无返回值)
            /// </summary>
            AsyncAction,

            /// <summary>
            /// 异步方法(有返回值)
            /// </summary>
            AsyncFunction
        }
    }
}
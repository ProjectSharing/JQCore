using Autofac;
using Autofac.Extensions.DependencyInjection;
using JQCore.Dependency;
using JQCore.Dependency.AutofacContainer.Intercept;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JQCore.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var setttins = new ConfigurationBuilder().Build();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMemoryCache();

            var builder = new ContainerBuilder();
            builder.UseDefaultConfig();
            builder.Populate(serviceCollection);
            var container = ContainerManager.UseAutofacContainer(builder);
            ContainerManager.Instance.Container.RegisterType(typeof(CacheIntercept), lifeStyle: LifeStyle.PerLifetimeScope);
            ContainerManager.Instance.Container.RegisterType<IFind, Find>(new Type[] { typeof(CacheIntercept) }, lifeStyle: LifeStyle.PerLifetimeScope);

            var find = ContainerManager.Resolve<IFind>();
            Console.WriteLine(find.GetValue());
            Console.ReadLine();
        }
    }

    public interface IFind
    {
        [Cache("GetValue", CacheAction.AddOrUpdate)]
        int GetValue();
        [Cache("GetValue", CacheAction.Remove)]
        string SetValue(string value);
    }

    public class Find : IFind
    {
    
        public int GetValue()
        {
            return 1;
        }

   
        public string SetValue(string value)
        {
            return value;
        }
    }
}
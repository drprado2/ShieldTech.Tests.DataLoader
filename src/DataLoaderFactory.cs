using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;

namespace ShieldTech.Tests.DataLoader
{
    public class DataLoaderFactory
    {
        private readonly Dictionary<Type, Object> _loaders = new Dictionary<Type, Object>();
        public TestServer Server { get; private set; }

        public DataLoaderFactory(TestServer server)
        {
            Server = server;
        }
        
        public async Task<TLoader> GetLoaderAsync<TLoader>() where TLoader : DataLoader
        {
            var type = typeof(TLoader);
            return (TLoader)(await GetLoaderAsync(type));
        }
        
        public async Task<DataLoader> GetLoaderAsync(Type loaderType)
        {
            if (_loaders.TryGetValue(loaderType, out var loader))
                return (DataLoader)loader;

            var resultLoader = (DataLoader)Activator.CreateInstance(loaderType);
            var dependentLoaders = loaderType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDependentDataLoader<>))
                .Select(x => x.GetGenericArguments().First());
            foreach (var dependentLoaderType in dependentLoaders)
            {
                var resolvedLoader = await GetLoaderAsync(dependentLoaderType);
                resultLoader.AddDependentLoader(resolvedLoader);
            }
            
            await resultLoader.LoadAsync(Server);
            await resultLoader.RefreshAllDependentsStateAsync(Server);
            _loaders.Add(loaderType, resultLoader);
            return resultLoader;
        }
    }
}
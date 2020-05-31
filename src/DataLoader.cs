using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;

namespace ShieldTech.Tests.DataLoader
{
    /// <summary>
    /// Class used to manager data for tests
    /// </summary>
    public abstract class DataLoader
    {
        private readonly Dictionary<Type, DataLoader> _dependentLoader = new Dictionary<Type, DataLoader>();
        
        /// <summary>
        /// Should implement the fill of data, create your own properties and fill those
        /// </summary>
        public abstract Task LoadAsync(TestServer server);

        /// <summary>
        /// Implement here the logic to refresh the state of entities from your loader
        /// </summary>
        public abstract Task RefreshStateAsync(TestServer server);

        /// <summary>
        /// Register another loader that the class need for fill your data
        /// </summary>
        public void AddDependentLoader<TLoader>(TLoader loader) where TLoader : DataLoader
        {
            _dependentLoader.Add(loader.GetType(), loader);
        }

        /// <summary>
        /// Call RefreshState in all dependent loaders
        /// </summary>
        public async Task RefreshAllDependentsStateAsync(TestServer server)
        {
            var tasks = _dependentLoader.Values.Select(x => x.RefreshStateAsync(server)).ToList();
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Get an pre registered loader, the loaders are automatically registered if you assign IDependentDataLoader<> interface and use the DataLoaderFactory for create your loaders
        /// </summary>
        /// <exception cref="LoaderNotFoundException">Exception throwed case you try to get an instance not registered</exception>
        public TLoader GetLoader<TLoader>() where TLoader : DataLoader
        {
            var type = typeof(TLoader);
            if (_dependentLoader.TryGetValue(type, out var loader))
                return (TLoader)loader;
            
            throw new LoaderNotFoundException(type.Name);
        }
    }
}
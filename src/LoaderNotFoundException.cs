using System;

namespace ShieldTech.Tests.DataLoader
{
    public class LoaderNotFoundException : Exception
    {
        public LoaderNotFoundException(string loaderName) : base($"The {loaderName} has not been registered. You must subscribe to the IDependentDataLoader<{loaderName}> interface")
        {
        }
    }
}
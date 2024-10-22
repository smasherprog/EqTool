using Autofac;
using System.Windows;

namespace EQTool.Services
{
    public class WindowFactory
    {
        //anti pattern below, but its the only way to do this.
        private readonly ILifetimeScope container;
        public WindowFactory(ILifetimeScope container)
        {
            this.container = container;
        }

        public T CreateWindow<T>() where T : Window
        {
            return container.Resolve<T>();
        }
    }
}

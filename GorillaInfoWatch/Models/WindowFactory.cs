using GorillaInfoWatch.Interfaces;
using System;
using Zenject;

namespace GorillaInfoWatch.Models
{
    public class WindowFactory : IFactory<Type, IWindow>
    {
        private readonly DiContainer Container;

        public WindowFactory(DiContainer container) => Container = container;

        public IWindow Create(Type viewType) => (IWindow)Container.Instantiate(viewType);
    }
}

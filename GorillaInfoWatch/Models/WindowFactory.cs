using GorillaInfoWatch.Interfaces;
using System;
using Zenject;

namespace GorillaInfoWatch.Models
{
    public class WindowFactory : IFactory<Type, IPage>
    {
        private readonly DiContainer Container;

        public WindowFactory(DiContainer container) => Container = container;

        public IPage Create(Type viewType) => (IPage)Container.Instantiate(viewType);
    }
}

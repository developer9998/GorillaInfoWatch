using GorillaInfoWatch.Interfaces;
using System;
using Zenject;

namespace GorillaInfoWatch.Models
{
    public class TabFactory : IFactory<Type, ITab>
    {
        private readonly DiContainer _container;

        public TabFactory(DiContainer container) => _container = container;

        public ITab Create(Type viewType) => (ITab)_container.Instantiate(viewType);
    }
}

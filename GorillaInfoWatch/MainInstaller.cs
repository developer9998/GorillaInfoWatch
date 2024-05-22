using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Pages;
using GorillaInfoWatch.QuickActions;
using GorillaInfoWatch.Tools;
using GorillaLocomotion;
using System;
using UnityEngine;
using Zenject;

namespace GorillaInfoWatch
{
    public class MainInstaller : Installer
    {
        public GameObject Player(InjectContext ctx) => UnityEngine.Object.FindObjectOfType<Player>().gameObject;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Main>().FromNewComponentOn(Player).AsSingle();

            Container.BindInterfacesAndSelfTo<AssetLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<Configuration>().AsSingle();
            Container.BindInterfacesAndSelfTo<DataManager>().AsSingle();

            Container
                .BindFactory<Type, IPage, WindowPlaceholderFactory>()
                .FromFactory<WindowFactory>();

            Container.Bind<HomePage>().AsSingle();
            Container.Bind<ModRoomWarningPage>().AsSingle();

            // Quick Actions (a simple action which can be ran at a click of a button)
            Container.Bind<IQuickAction>().To<Disconnect>().AsSingle();
            Container.Bind<IQuickAction>().To<Rejoin>().AsSingle();
            Container.Bind<IQuickAction>().To<Quit>().AsSingle();
            Container.Bind<IQuickAction>().To<SetVoice>().AsSingle(); // :3 (lunakitty)
            Container.Bind<IQuickAction>().To<SetParticles>().AsSingle();
        }
    }
}
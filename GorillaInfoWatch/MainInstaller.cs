using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.QuickActions;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Windows;
using GorillaInfoWatch.Windows.Scoreboard;
using GorillaLocomotion;
using System;
using Zenject;

namespace GorillaInfoWatch
{
    public class MainInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Main>().FromNewComponentOn((InjectContext ctx) => UnityEngine.Object.FindObjectOfType<Player>().gameObject).AsSingle();
            Container.BindInterfacesAndSelfTo<AssetLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<Configuration>().AsSingle();

            Container
                .BindFactory<Type, IWindow, WindowPlaceholderFactory>()
                .FromFactory<WindowFactory>();

            Container.Bind<HomeWindow>().AsSingle();

            Container.Bind<IEntry>().To<ScoreboardEntry>().AsSingle();
            Container.Bind<IEntry>().To<DetailsEntry>().AsSingle();
            Container.Bind<IEntry>().To<ModStatusEntry>().AsSingle();
            Container.Bind<IEntry>().To<QuickActionsEntry>().AsSingle();
            Container.Bind<IEntry>().To<SettingsEntry>().AsSingle();

            Container.Bind<IQuickAction>().To<Disconnect>().AsSingle();
            Container.Bind<IQuickAction>().To<Quit>().AsSingle();
            Container.Bind<IQuickAction>().To<SetVoice>().AsSingle(); // :3 (lunakitty)
            Container.Bind<IQuickAction>().To<SetParticles>().AsSingle();
        }
    }
}
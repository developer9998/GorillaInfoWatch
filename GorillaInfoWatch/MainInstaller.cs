using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.PlayerFunctions;
using GorillaInfoWatch.QuickActions;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Windows;
using GorillaInfoWatch.Windows.Scoreboard;
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
            Container.BindInterfacesAndSelfTo<PlayerExecution>().FromNewComponentOn(Player).AsSingle();

            Container.BindInterfacesAndSelfTo<AssetLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<Configuration>().AsSingle();

            Container
                .BindFactory<Type, IWindow, WindowPlaceholderFactory>()
                .FromFactory<WindowFactory>();

            Container.Bind<HomeWindow>().AsSingle();

            // Entries (an interface with a display name and redirect window in the form of a type)
            Container.Bind<IEntry>().To<ScoreboardEntry>().AsSingle();
            Container.Bind<IEntry>().To<DetailsEntry>().AsSingle();
            Container.Bind<IEntry>().To<ModStatusEntry>().AsSingle();
            Container.Bind<IEntry>().To<QuickActionsEntry>().AsSingle();
            Container.Bind<IEntry>().To<SettingsEntry>().AsSingle();

            // Quick Actions (a simple action which can be ran at a click of a button)
            Container.Bind<IQuickAction>().To<Disconnect>().AsSingle();
            Container.Bind<IQuickAction>().To<Quit>().AsSingle();
            Container.Bind<IQuickAction>().To<SetVoice>().AsSingle(); // :3 (lunakitty)
            Container.Bind<IQuickAction>().To<SetParticles>().AsSingle();

            // Player Functions (a set of actions ran based on when a player joins/leaves with set parameters)
            Container.Bind<IPlayerFunction>().To<Volume>().AsSingle();
            Container.Bind<IPlayerFunction>().To<Favourites>().AsSingle();
        }
    }
}
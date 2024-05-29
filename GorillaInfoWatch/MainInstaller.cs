using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Pages;
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
            Container.BindInterfacesAndSelfTo<Metadata>().AsSingle();
            Container.BindInterfacesAndSelfTo<Logging>().AsSingle();

            Container.BindFactory<Type, IPage, WindowPlaceholderFactory>().FromFactory<WindowFactory>();

            Container.Bind<HomePage>().AsSingle();
            Container.Bind<ModRoomWarningPage>().AsSingle();
        }
    }
}
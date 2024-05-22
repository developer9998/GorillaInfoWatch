using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Pages;
using System;
using System.Linq;

namespace GorillaInfoWatch.Tools
{
    public class PageManager
    {
        public ModRoomWarningPage ModWarnPage;

        public event Action<HeaderLine> OnHeaderSet;
        public event Action<object[]> OnLinesSet, OnLinesUpdated;
        public event Action<Type, Type, object[]> OnPageSwitched;

        public IPage CurrentPage;

        public void SwitchPage(IPage page, object[] parameters)
        {
            Type pageType = page.GetType();

            if (pageType != typeof(ModRoomWarningPage) && pageType.GetCustomAttributes(typeof(ModdedExclusivePage), false).FirstOrDefault() as ModdedExclusivePage != null && !Plugin.InModdedRoom)
            {
                SwitchPage(ModWarnPage, [CurrentPage != null ? CurrentPage.GetType() : typeof(HomePage)]);
                return;
            }

            if (page != CurrentPage)
            {
                if (CurrentPage != null)
                {
                    CurrentPage.OnSetHeaderRequest -= OnSetHeaderRequest;
                    CurrentPage.OnSetLinesRequest -= OnSetLinesRequest;
                    CurrentPage.OnUpdateLinesRequest -= OnUpdateLinesRequest;
                    CurrentPage.OnPageSwitchRequest -= OnPageSwitchRequest;
                    CurrentPage.OnClose();
                }

                page.OnSetHeaderRequest += OnSetHeaderRequest;
                page.OnSetLinesRequest += OnSetLinesRequest;
                page.OnUpdateLinesRequest += OnUpdateLinesRequest;
                page.OnPageSwitchRequest += OnPageSwitchRequest;
            }

            CurrentPage = page;
            DisplayPage(page, parameters);
        }

        public void DisplayPage() => DisplayPage(CurrentPage, CurrentPage.Parameters);

        private void DisplayPage(IPage page, object[] parameters)
        {
            Type pageType = page.GetType();

            if (pageType != typeof(ModRoomWarningPage) && pageType.GetCustomAttributes(typeof(ModdedExclusivePage), false).FirstOrDefault() as ModdedExclusivePage != null && !Plugin.InModdedRoom)
            {
                SwitchPage(ModWarnPage, [CurrentPage != null ? CurrentPage.GetType() : typeof(HomePage)]);
                return;
            }

            page.Lines = [];
            page.Parameters = parameters;
            page.OnDisplay();
        }

        private void OnSetHeaderRequest(HeaderLine header) => OnHeaderSet?.Invoke(header);
        private void OnSetLinesRequest(object[] lines) => OnLinesSet?.Invoke(lines);
        private void OnUpdateLinesRequest(object[] lines) => OnLinesUpdated?.Invoke(lines);
        private void OnPageSwitchRequest(Type origin, Type type, object[] parameters) => OnPageSwitched?.Invoke(origin, type, parameters);
    }
}
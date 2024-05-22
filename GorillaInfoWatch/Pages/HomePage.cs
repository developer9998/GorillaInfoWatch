using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GorillaInfoWatch.Pages
{
    public class HomePage : Page
    {
        private List<Type> _pages;

        public void SetEntries(List<Type> allPages)
        {
            _pages = [.. GetEntries(allPages, true).Concat(GetEntries(allPages, false))];
        }

        private List<Type> GetEntries(List<Type> allPages, bool useExecutingAssembly)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            IEnumerable<Type> pages = allPages.Where(type => type.Assembly == executingAssembly);
            return useExecutingAssembly ? [.. pages] : [.. allPages.Except(pages)];
        }

        public override void OnDisplay()
        {
            base.OnDisplay();

            SetHeader(string.Format("{0} v{1}", Constants.Name, Constants.Version), "Created by Dev and Lunakitty");

            int searchIndex = 0;
            foreach (Type page in _pages)
            {
                DisplayInHomePage attribute = page.GetCustomAttributes(typeof(DisplayInHomePage), false).FirstOrDefault() as DisplayInHomePage;
                if (attribute != null) AddLine(attribute.DisplayName, button: new LineButton(OnPageSelected, searchIndex));
                searchIndex++;
            }

            SetLines();
        }

        private void OnPageSelected(object sender, ButtonArgs args)
        {
            ShowPage(_pages[args.returnIndex]);
        }
    }
}

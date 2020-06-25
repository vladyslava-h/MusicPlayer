using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MusicPlayer.Infrastructure
{
    class PageFactory
    {
        public Page GetPage(object dataContext, string page)
        {
            Type typePage = Type.GetType($"MusicPlayer.Pages.{page}");
            if (typePage != null)
            {
                var new_page = Activator.CreateInstance(typePage) as Page;
                new_page.DataContext = dataContext;
                return new_page;
            }
            return null;
        }
    }
}

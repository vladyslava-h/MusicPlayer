using MusicPlayer.Infrastructure;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using Ninject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer
{
    class ServiceLocator
    {
        public static IKernel Kernel { get; private set; } = new StandardKernel();

        public static void Setup()
        {
            string[] resourses = File.ReadAllText(Path.GetFullPath(@"..\..\Data\config.txt")).Split(';').ToArray();

            Kernel.Bind<ApplicationPageViewModel>().ToConstant(new ApplicationPageViewModel());
            Kernel.Bind<IIOService<ObservableCollection<FullSong>>>().To<JsonService>();
            Kernel.Bind<ResoursePath>()
                .To<ResoursePath>()
                .WithPropertyValue("ArtistsPath", resourses[0])
                .WithPropertyValue("AlbumsPath", resourses[1])
                .WithPropertyValue("SongsPath", resourses[2]);
        }

        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }

        public MainViewModel MainViewModel
        {
            get => Kernel.Get<MainViewModel>();
        }

    }
}

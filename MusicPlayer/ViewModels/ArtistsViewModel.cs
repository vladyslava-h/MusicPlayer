using MusicPlayer.Infrastructure;
using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicPlayer.ViewModels
{
    class ArtistsViewModel : Notifier, IRecipient, IContentView
    {
        public ArtistsViewModel()
        {
            Songs = new ObservableCollection<FullSong>();
            Artists = new ObservableCollection<Artist>();
            SelectArtistCommand = new RelayCommand(ArtistSongs);
        }
        public ICommand SelectArtistCommand { set; get; }

        #region Member fields

        private IMainPlayer mainwindow;
        private ObservableCollection<Artist> artists;
        private int content_size;
        private string foreground;
        private int fontSize;
        private int fontSizeAdditional;
        #endregion

        #region Properties

        public int FontSize
        {
            set
            {
                fontSize = value;
                Notify();
            }
            get => fontSize;
        }
        public int FontSizeAdditional
        {
            set
            {
                fontSizeAdditional = value;
                Notify();
            }
            get => fontSizeAdditional;
        }
        public string Foreground
        {
            set
            {
                foreground = value;
                Notify();
            }
            get => foreground;
        }
        public ObservableCollection<Artist> Artists
        {
            set
            {
                artists = value;
                Notify();
            }
            get => artists;
        }
        public ObservableCollection<FullSong> Songs { set; get; }
        public int ThemeIndex { get; set; }

        #endregion

        #region Methods
        public void ReceiveData(object data, IMainPlayer sender)
        {
            Songs = data as ObservableCollection<FullSong>;

            if (Songs == null)
                return;

            Artists.Clear();
            foreach (var song in Songs)
                Artists.Add(song.Artist);

            Artists = new ObservableCollection<Artist>(Artists.Distinct());
            mainwindow = sender;
        }


        private void ArtistSongs(object artistID)
        {
            PageFactory pageFactory = new PageFactory();
            IRecipient recipient = new SongsViewModel();
            string name = recipient.GetType().Name;
            var page = pageFactory.GetPage(recipient, $"{name.Substring(0, name.Length - 9)}Page");
            var tmp_songs = new ObservableCollection<FullSong>(Songs.Where(x => x.Artist.ID == (int)artistID).OrderBy(x => x.Album.Title));

            Transference.Send(recipient, tmp_songs, mainwindow);
            (page.DataContext as IContentView).ContentSize(content_size);
            (page.DataContext as IContentView).ContentForeground(ThemeIndex);
            ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage = page;
        }

        public void SortContent(int sort)
        {
            Artists = sort == 0 ? new ObservableCollection<Artist>(Artists.OrderBy(x => x.Name)) :
                        new ObservableCollection<Artist>(Artists.OrderByDescending(x => x.Name));
        }

        public void ContentSize(int size)
        {
            content_size = size;
            FontSize = size == 1 ? 21 : (size == 0 ? 16 : 25);
            FontSizeAdditional = size == 1 ? 14 : (size == 0 ? 9 : 18);
        }

        public void ContentForeground(int color)
        {
            ThemeIndex = color;
            Foreground = color == 0 ? "Black" : "White";
        }
        #endregion
    }
}

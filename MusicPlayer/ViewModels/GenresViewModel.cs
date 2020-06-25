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
    class GenresViewModel : Notifier, IRecipient, IContentView
    {
        public GenresViewModel()
        {
            SelectGenreCommand = new RelayCommand(GenreSongs);
            Genres = new Dictionary<string, int>();
        }
        public ICommand SelectGenreCommand { set; get; }

        #region Member fields

        private IMainPlayer mainwindow;
        private Dictionary<string, int> genres;
        private int content_size;
        private int fontSize;
        private int fontSizeAdditional;
        private string foreground;
        #endregion

        #region Properties
        public ObservableCollection<FullSong> Songs { set; get; }
        public Dictionary<string, int> Genres
        {
            set
            {
                genres = value;
                Notify();
            }
            get => genres;
        }
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

        public int ThemeIndex { get; set; }
        #endregion

        #region Methods
        private void GenreSongs(object index)
        {
            PageFactory pageFactory = new PageFactory();
            IRecipient recipient = new SongsViewModel();
            string name = recipient.GetType().Name;
            var page = pageFactory.GetPage(recipient, $"{name.Substring(0, name.Length - 9)}Page");
            ObservableCollection<FullSong> tmp_songs = null;
            if ((string)index == "Unknown")
                tmp_songs = new ObservableCollection<FullSong>(Songs.Where(x => x.Song.Genre == null || x.Song.Genre == "" || x.Song.Genre == "Unknown"));
            else
                tmp_songs = new ObservableCollection<FullSong>(Songs.Where(x => x.Song.Genre == (string)index));

            Transference.Send(recipient, tmp_songs, mainwindow);
            (page.DataContext as IContentView).ContentSize(content_size);
            (page.DataContext as IContentView).ContentForeground(ThemeIndex);
            ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage = page;
        }

        public void ReceiveData(object data, IMainPlayer sender = null)
        {
            Songs = data as ObservableCollection<FullSong>;

            if (Songs == null)
                return;

            Genres.Clear();
            List<string> genres = new List<string>();
            foreach (var song in Songs)
                genres.Add(song.Song.Genre);

            foreach (var genre in genres)
            {
                if (Genres.ContainsKey(genre ?? "Unknown"))
                    Genres[genre ?? "Unknown"]++;
                else
                    Genres[genre ?? "Unknown"] = 1;
            }

            mainwindow = sender;
        }

        public void SortContent(int sort)
        {
            Genres = sort == 0 ? Genres.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value) :
                Genres.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }
        public void ContentSize(int size)
        {
            content_size = size;
            FontSize = size == 1 ? 20 : (size == 0 ? 16 : 25);
            FontSizeAdditional = size == 1 ? 13 : (size == 0 ? 9 : 18);
        }

        public void ContentForeground(int color)
        {
            ThemeIndex = color;
            Foreground = color == 0 ? "Black" : "White";
        }
        #endregion
    }
}

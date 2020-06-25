using MusicPlayer.Infrastructure;
using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicPlayer.ViewModels
{
    internal class AlbumsViewModel : Notifier, IRecipient, IContentView
    {
        public AlbumsViewModel()
        {
            Songs = new ObservableCollection<FullSong>();
            Albums = new ObservableCollection<Album>();
            SelectAlbumCommand = new RelayCommand(x => AlbumSongs(x));
        }
        public ICommand SelectAlbumCommand { set; get; }

        #region Member fields

        private int content_size;
        private ObservableCollection<Album> albums;
        private int imageSize;
        private int fontSize;
        private string foreground;
        private IMainPlayer mainwindow;

        #endregion

        #region Properties
        public ObservableCollection<FullSong> Songs { set; get; }

        public ObservableCollection<Album> Albums
        {
            set
            {
                albums = value;
                Notify();
            }
            get => albums;
        }

        public int ImageSize
        {
            set
            {
                imageSize = value;
                Notify();
            }
            get => imageSize;
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
        private void AlbumSongs(object albumID)
        {
            PageFactory pageFactory = new PageFactory();
            IRecipient recipient = new SongsViewModel();
            string name = recipient.GetType().Name;
            var page = pageFactory.GetPage(recipient, $"{name.Substring(0, name.Length - 9)}Page");

            var tmp_songs = new ObservableCollection<FullSong>(Songs.Where(x => x.Album.ID == (int)albumID));

            Transference.Send(recipient, tmp_songs, mainwindow);
            (page.DataContext as IContentView).ContentSize(content_size);
            (page.DataContext as IContentView).ContentForeground(ThemeIndex);
            ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage = page;
        }

        public void ReceiveData(object data, IMainPlayer sender)
        {
            Songs = data as ObservableCollection<FullSong>;

            if (Songs == null)
                return;

            Albums.Clear();
            foreach (var song in Songs)
                Albums.Add(song.Album);

            Albums = new ObservableCollection<Album>(Albums.Distinct());
            mainwindow = sender;
        }

        public void SortContent(int sort)
        {
            Albums = sort == 0 ? new ObservableCollection<Album>(Albums.OrderBy(x => x.Title)) :
              new ObservableCollection<Album>(Albums.OrderByDescending(x => x.Title));
        }

        public void ContentSize(int size)
        {
            content_size = size;
            ImageSize = size == 1 ? 210 : (size == 0 ? 100 : 250); 
            FontSize = size == 1 ? 24 : (size == 0 ? 19 : 28); 
        }

        public void ContentForeground(int color)
        {
            ThemeIndex = color;
            Foreground = color == 0 ? "Black" : "White";
        }

        #endregion
    }
}

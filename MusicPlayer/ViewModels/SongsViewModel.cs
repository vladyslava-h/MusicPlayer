using MusicPlayer.Infrastructure;
using MusicPlayer.Models;
using MusicPlayer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MusicPlayer.ViewModels
{
    class SongsViewModel : Notifier, IRecipient, IContentView
    {
        public SongsViewModel()
        {
            Songs = new ObservableCollection<FullSong>();
            InitCommands();
        }

        #region Member Fields

        private IMainPlayer mainwindow;
        private ObservableCollection<FullSong> songs;
        private int imageSize;
        private int fontSize;
        private string foreground;
        #endregion

        #region Properties

        public ObservableCollection<FullSong> Songs
        {
            set
            {
                songs = value;
                Notify();
            }
            get => songs;
        }
        public FullSong CurrentlySelectedSong { set; get; }
        public FullSong CurrentlyPlayingSong { set; get; }
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

        #region Commands

        public ICommand PlayCommand { set; get; }
        public ICommand RemoveCommand { set; get; }
        public ICommand EditCommand { set; get; }

        #endregion

        #region Methods
        private void InitCommands()
        {
            PlayCommand = new RelayCommand(SendSelectedSong);
            RemoveCommand = new RelayCommand(x =>
            {
                bool reload_page = false;
                FullSong song = new FullSong();
                song = Songs.Where(s => s.Song == (Song)x).First();
                Songs.Remove(song);
                if (Songs.Count == 0)
                    reload_page = true;

                mainwindow.RemoveSong(song, reload_page);
            });
            EditCommand = new RelayCommand(x =>
            {
                IRecipient recipient = new EditViewModel();
                EditView view = new EditView();
                FullSong song = new FullSong(Songs.Where(s => s.Song == (Song)x).First());
                Transference.Send(recipient, song, mainwindow);
                view.DataContext = recipient;
                view.ShowDialog();
                if (song.Artist.ID != -1)
                    mainwindow.AddSong(song);
            });
        }

        public void ReceiveData(object data, IMainPlayer sender)
        {
            Songs = data as ObservableCollection<FullSong>;
            if (Songs == null)
                return;
            mainwindow = sender;
        }

        private void SendSelectedSong(object param)
        {
            CurrentlySelectedSong = Songs.Where(y => y.Song.Path == param.ToString()).FirstOrDefault();
            mainwindow.PlayingSong = CurrentlySelectedSong;
            if (!mainwindow.Playlist.Equals(Songs))
                mainwindow.Playlist = Songs;
        }

        public void SortContent(int sort)
        {
            Songs = sort == 0 ? new ObservableCollection<FullSong>(Songs.OrderBy(x => x.Song.Title)) :
                new ObservableCollection<FullSong>(Songs.OrderByDescending(x => x.Song.Title));
        }

        public void ContentSize(int size)
        {
            ImageSize = size == 1 ? 35 : (size == 0 ? 30 : 45);
            FontSize = size == 1 ? 19 : (size == 0 ? 16 : 22);
        }

        public void ContentForeground(int color)
        {
            Foreground = color == 0 ? "Black" : "White";
        }

        #endregion
    }
}

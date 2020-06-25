using MusicPlayer.Infrastructure;
using MusicPlayer.Models;
using MusicPlayer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MusicPlayer.ViewModels
{
    class MainViewModel : Notifier, IMainPlayer
    {
        public MainViewModel(IIOService<ObservableCollection<FullSong>> service)
        {
            InitCommands();
            InitRecipients();
            inOutService = service;

            songs = inOutService.Load();

            pageFactory = new PageFactory();

            mediaPlayer = new MediaPlayer();
            Playlist = new ObservableCollection<FullSong>();
            PlayButtonContent = "Play";

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(TimerTick);
            tick = new timerTick(ChangeStatus);
            Settings();

        }

        #region Commands

        public ICommand GoBackCommand { set; get; }
        public ICommand ResizeWindowCommand { set; get; }
        public ICommand CloseWindowCommand { set; get; }
        public ICommand HideWindowCommand { set; get; }
        public ICommand PlayCommand { set; get; }
        public ICommand NextSongCommand { set; get; }
        public ICommand PreviousSongCommand { set; get; }
        public ICommand ScanDirectoryCommand { set; get; }
        public ICommand AddFileCommand { set; get; }

        #endregion


        #region Member fields

        private ObservableCollection<FullSong> songs;
        private IIOService<ObservableCollection<FullSong>> inOutService;

        private string playButtonContent;
        private Thickness contentSliderMargin;

        private MediaPlayer mediaPlayer;
        private int selectedMenuItem;
        private FullSong playingSong;

        private PageFactory pageFactory;
        private List<IRecipient> recipients;

        private TimeSpan status;
        private int statusSeconds;
        private int maximumStatus;

        private timerTick tick;
        public delegate void timerTick();
        private DispatcherTimer timer;

        private int sortOption;
        private int layoutSize;
        private int themeIndex;
        private int language;


        #endregion


        #region Properties
        public Thickness ContentSliderMargin
        {
            set
            {
                contentSliderMargin = value;
                Notify();
            }
            get => contentSliderMargin;
        }
        public int SelectedMenuItem
        {
            set
            {
                selectedMenuItem = value;
                MoveContentSlider();
                ChangePage();
            }
            get => selectedMenuItem;
        }
        public ObservableCollection<FullSong> Playlist { set; get; }
        public FullSong PlayingSong
        {
            set
            {
                playingSong = value;
                PlayButtonContent = "Play";
                mediaPlayer.Open(new Uri(PlayingSong.Song.Path));
                Status = new TimeSpan(0, 0, 0);
                MaximumStatus = value.Song.Duration.Seconds + (value.Song.Duration.Minutes * 60);
                PlayCommand.Execute(null);
                Notify();
            }
            get => playingSong;
        }
        public string PlayButtonContent
        {
            set
            {
                playButtonContent = value;
                Notify();
            }
            get => playButtonContent;
        }
        public TimeSpan Status
        {
            set
            {
                status = value;
                Notify();
            }
            get => status;
        }
        public int StatusSeconds
        {
            set
            {
                statusSeconds = value;
                Notify();
            }
            get => statusSeconds;
        }
        public int MaximumStatus
        {
            set
            {
                maximumStatus = value;
                Notify();
            }
            get => maximumStatus;
        }
        public int SortOption
        {
            set
            {
                sortOption = value;
                if (ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage != null)
                {
                    var datacontext = ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage.DataContext;
                    (datacontext as IContentView).SortContent(value);
                }
                Notify();
            }
            get => sortOption;
        }
        public int LayoutSize
        {
            set
            {
                layoutSize = value;
                if (ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage != null)
                {
                    var datacontext = ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage.DataContext;
                    (datacontext as IContentView).ContentSize(value);
                }
                Notify();
            }
            get => layoutSize;
        }
        public int ThemeIndex
        {
            set
            {
                themeIndex = value;
                Notify();
                ChangeTheme();
                
                foreach(var song in songs)
                {
                    if(song.Album.HasImage == false)
                    {
                        byte[] image = File.ReadAllBytes(Path.GetFullPath(@"..\..\Images\album_default_light.png"));
                        if (themeIndex == 1)
                            image = File.ReadAllBytes(Path.GetFullPath(@"..\..\Images\album_default_dark.png"));
                        song.Album.AlbumCover = BitmapFrame.Create(new MemoryStream(image));
                    }
                }

                if (ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage != null)
                {
                    var datacontext = ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage.DataContext;
                    (datacontext as IContentView).ContentForeground(value);
                }

            }
            get => themeIndex;
        }
        public int Language
        {
            set
            {
                language = value;
                Notify();
                ChangeLanguage();
            }
            get => language;
        }
        #endregion


        #region Methods
        public void InitCommands()
        {
            //Frame Navigation Commands
            GoBackCommand = new RelayCommand(x => ChangePage());

            //Window State Commands
            CloseWindowCommand = new RelayCommand(x =>
            {
                inOutService.Save(songs);
                inOutService.SaveSettings($"{ThemeIndex} {SortOption} {Language} {LayoutSize}");
                Application.Current.Shutdown();

            });
            HideWindowCommand = new RelayCommand(x => Application.Current.MainWindow.WindowState = WindowState.Minimized);
            ResizeWindowCommand = new RelayCommand(x =>
            {
                if (Application.Current.MainWindow.WindowState == WindowState.Normal)
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                else
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
            });

            //Audio Player Commands
            PlayCommand = new RelayCommand(x =>
            {
                PlayButtonContent = PlayButtonContent == "Pause" ? "Play" : "Pause";
                if (PlayButtonContent == "Play")
                    Pause();
                else Play();
            });

            NextSongCommand = new RelayCommand(x => NextSong());
            PreviousSongCommand = new RelayCommand(x => PreviousSong());

            //Add Data Commands
            ScanDirectoryCommand = new RelayCommand(x =>
            {
                string path = string.Empty;
                System.Windows.Forms.FolderBrowserDialog browser = new System.Windows.Forms.FolderBrowserDialog();

                if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    path = browser.SelectedPath;
                    songs = new ObservableCollection<FullSong>(CollectionFactory.ScanDirectory(songs, path, ThemeIndex));
                }
                ChangePage();
            });

            AddFileCommand = new RelayCommand(x =>
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "Music Files|*.mp3";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var tuple = CollectionFactory.GetColections(songs) as Tuple<List<Artist>, List<Album>, List<Song>>;
                    int artistID = tuple.Item1.Count;
                    int albumID = tuple.Item2.Count;
                    FullSong tmp = new FullSong(dialog.FileName, albumID, artistID);
                    songs = new ObservableCollection<FullSong>(CollectionFactory.AddItem(songs, tmp, ThemeIndex ,false));
                    ChangePage();
                }
            });
        }

        public void InitRecipients()
        {
            recipients = new List<IRecipient>
            {
                new SongsViewModel(),
                new ArtistsViewModel(),
                new AlbumsViewModel(),
                new GenresViewModel()
            };
        }

        public void Settings()
        {
            string settings_str = inOutService.GetSettings();
            int[] settings = settings_str.Split(' ').Select(x => Convert.ToInt32(x)).ToArray();
            ThemeIndex = settings[0];
            SortOption = settings[1];
            Language = settings[2];
            LayoutSize = settings[3];
        }

        private async void MoveContentSlider()
        {
            await Task.Factory.StartNew(() =>
            {
                for (double i = -5; i < 0; i++)
                {
                    ContentSliderMargin = new Thickness(i, -160 + (80 * selectedMenuItem + 1), 0, 0);
                    Thread.Sleep(45);
                }
            });
        }

        public void ChangePage()
        {
            IRecipient recipient = SelectedMenuItem == 4 ? recipients[0] : recipients[selectedMenuItem];
            string name = recipient.GetType().Name;
            var page = pageFactory.GetPage(recipient, $"{name.Substring(0, name.Length - 9)}Page");
            ObservableCollection<FullSong> collection = SelectedMenuItem != 4 ? new ObservableCollection<FullSong>(songs) :
                                                        new ObservableCollection<FullSong>(songs.Where(x => x.Song.IsLiked == true));
            Transference.Send(page.DataContext as IRecipient, collection, this);
            (page.DataContext as IContentView).SortContent(SortOption);
            (page.DataContext as IContentView).ContentForeground(ThemeIndex);
            (page.DataContext as IContentView).ContentSize(LayoutSize);
            ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage = page;
        }

        public void RemoveSong(FullSong song, bool reload_page)
        {
            songs.Remove(songs.Where(x => x.Song.Path == song.Song.Path).FirstOrDefault());
            songs = new ObservableCollection<FullSong>(CollectionFactory.Update(songs, ThemeIndex));
            if (reload_page)
                ChangePage();
        }

        public void AddSong(FullSong song)
        {
            songs = new ObservableCollection<FullSong>(CollectionFactory.AddItem(songs, song, ThemeIndex));
        }

        void TimerTick(object sender, EventArgs e)
        {
            tick.Invoke();
        }

        public void Play()
        {
            mediaPlayer.Play();
            timer.Start();
        }

        public void Pause()
        {
            timer.Stop();
            mediaPlayer.Pause();
        }

        public void NextSong()
        {
            if (Playlist.Count == 0)
                return;
            int index = Playlist.IndexOf(PlayingSong);
            PlayingSong = index == Playlist.Count - 1 ? Playlist.ElementAt(0) : Playlist.ElementAt(++index);
        }

        public void PreviousSong()
        {
            if (Playlist.Count == 0)
                return;
            int index = Playlist.IndexOf(PlayingSong);
            PlayingSong = index == 0 ? Playlist.ElementAt(Playlist.Count - 1) : PlayingSong = Playlist.ElementAt(--index);
        }

        void ChangeStatus()
        {
            Status = Status.Add(new TimeSpan(0, 0, 1));
            StatusSeconds = Status.Seconds + (Status.Minutes * 60);
            if (StatusSeconds == MaximumStatus)
                NextSong();
        }

        void ChangeTheme()
        {
            App.Current.Resources.MergedDictionaries.RemoveAt(App.Current.Resources.MergedDictionaries.Count - 1);
            if (themeIndex == 0)
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Dictionaries/LightTheme.xaml", UriKind.Relative) });
            else
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Dictionaries/DarkTheme.xaml", UriKind.Relative) });
        }

        void ChangeLanguage()
        {
            App.Current.Resources.MergedDictionaries.RemoveAt(0);
            if (language == 0)
                App.Current.Resources.MergedDictionaries.Insert(0, new ResourceDictionary() { Source = new Uri("/Dictionaries/en-US.xaml", UriKind.Relative) });
            else
                App.Current.Resources.MergedDictionaries.Insert(0, new ResourceDictionary() { Source = new Uri("/Dictionaries/ko-KR.xaml", UriKind.Relative) });
        }

        #endregion
    }
}

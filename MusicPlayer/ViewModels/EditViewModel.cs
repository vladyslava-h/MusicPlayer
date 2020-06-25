using MusicPlayer.Infrastructure;
using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MusicPlayer.ViewModels
{
    class EditViewModel : Notifier, IRecipient
    {
        public EditViewModel()
        {
            Song = new FullSong();
            CancelCommand = new RelayCommand(x =>
            {
                Song.Artist.ID = -1;
                (x as Window).Close();
            });
            SaveCommand = new RelayCommand(Save);
            ChangeImageCommand = new RelayCommand(ChangeImage);
        }

        #region Member fields

        private string image_path;
        private BitmapFrame image;
        private FullSong song;
        private string backgroundImage;
        #endregion

        #region Commands
        public ICommand SaveCommand { set; get; }
        public ICommand CancelCommand { set; get; }
        public ICommand ChangeImageCommand { set; get; }
        #endregion

        #region Properties

        public FullSong Song
        {
            set
            {
                song = value;
                Notify();
            }
            get => song;
        }

        public string BackgroundImage
        {
            set
            {
                backgroundImage = value;
                Notify();
            }
            get => backgroundImage;
        }
        public BitmapFrame Image
        {
            set
            {
                image = value;
                Notify();
            }
            get => image;
        }

        #endregion

        #region Methods
        public void ReceiveData(object data, IMainPlayer sender)
        {
            SetBackgroundImage((sender as MainViewModel).ThemeIndex);
            Song = data as FullSong;
            Image = BitmapFrame.Create(new MemoryStream(Song.Album.Image));
        }

        private void Save(object data)
        {
            TagLib.File file = TagLib.File.Create(Song.Song.Path);
            file.Tag.Title = Song.Song.Title;
            file.Tag.Album = Song.Album.Title;
            file.Tag.Performers = new string[] { Song.Artist.Name };
            file.Tag.Year = (uint)Song.Album.Year;
            file.Tag.Genres = new string[] { Song.Song.Genre??"" };

            if (image_path != null)
            {
                var pic = new TagLib.IPicture[1];
                pic[0] = new TagLib.Picture(image_path);
                file.Tag.Pictures = pic;
            }

            file.Save();

            if (Song.Album.Title == null || Song.Album.Title == "")
                Song.Album.Title = "Unknown";
            if (Song.Artist.Name == null || Song.Artist.Name == "")
                Song.Artist.Name = "Unknown";
            if (Song.Song.Title == null || Song.Song.Title == "")
                Song.Song.Title = Path.GetFileNameWithoutExtension(file.Name);
            if (Song.Song.Genre == null || Song.Song.Genre == "")
                Song.Song.Genre = "Unknown";
            (data as Window).Close();
        }

        private void ChangeImage(object data)
        {
            image_path = GetImagePath();
            if (image_path != null)
            {
                byte[] image = File.ReadAllBytes(image_path);
                Song.Album.Image = image;
                Song.Album.AlbumCover = BitmapFrame.Create(new MemoryStream(image));
                Song.Album.HasImage = true;
                Image = BitmapFrame.Create(new MemoryStream(Song.Album.Image));
            }
        }

        private string GetImagePath()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png";

            if (dialog.ShowDialog() == DialogResult.OK)
                return dialog.FileName;
            return null;
        }

        public void SetBackgroundImage(int img)
        {
            BackgroundImage = img == 0 ? "/Images/1.png" : "/Images/2.png";
        }
        #endregion
    }
}

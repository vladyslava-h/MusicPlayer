using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MusicPlayer.Models
{
    class FullSong
    {
        public Song Song { set; get; }
        public Artist Artist { set; get; }
        public Album Album { set; get; }

        public FullSong() { }

        public FullSong(FullSong song)
        {
            this.Album = new Album
            {
                AlbumCover = song.Album.AlbumCover,
                ArtistID = song.Album.ArtistID,
                ID = song.Album.ID,
                Image = song.Album.Image,
                Title = song.Album.Title ?? "Unknown",
                Year = song.Album.Year
            };
            this.Artist = new Artist
            {
                ID = song.Artist.ID,
                Name = song.Artist.Name??"Unknown",
                TotalAlbums = song.Artist.TotalAlbums,
                TotalSongs = song.Artist.TotalSongs
            };
            this.Song = new Song
            {
                Title = song.Song.Title ?? "Unknown",
                Genre = song.Song.Genre,
                Duration = song.Song.Duration,
                AlbumID = song.Song.AlbumID,
                IsLiked = song.Song.IsLiked,
                Path = song.Song.Path
            };

        }

        public FullSong(string path, int albumID, int artistID)
        {
            TagLib.File file = TagLib.File.Create(path);

            MemoryStream stream = new MemoryStream();
            byte[] image = null;
            try
            {
                TagLib.IPicture pic = file.Tag.Pictures[0];
                stream = new MemoryStream(pic.Data.Data);
                image = pic.Data.Data;
            }
            catch
            {
                image = File.ReadAllBytes(Path.GetFullPath(@"..\..\Images\album_default_light.png"));
                stream = new MemoryStream(image);
            }

            this.Album = new Album
            {
                AlbumCover = BitmapFrame.Create(stream),
                ArtistID = artistID,
                ID = albumID,
                Image = image,
                Title = file.Tag.Album ?? "Unknown",
                Year = (int)file.Tag.Year
            };
            this.Artist = new Artist
            {
                ID = artistID,
                Name = file.Tag.FirstPerformer ?? "Unknown",
            };
            this.Song = new Song
            {
                Title = file.Tag.Title??file.Name,
                Genre = file.Tag.FirstGenre,
                Duration = new TimeSpan(0, file.Properties.Duration.Minutes, file.Properties.Duration.Seconds),
                AlbumID = albumID,
                IsLiked = false,
                Path = path
            };

        }
    }
}

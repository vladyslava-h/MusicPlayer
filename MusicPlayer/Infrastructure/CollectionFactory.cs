using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MusicPlayer.Infrastructure
{
    class CollectionFactory
    {

        public static ObservableCollection<FullSong> Combine(List<Artist> artists, List<Album> albums, List<Song> songs, int theme_index)
        {
            ObservableCollection<FullSong> collection = new ObservableCollection<FullSong>();
            var image_path = Path.GetFullPath(@"..\..\Images\album_default_light.png");
            if (theme_index == 1)
                image_path = Path.GetFullPath(@"..\..\Images\album_default_dark.png");

            if (ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage != null)
                image_path = (ServiceLocator.Get<ApplicationPageViewModel>().CurrentPage.DataContext as IContentView).ThemeIndex == 0 ?
                    Path.GetFullPath(@"..\..\Images\album_default_light.png") :
                    Path.GetFullPath(@"..\..\Images\album_default_dark.png");

            for (int i = 0; i < songs.Count; i++)
            {
                FullSong tmp_song = new FullSong();
                if (!File.Exists(songs[i].Path))
                    continue;
                tmp_song.Song = songs.ElementAt(i);


                tmp_song.Album = albums.Where(x => x.ID == tmp_song.Song.AlbumID).FirstOrDefault();
                if (tmp_song.Album.HasImage == true)
                {
                    try
                    {
                        tmp_song.Album.AlbumCover = BitmapFrame.Create(new MemoryStream(tmp_song.Album.Image));
                    }
                    catch
                    {
                        tmp_song.Album.AlbumCover = BitmapFrame.Create(new Uri(image_path));
                        tmp_song.Album.HasImage = false;
                    }
                }

                tmp_song.Artist = artists.Where(x => x.ID == tmp_song.Album.ArtistID).FirstOrDefault();
                tmp_song.Artist.TotalAlbums = albums.Where(x => x.ArtistID == tmp_song.Artist.ID).Count();

                collection.Add(tmp_song);
            }

            foreach (var artist in collection)
            {
                int totalsongs = 0;
                foreach (var album in albums)
                {
                    if (album.ArtistID == artist.Artist.ID)
                        totalsongs += songs.Where(x => x.AlbumID == album.ID).Count();
                }
                artist.Artist.TotalSongs = totalsongs;
            }


            return collection;
        }

        public static object GetColections(ObservableCollection<FullSong> collection)
        {
            List<Artist> artists = new List<Artist>();
            List<Album> albums = new List<Album>();
            List<Song> songs = new List<Song>();

            foreach (var fullsong in collection)
            {
                artists.Add(fullsong.Artist);
                albums.Add(fullsong.Album);
                songs.Add(fullsong.Song);
            }

            artists = new List<Artist>(artists.Distinct());
            albums = new List<Album>(albums.Distinct());

            return new Tuple<List<Artist>, List<Album>, List<Song>>(artists, albums, songs);

        }

        public static ObservableCollection<FullSong> AddItem(ObservableCollection<FullSong> songs, FullSong song, int theme_index, bool insert = true)
        {

            var tuple = GetColections(songs) as Tuple<List<Artist>, List<Album>, List<Song>>;

            TagLib.File file = TagLib.File.Create(song.Song.Path);

            if (insert == false) //if song already exists
            {
                foreach (var alb in tuple.Item2)
                {
                    if (tuple.Item3.Where(x => x.Path == song.Song.Path && x.AlbumID == alb.ID).FirstOrDefault() != null)
                        return songs;
                }
            }

            Album album = new Album();
            album = tuple.Item2.Where(x => x.ID == song.Album.ID && x.Title == song.Album.Title).FirstOrDefault();
            Artist artist = new Artist();
            artist = tuple.Item1.Where(x => x.Name == song.Artist.Name).FirstOrDefault();

            if (album == null)
                song.Song.AlbumID = song.Album.ID = tuple.Item2.Count;
            if (artist == null)
            {
                song.Album.ArtistID = song.Artist.ID = tuple.Item1.Count;
                song.Song.AlbumID = song.Album.ID = tuple.Item2.Count;
            }
            else //if artist already exists
            {
                album = tuple.Item2.Where(x => x.Title == song.Album.Title && x.ArtistID == artist.ID).FirstOrDefault();
                if (album == null)
                {
                    MemoryStream stream = new MemoryStream();
                    bool hasImage = true;
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
                        if (theme_index == 1)
                            image = File.ReadAllBytes(Path.GetFullPath(@"..\..\Images\album_default_dark.png"));
                        hasImage = false;
                        stream = new MemoryStream(image);
                    }

                    song.Album = new Album
                    {
                        Title = file.Tag.Album ?? "Unknown",
                        Year = Convert.ToInt32(file.Tag.Year),
                        Image = image,
                        AlbumCover = BitmapFrame.Create(stream),
                        HasImage = hasImage
                    };
                    song.Song.AlbumID = song.Album.ID = tuple.Item2.Count;
                }
                else
                {
                    var image = song.Album.Image;
                    int year = song.Album.Year;
                    song.Album = album;
                    song.Album.Image = image;
                    song.Album.Year = year;
                    song.Album.AlbumCover = BitmapFrame.Create(new MemoryStream(image));
                    song.Song.AlbumID = song.Album.ID;
                }
            }

            song.Album.HasImage = false;
            songs.Remove(songs.Where(x => x.Song.Path == song.Song.Path).FirstOrDefault());
            songs.Add(song);
            return CollectionFactory.Update(songs, theme_index);
        }

        public static ObservableCollection<FullSong> Update(ObservableCollection<FullSong> collection, int theme_index)
        {
            var tuple = GetColections(collection) as Tuple<List<Artist>, List<Album>, List<Song>>;
            return CollectionFactory.Combine(tuple.Item1, tuple.Item2, tuple.Item3, theme_index);
        }

        public static ObservableCollection<FullSong> ScanDirectory(ObservableCollection<FullSong> collection, string path, int theme_index)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            var tuple = GetColections(collection) as Tuple<List<Artist>, List<Album>, List<Song>>;

            List<Artist> artists = tuple.Item1;
            List<Album> albums = tuple.Item2;
            List<Song> songs = tuple.Item3;

            foreach (var fileInfo in directory.EnumerateFiles("*.mp3", SearchOption.AllDirectories))
            {
                TagLib.File file = TagLib.File.Create(fileInfo.FullName);
                string tmp = null;
                int tmp_artistID = -1;
                int tmp_albumID = -1;

                //add artist
                bool notExists = true;
                tmp = file.Tag.FirstPerformer ?? "Unknown";
                foreach (var artist in artists)
                {
                    if (artist.Name == tmp)
                    {
                        notExists = false;
                        tmp_artistID = artist.ID;
                        break;
                    }
                }
                if (tmp_artistID == -1)
                    tmp_artistID = artists.Count;
                if (notExists)
                    artists.Add(new Artist { Name = tmp, ID = artists.Count });


                //add album
                notExists = true;
                foreach (var album in albums)
                {
                    if (album.Title == file.Tag.Album)
                    {
                        notExists = false;
                        tmp_albumID = album.ID;
                        break;
                    }
                }
                if (tmp_albumID == -1)
                    tmp_albumID = albums.Count;

                if (notExists)
                {
                    MemoryStream stream = new MemoryStream();
                    bool hasImage = true;
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
                        if (theme_index == 1)
                            image = File.ReadAllBytes(Path.GetFullPath(@"..\..\Images\album_default_dark.png"));
                        hasImage = false;
                        stream = new MemoryStream(image);
                    }

                    albums.Add(new Album
                    {
                        Title = file.Tag.Album ?? "Unknown",
                        ArtistID = tmp_artistID,
                        Year = Convert.ToInt32(file.Tag.Year),
                        Image = image,
                        AlbumCover = BitmapFrame.Create(stream),
                        ID = albums.Count,
                        HasImage = hasImage
                    });
                }

                if (songs.Where(x => x.Title == (file.Tag.Title ?? file.Name) && x.AlbumID == tmp_albumID).FirstOrDefault() != null)
                    continue;

                songs.Add(new Song
                {
                    Title = file.Tag.Title ?? file.Name,
                    AlbumID = tmp_albumID,
                    Genre = file.Tag.FirstGenre,
                    Path = file.Name,
                    Duration = new TimeSpan(0, file.Properties.Duration.Minutes, file.Properties.Duration.Seconds)
                });
            }

            return CollectionFactory.Combine(artists, albums, songs, theme_index);
        }

    }
}

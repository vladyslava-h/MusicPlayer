using MusicPlayer.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Models
{
    class JsonService : IIOService<ObservableCollection<FullSong>>
    {
        ResoursePath path;
        public JsonService(ResoursePath path)
        {
            this.path = path;
        }

        public ObservableCollection<FullSong> Load()
        {
            if (!File.Exists($"{Path.GetFullPath(@"..\..\Data")}\\{path.ArtistsPath}") ||
               !File.Exists($"{Path.GetFullPath(@"..\..\Data")}\\{path.AlbumsPath}") ||
               !File.Exists($"{Path.GetFullPath(@"..\..\Data")}\\{path.SongsPath}"))
                return new ObservableCollection<FullSong>();

            List<Artist> artists = null;
            List<Album> albums = null;
            List<Song> songs = null;
            try
            {
                artists =
                    JsonConvert.DeserializeObject<List<Artist>>(File.ReadAllText($"{Path.GetFullPath(@"..\..\Data")}\\{path.ArtistsPath}"));
                albums =
                    JsonConvert.DeserializeObject<List<Album>>(File.ReadAllText($"{Path.GetFullPath(@"..\..\Data")}\\{path.AlbumsPath}"));
                songs =
                    JsonConvert.DeserializeObject<List<Song>>(File.ReadAllText($"{Path.GetFullPath(@"..\..\Data")}\\{path.SongsPath}"));

            }
            catch { }

            if (songs != null && albums != null && artists != null)
            {
                string settings_str = GetSettings();
                int[] settings = settings_str.Split(' ').Select(x => Convert.ToInt32(x)).ToArray();
                return CollectionFactory.Combine(artists, albums, songs, settings[0]);
            }
            return new ObservableCollection<FullSong>();
        }

        public void Save(ObservableCollection<FullSong> data)
        {
            List<Artist> artists = new List<Artist>();
            List<Album> albums = new List<Album>();
            List<Song> songs = new List<Song>();

            foreach (var fullsong in data)
            {
                artists.Add(fullsong.Artist);
                albums.Add(fullsong.Album);
                songs.Add(fullsong.Song);
            }

            File.WriteAllText($"{Path.GetFullPath(@"..\..\Data")}\\{path.ArtistsPath}",
                JsonConvert.SerializeObject(artists.Distinct(), Formatting.Indented));
            File.WriteAllText($"{Path.GetFullPath(@"..\..\Data")}\\{path.AlbumsPath}",
                JsonConvert.SerializeObject(albums.Distinct(), Formatting.Indented));
            File.WriteAllText($"{Path.GetFullPath(@"..\..\Data")}\\{path.SongsPath}",
                JsonConvert.SerializeObject(songs.Distinct(), Formatting.Indented));

        }

        public void SaveSettings(string settings)
        {
            File.WriteAllText(Path.GetFullPath(@"..\..\Data\settings_config.json"),
                                 JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public string GetSettings()
        {
            return JsonConvert.DeserializeObject<string>(File.ReadAllText(Path.GetFullPath(@"..\..\Data\settings_config.json")));
        }
    }
}

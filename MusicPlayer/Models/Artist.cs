using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Models
{
    class Artist
    {
        public string Name { set; get; }
        public int ID { set; get; }

        [JsonIgnore]
        public int TotalAlbums { set; get; } = 0;
        [JsonIgnore]
        public int TotalSongs { set; get; } = 0;

    }
}

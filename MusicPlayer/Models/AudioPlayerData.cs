using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Models
{
    class AudioPlayerData
    {
        public ObservableCollection<Artist> Artists { get; set; }

        public AudioPlayerData() { }
        public AudioPlayerData(ObservableCollection<Artist> artists)
        {
            Artists = artists;
        }
    }
}

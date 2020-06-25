using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Infrastructure
{
    interface IMainPlayer
    {
        FullSong PlayingSong { set; get; }
        ObservableCollection<FullSong> Playlist { set; get; }
        void RemoveSong(FullSong songs, bool reload_page);
        void AddSong(FullSong song);
    }
}

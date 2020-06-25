using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Models
{
    class Song
    {
        public string Title { set; get; }
        public int AlbumID { set; get; }
        public string Genre { set; get; }
        public bool IsLiked { set; get; }
        public string Path { set; get; }
        public TimeSpan Duration { set; get; }
    }
}


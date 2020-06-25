using MusicPlayer.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MusicPlayer.Models
{
    class Album : Notifier
    {
        public string Title { set; get; }

        public int Year { set; get; }

        public int ArtistID { set; get; }

        public int ID { set; get; }
            
        public byte[] Image { set; get; }

        [JsonIgnore]
        private BitmapFrame albumCover;
        [JsonIgnore]
        public BitmapFrame AlbumCover 
        {
            set
            {
                albumCover = value;
                Notify();
            }
            get => albumCover;
        }

        public bool HasImage { set; get; } = true;

    }
}

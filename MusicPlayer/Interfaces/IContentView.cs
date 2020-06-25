using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Infrastructure
{
    interface IContentView
    {
        void SortContent(int sort);
        void ContentSize(int size);
        void ContentForeground(int color);
        int ThemeIndex { set; get; }
    }
}

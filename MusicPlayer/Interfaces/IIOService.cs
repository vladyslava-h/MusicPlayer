using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Infrastructure
{
    interface IIOService<T>
    {
        void Save(T data);
        T Load();

        string GetSettings();
        void SaveSettings(string settings);
    }
}

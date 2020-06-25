using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Infrastructure
{
    interface IRecipient
    {
        void ReceiveData(object data, IMainPlayer sender = null);
    }
}

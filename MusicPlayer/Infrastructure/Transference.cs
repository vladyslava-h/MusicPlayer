using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Infrastructure
{
    class Transference
    {
        public static void Send(IRecipient recipient, object data, IMainPlayer sender = null)
        {
            recipient.ReceiveData(data, sender);
        }
    }
}

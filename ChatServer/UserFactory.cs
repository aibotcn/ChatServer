using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class UserFactory
    {
        public static void UserSession(object threadContext)
        {
            Socket userSocket = (Socket)threadContext;
            SocketUser user = new SocketUser(userSocket);
            user.Chat();
        }
    }
}

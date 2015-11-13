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
            User user = new User(userSocket);
            user.Chat();
        }
    }
}

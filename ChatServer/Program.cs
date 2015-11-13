using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {   
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 12345);
            Socket serverListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverListener.Bind(ipep);
            serverListener.Listen(100);
            Log.append("Server start and listen...");

            while (true)
            {
                Socket userSocket = serverListener.Accept();
                ThreadPool.QueueUserWorkItem(UserFactory.UserSession, userSocket);
            }          
            //listensock.Close();
        }
    }
}

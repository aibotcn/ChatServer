using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ChatServer
{
    /* User status class
     * indicate user configuration status.
     * */
    class UserStatus
    {
        private bool connected;
        private string userName;
        private IPEndPoint useripep;
        private Hall userHall;
        //Connection status property
        public bool Connected
        {
            get { return connected; }
            set { connected = value; }
        }

        //UserName property
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        //user client property
        public IPEndPoint UserIPEP
        {
            get { return useripep; }
            set { useripep = value; }
        }

        //user chat hall
        public Hall UserHall
        {
            get { return userHall; }
            set { userHall = value; }
        }

        //Constructor
        public UserStatus(bool _connected = true, string _name = "", IPEndPoint _ipep = null, Hall _userHall = null)
        {
            connected = _connected;
            userName = _name;
            useripep = _ipep;
            userHall = _userHall;
        }      
    }
}

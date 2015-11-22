using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    /* User is one person who chat in the server.
     * User needs one legally name when login.
     * User can speak with others in the same room.
     * User can use series command, e.g. '/join', '/quit'.
     * */
    class SocketUser : IUser
    {
        private Socket userSocket = null;
        private NetworkStream networkStream = null;
        private StreamReader streamReader = null;
        private StreamWriter streamWriter = null;
        private UserStatus userStatus = null;
        private Command command = null;
        private Hall hall = null;

        public SocketUser(Socket _userSocket)
        {
            userSocket = _userSocket;
            networkStream = new NetworkStream(userSocket);
            streamReader = new System.IO.StreamReader(networkStream);
            streamWriter = new System.IO.StreamWriter(networkStream);
            streamWriter.AutoFlush = true;
            command = new Command(this);  //assign command
            hall = Hall.GetInstance();  //assign chat hall
            userStatus = new UserStatus(true, "", (IPEndPoint)userSocket.RemoteEndPoint, hall);
        }

        //Chat process, loop on thread.
        public void Chat()
        {
            Log.append("Connect " + userStatus.UserIPEP.Address + ":" + userStatus.UserIPEP.Port);
            Login();            

            while (userStatus.Connected)
            {
                string receiveString = ReceiveFromClient();
                if (receiveString != "")
                    MessageProcessing(receiveString);                              
            }
            Logout();//chat terminated. clear resources.
        }

        public void MessageProcessing(string message)
        {
            command.Execute(message);
        }

        /* User login server.
         * */
        public void Login()
        {
            SendToClient("<= welcome to Yulong's chat server\r\n<= Login Name?(only accept letter, number, and '_')\r\n=> ");
            while (userStatus.Connected)
            {
                string name = ReceiveFromClient();
                name = name.Trim();
                if (name != "")
                {
                    name = Regex.Replace(name, "[^a-zA-Z0-9_]", "");

                    if (hall.ContainUser(name))
                    {
                        SendToClient("<= Sorry, name taken.\r\n<= Login Name?\r\n=> ");
                    }
                    else
                    {
                        userStatus.UserName = name;
                        hall.AddUserToHall(this);
                        SendToClient("<= Welcome " + userStatus.UserName + "!\r\n=> ");
                        break;
                    }
                }
            }
        }

        /* User quit server.
         * */
        public void Logout()
        {
            hall.RemoveUserFromHall(this);
            SendToClient("<= BYE\r\n");

            //clear resources
            userStatus.Connected = false;
            streamReader.Close();
            streamWriter.Close();
            networkStream.Close();
            userSocket.Close();
            Log.append("Disconnect " + userStatus.UserIPEP.Address + ":" + userStatus.UserIPEP.Port);
        }

        /*---------Interface Implementation----------*/
        //Send Message to client.
        public bool SendToClient(string message)
        {
            try
            {
                Log.append("to [" + userStatus.UserIPEP.Address + ":" + userStatus.UserIPEP.Port + "]: " + message);
                streamWriter.Write(message);
                return true;  //send success.
            }
            catch (Exception e)
            {
                Log.append("Exception: " + e.ToString());
                userStatus.Connected = false;//Exception occur, close connection.
                return false;
            }
        }

        //Receive Message from client.
        public string ReceiveFromClient()
        {
            string receiveString = null;
            try
            {
                receiveString = streamReader.ReadLine();
            }
            catch (Exception e)
            {
                Log.append("Exception: " + e.ToString());
            }
            if (receiveString != null)
            {
                return receiveString;
            }
            else
            {
                userStatus.Connected = false;//Exception occur, close connection.
                return "";
            }
        }

        /* get user status.
         * */
        public UserStatus GetStatus()
        {
            return userStatus;
        }

        /* set user status.
         * */
        public void SetStatus(UserStatus status)
        {
            userStatus = status;
        }
    }
}

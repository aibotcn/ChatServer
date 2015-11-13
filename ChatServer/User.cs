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
    class User
    {
        private bool status = true;  //connect status. true:ok, false:quit
        private string userName = "";
        private Socket userSocket = null;
        private NetworkStream networkStream = null;
        private StreamReader streamReader = null;
        private StreamWriter streamWriter = null;
        IPEndPoint useripep = null;

        public User(Socket _userSocket)
        {
            userSocket = _userSocket;
            networkStream = new NetworkStream(userSocket);
            streamReader = new System.IO.StreamReader(networkStream);
            streamWriter = new System.IO.StreamWriter(networkStream);
            streamWriter.AutoFlush = true;
            useripep = (IPEndPoint)userSocket.RemoteEndPoint;
        }

        public void Chat()
        {
            Log.append("Connect " + useripep.Address + ":" + useripep.Port);
            LoginCommand();            

            while (status)
            {
                try
                {
                    string receiveString = ReceiveFromClient();
                    if (receiveString == null)  //reach the end of stream. client shutdown.
                        QuitCommand();
                    if (receiveString == "")
                        continue;
                    //Message process
                    MessageProcessing(receiveString);
                }
                catch(Exception e)  //network exception.
                {
                    Log.append("Exception: " + e.ToString());
                    QuitCommand();
                }               
            }
        }

        public void MessageProcessing(string message)
        {
            message = message.Trim();
            Log.append("from [" + useripep.Address + ":" + useripep.Port + "]: " + message);

            string command = "";
            string content = "";

            //1. extract command and content
            if(message.StartsWith("/"))
            {
                //command mode
                if (message.IndexOf(" ") < 0)
                {
                    command = message;  //the whole sentence is one command
                }
                else
                {
                    command = message.Substring(0,  //command is in the front of sentence.
                        Math.Min(message.IndexOf(" "), message.Length));
                }
                content = message.Substring(command.Length, message.Length - command.Length);
                content = content.Trim();
            }else{
                //none command mode
                command = "";
                content = message;
            }

            //2. message process
            switch (command)
            {
                case "":
                    SpeakCommand(content);
                    break;
                case "/quit":
                    QuitCommand();
                    break;
                case "/join":
                    JoinRoomCommand(content);
                    break;
                case "/leave":
                    LeaveRoomCommand();
                    break;
                case "/create":
                    CreateRoomCommand(content);
                    break;
                case "/rooms":
                    ShowRoomCommand();
                    break;
                default:
                    WrongCommand();                   
                    break;
            }
        }

        //Receive Message from client.
        public string ReceiveFromClient()
        {
            try
            {
                return streamReader.ReadLine();
            }
            catch (Exception e)
            {
                Log.append("Exception: " + e.ToString());
            }
            return null;
        }

        //Send Message to client.
        public void SendToClient(string message)
        {
            try
            {
                Log.append("to [" + useripep.Address + ":" + useripep.Port + "]: " + message);
                streamWriter.Write(message);
            }
            catch (Exception e)
            {
                Log.append("Exception: " + e.ToString());
            }
        }

        //get user name.
        public string GetUserName()
        {
            return userName;
        }

        //Command
        //User speak. if in room, all in the room will receive user's words.
        public void SpeakCommand(string message)
        {
            //SendToClient("<= Re: " + message + "\r\n=> ");
            if (Hall.GetInstance().GetRoomFromUser(this) != Hall.nullRoomName)
            {
                message = "\r\n<= " + userName + ": " + message + "\r\n=> ";
                Hall.GetInstance().UserSendMassageToRoom(this, message);
            }
            SendToClient("=> ");
        }

        /* User login server.
         * */
        public void LoginCommand()
        {
            SendToClient("<= welcome to Yulong's chat server\r\n<= Login Name?\r\n=> ");
            while (true)
            {
                string name = streamReader.ReadLine();
                name = name.Trim();
                if (name == "")
                {
                    SendToClient("=> ");
                    continue;
                }
                name = Regex.Replace(name, "[^a-zA-Z0-9_]","");

                if (Hall.GetInstance().ContainUser(name))
                {
                    SendToClient("<= Sorry, name taken.\r\n<= Login Name?\r\n=> ");
                }
                else
                {
                    userName = name;
                    Hall.GetInstance().AddUserToHall(this);
                    SendToClient("<= Welcome " + userName + "!\r\n=> ");
                    break;
                }
            }
        }

        /* User leave room
         * */
        public void LeaveRoomCommand()
        {
            Hall.GetInstance().RemoveUserFromRoom(this);
            SendToClient("=> ");
        }

        /* User quit server.
         * */
        public void QuitCommand()
        {
            Hall.GetInstance().RemoveUserFromHall(this);
            SendToClient("<= BYE\r\n");

            //clear resources
            status = false;
            streamReader.Close();
            streamWriter.Close();
            networkStream.Close();
            userSocket.Close();
            Log.append("Disconnect " + useripep.Address + ":" + useripep.Port);
        }

        /* User join room.
         * */
        public void JoinRoomCommand(string roomName)
        {
            if (roomName == "")
            {
                SendToClient("<== room name needed.\r\n=> ");
                return;
            }

            Hall hall = Hall.GetInstance();
            
           

            if (hall.ContainRoom(roomName))
            {
                if (hall.GetRoomFromUser(this) == roomName)
                {
                    SendToClient("<= you are already in.\r\n=> ");
                    return;
                }


                hall.AddUserToRoom(this, roomName);

                List<User> userList = hall.GetUserListFromRoom(roomName);
                string reply = "<= entering room:"+roomName+"\r\n";
                foreach (var it in userList)
                {
                    string roomusername = it.GetUserName();
                    reply += "<= *";
                    reply += (roomusername==userName)?(roomusername+" (** this is you)"):(roomusername);
                    reply += "\r\n";
                }
                reply += "<= end of list.\r\n";
                SendToClient(reply);
                SendToClient("=> ");
            }
            else
            {
                SendToClient("<= room not exists.\r\n=> ");
            }
        }

        public void CreateRoomCommand(string name)
        {
            Hall hall = Hall.GetInstance();
            if (hall.ContainRoom(name))
            {
                SendToClient("<= room already exists.\r\n=> ");
            }
            else
            {
                if (hall.AddRoom(name))
                {
                    SendToClient("<= room created.\r\n=> ");
                }
                else
                {
                    SendToClient("<= room create failed.\r\n=> ");
                }

            }
        }

        /* get room list.
         * */
        public void ShowRoomCommand()
        {
            Hall hall = Hall.GetInstance();
            List<KeyValuePair<string,int>> roomList = hall.GetRoomList();
            string reply = "<= Active rooms are:\r\n";
            foreach (var it in roomList)
            {
                reply += "<= *" + it.Key + "(" + it.Value + ")\r\n";
            }
            reply += "<= end of list.\r\n=> ";
            SendToClient(reply);
        }

        /* unknown command.
         * */
        public void WrongCommand()
        {
            SendToClient("<= unknown command\r\n=> ");
        }
    }
}

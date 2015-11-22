using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Command
    {
        private IUser user;
        
        //Constructor
        public Command(IUser newUser)
        {
            user = newUser;
        }

        //Execute command
        public bool Execute(string message)
        {
            message = message.Trim();
            Log.append("from [" + user.GetStatus().UserIPEP.Address + ":" + user.GetStatus().UserIPEP.Port + "]: " + message);

            string command = "";
            string content = "";

            //1. extract command and content
            if (message.StartsWith("/"))
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
            }
            else
            {
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
                case "/room":
                case "/rooms":
                    ShowRoomCommand();
                    break;
                case "/help":
                    HelpCommand();
                    break;
                default:
                    WrongCommand();
                    break;
            }
            return true;
        }

        //Command
        //User speak. if in room, all in the room will receive user's words.
        public void SpeakCommand(string message)
        {
            //SendToClient("<= Re: " + message + "\r\n=> ");
            Hall hall = user.GetStatus().UserHall;
            if (hall.GetRoomFromUser(user) != Hall.nullRoomName)
            {
                message = "\r\n<= " + user.GetStatus().UserName + ": " + message + "\r\n=> ";
                hall.UserSendMassageToRoom(user, message);
            }
            user.SendToClient("=> ");
        }

        /* User leave room
         * */
        public void LeaveRoomCommand()
        {
            user.GetStatus().UserHall.RemoveUserFromRoom(user);
            user.SendToClient("=> ");
        }

        public void QuitCommand()
        {
            user.GetStatus().Connected = false;
        }

        /* User join room.
         * */
        public void JoinRoomCommand(string roomName)
        {
            if (roomName == "")
            {
                user.SendToClient("<== room name needed.\r\n=> ");
                return;
            }

            Hall hall = user.GetStatus().UserHall;

            if (hall.ContainRoom(roomName))
            {
                if (hall.GetRoomFromUser(user) == roomName)
                {
                    user.SendToClient("<= you are already in.\r\n=> ");
                    return;
                }

                hall.AddUserToRoom(user, roomName);

                List<IUser> userList = hall.GetUserListFromRoom(roomName);
                string reply = "<= entering room:" + roomName + "\r\n";
                foreach (var it in userList)
                {
                    string roomusername = it.GetStatus().UserName;
                    reply += "<= *";
                    reply += (roomusername == user.GetStatus().UserName) ? (roomusername + " (** this is you)") : (roomusername);
                    reply += "\r\n";
                }
                reply += "<= end of list.\r\n";
                user.SendToClient(reply);
                user.SendToClient("=> ");
            }
            else
            {
                user.SendToClient("<= room not exists.\r\n=> ");
            }
        }

        public void CreateRoomCommand(string name)
        {
            Hall hall = user.GetStatus().UserHall;
            if (hall.ContainRoom(name))
            {
                user.SendToClient("<= room already exists.\r\n=> ");
            }
            else
            {
                if (hall.AddRoom(name))
                {
                    user.SendToClient("<= room created.\r\n=> ");
                }
                else
                {
                    user.SendToClient("<= room create failed.\r\n=> ");
                }

            }
        }

        /* get room list.
         * */
        public void ShowRoomCommand()
        {
            Hall hall = user.GetStatus().UserHall;
            List<KeyValuePair<string, int>> roomList = hall.GetRoomList();
            string reply = "<= Active rooms are:\r\n";
            foreach (var it in roomList)
            {
                reply += "<= *" + it.Key + "(" + it.Value + ")\r\n";
            }
            reply += "<= end of list.\r\n=> ";
            user.SendToClient(reply);
        }

        /* unknown command.
         * */
        public void WrongCommand()
        {
            user.SendToClient("<= unknown command\r\n=> ");
        }

        /* help of chat server
         * */
        public void HelpCommand()
        {
            string content = "<= command list:\r\n"
                +"   1:/help  ---list all command supported\r\n"
                +"   2:/rooms  ---list all rooms available\r\n"
                +"   3:/create [room name]  ---create a new chat room\r\n"
                +"   4:/join [room name]  ---join the specified room\r\n"
                +"   5:/leave  ---leave the current room\r\n"
                +"   6:/quit  ---quit chat server\r\n=> ";
            user.SendToClient(content);
        }
    }
}

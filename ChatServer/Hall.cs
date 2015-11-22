using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    /* Hall is one place where user communicate with each other.
     * When user login, he enter the Hall.
     * When user join one room, he enter the room, and can speak to or hear from others in the room.
     * When user quit, he leave the Hall.
     * */
    class Hall
    {
        private static Hall instance = new Hall();
        private Dictionary<string, List<IUser>> roomMap = null;
        private Dictionary<IUser, string> userMap = null;
        public const string publicRoomName = "public";
        public const string nullRoomName = "";
        private Hall() 
        {
            userMap = new Dictionary<IUser, string>();
            roomMap = new Dictionary<string, List<IUser>>();
            roomMap.Add(publicRoomName, new List<IUser>());//public always exist
        }

        //get single instance
        public static Hall GetInstance(){           
            return instance;
        }

        /* User send message to his room. all others will receive this message.
         * */
        public void UserSendMassageToRoom(IUser user, string message)
        {
            if (userMap[user] == nullRoomName)
                return;  //user not in any room

            foreach (var it in roomMap[userMap[user]])
            {
                if (it != user)
                    it.SendToClient(message);
            }
        }

        /* Room broadcast, every one in the room will receive this message.
         * User can use RoomBroadcast when specify the user parameter.
         * */
        public void RoomBroadcast(string roomname, string message)
        {
            foreach (var it in roomMap[roomname])
            {               
                it.SendToClient(message);               
            }
        }

        /* check whether Hall contains this room.
         * */
        public bool ContainRoom(string roomName)
        {
            return roomMap.ContainsKey(roomName);
        }

        /* check whether user is already in Hall.
         * */
        public bool ContainUser(IUser user)
        {
            return userMap.ContainsKey(user);
        }

        /* check whether user is already in Hall, by name.
         * */
        public bool ContainUser(string name)
        {
            foreach (var it in userMap)
            {
                if (it.Key.GetStatus().UserName == name)
                {
                    return true;
                }
            }
            return false;
        }

        /* Add room to Hall.
         * */
        public bool AddRoom(string name)
        {
            if (ContainRoom(name))
                return false;
            
            roomMap.Add(name, new List<IUser>());
            return true;
        }

        /* Remove room from Hall.
         * */
        public void RemoveRoom(string name)
        {
            if (ContainRoom(name))            
                roomMap.Remove(name);    
        }


        /* Add User to specified room.
         * If room is not specified, add to 'public' room.
         * If user in other room, switch to specified room.
         * */
        public bool AddUserToRoom(IUser user, string roomName = publicRoomName)
        {
            if (!ContainRoom(roomName))
                return false;  //room not exist

            if (!ContainUser(user))
                return false;  //user not exist

            if(userMap[user] == roomName)
                return true;  //stay in room.

            if (userMap[user] != nullRoomName)  //if user already in one room, leave firstly.            
                RemoveUserFromRoom(user);
                
            userMap[user] = roomName;                              
            roomMap[roomName].Add(user);               
            //Notify others.
            UserSendMassageToRoom(user, "\r\n<= *new user joined chat: "+user.GetStatus().UserName+"\r\n=> ");
            return true;
        }

        /* Remove user from room. when last user leave, room will be removed too
         * 'public' room always exist.
         * */
        public void RemoveUserFromRoom(IUser user)
        {
            if (userMap[user] == nullRoomName)  //user not in any room.
                return;

            string roomname = userMap[user];
            roomMap[roomname].Remove(user);
            userMap[user] = nullRoomName;

            RoomBroadcast(roomname, "\r\n<= *user has left chat: "+user.GetStatus().UserName+"\r\n=> ");
            user.SendToClient("\r\n<= *user has left chat: " + user.GetStatus().UserName + " (**this is you)\r\n");

            //when last user leave
            if (roomMap[roomname].Count == 0 && roomname != publicRoomName)
            {
                RemoveRoom(roomname);
            }
        }

        /* User login server.
         * */
        public void AddUserToHall(IUser user)
        {
            if (ContainUser(user))
                throw new Exception("duplicate user checked");
            userMap.Add(user, nullRoomName);
        }

        /* User quit from server.
         * */
        public void RemoveUserFromHall(IUser user)
        {
            if (!userMap.ContainsKey(user))
                return;
            //1.leave room if user is in room.
            if(userMap[user] != nullRoomName)
                RemoveUserFromRoom(user);
            //2.quit server
            userMap.Remove(user);
        }

        /* Get room list, including the number of people in rooms.
         * */
        public List<KeyValuePair<string,int>> GetRoomList()
        {
            List<KeyValuePair<string, int>> roomList = new List<KeyValuePair<string, int>>();
            foreach(var it in roomMap){
                roomList.Add(new KeyValuePair<string,int>(it.Key, it.Value.Count));
            }
            return roomList;
        }

        /* Get user list in the specified room.
         * */
        public List<IUser> GetUserListFromRoom(string name)
        {
            if (ContainRoom(name))
            {
                return roomMap[name];
            }
            else
            {
                return new List<IUser>();
            }
        }

        /* Get user's room.
         * */
        public string GetRoomFromUser(IUser user)
        {
            if (ContainUser(user))  //if user exist
                return userMap[user];
            return nullRoomName;
        }
    }
}

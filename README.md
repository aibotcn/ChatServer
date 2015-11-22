[Introduce]

ChatServer is a multiple users online chat system.
Developed by Yulong Qiu.
All rights reserved.

[User guide]

1. connect server: "telnet 131.94.129.32 12345".
2. set your user name.
3. use "/rooms" to check all chat rooms available.
4. use "/create myroom" to create a chat room named "myroom".
5. use "/join myroom" to join the chat room.
6. enter anything to talk with others in the same chat room.
7. use "/leave" to leave the chat room.
8. use "/quit" to quit chat server.
9. use "/help" to obtain supported command list.

[Architecture]

Client:
1.telnet terminal.

Server:

1.User:IUser:
  1)when server monitor one client connectin reached, allocate one thread to the user. 'User' object is used to maintain the connection and communication with clent in one to one mode.
  2)User can have many types, like Socket User, HTTP User, e.g. Just need to obey the same interface.
  
2.Hall:
  1)when user login server, Hall maintain all the user list and all the users' activity status, e.g. talk with others. Hall works in one to many mode.
  
3.Log:
  1)Log user activity and server activity to log files.
  
4.Command:
  1)supported command operations. different user type can have different command strategy and command set.
  
5.UserStatus:
  1)maintain the configuration of online user.

[Update history]

[2015-11-11]
start to develop project.

[2015-11-13]
ChatServer v0.1
support multiple users chat in real-time.
support command "help","login","rooms","create","join","leave","quit".

[2015-11-22]
# ChatServer v0.2
redesign architecture to make it easier to scale with more features.

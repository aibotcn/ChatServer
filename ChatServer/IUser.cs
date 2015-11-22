using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    /* User interface. User can have different implementation while obey this interface.
     * */
    interface IUser
    {
        bool SendToClient(string message);
        string ReceiveFromClient();
        UserStatus GetStatus();
        void SetStatus(UserStatus status);
    }
}

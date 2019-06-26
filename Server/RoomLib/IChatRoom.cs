using Client.UserLib;

namespace Server.RoomLib
{
    public interface IChatRoom
    {
        bool IsFull { get; set; }

        void AddUser(User user);

        void RemoveUser(User user);
    }
}

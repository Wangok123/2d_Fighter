using GameProtocol;
using UnityCore.Entities.Core;

namespace UnityCore.Entities.User
{
    public class UserDataComponent : Component
    {
        public ulong UserId { get; }
        public string Username { get; }
        public int Level { get; }
        public uint Exp { get; }
        
        public UserDataComponent(UserDto data)
        {
            UserId = data.UserId;
            Username = data.Username;
            Level = data.Level;
            Exp = data.Exp;
        }
    }
}
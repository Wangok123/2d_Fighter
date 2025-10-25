using GameProtocol;
using UnityCore.Base;
using UnityCore.Entities.User;
using UnityCore.EventDefine;
using UnityCore.Network.Dispatcher;

namespace UnityCore.Network.Handler
{
    public class LoginHandler : MessageHandler<LoginResponse>
    {
        public LoginHandler(uint cmdId) : base(cmdId)
        {
        }

        protected override void Process(LoginResponse message)
        {
            var userData = Game.World.GetSingletonEntity<UserDataEntity>();
            UserDataComponent component = new UserDataComponent(message.UserData);
            userData.AddComponent(component);

            HeroDataComponent heroSelectDataComponent = new HeroDataComponent();
            heroSelectDataComponent.HeroSelectDataList.Clear();
            foreach (var selectData in message.HeroData)
            {
                heroSelectDataComponent.HeroSelectDataList.Add(selectData.HeroId);
            }
            userData.AddComponent(heroSelectDataComponent);

            ResponseEventDefine.LoginSuccessArgs.SendEventMessage();
        }
    }
}
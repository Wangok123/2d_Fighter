using GameProtocol;
using LatProtocol;
using LatServer.Core.Common;
using LatServer.Core.Service.ConfigService;
using LatServer.Core.System.EventSys;
using LatServer.Core.System.EventSys.Events;
using LogLib;

namespace LatServer.Core.System.LoginSys;

public class LoginSystem : SystemRoot<LoginSystem>
{
    public override void Init()
    {
        base.Init();
        BindEvent();
        GameDebug.Log("LoginSystem Init Done!!!!!");
    }

    private void BindEvent()
    {
        EventManager.Subscribe<LoginEvent>(OnLoginEvent);
    }
    
    public void UnBindEvent()
    {
        EventManager.Unsubscribe<LoginEvent>(OnLoginEvent);
    }
    
    private void OnLoginEvent(LoginEvent loginEvent)
    {
        LoginResponse loginResponse = new LoginResponse();
        if (CacheService.IsAcctOnline(loginEvent.Account, out var session))
        {
            // 账号在线
            loginResponse.Code = (int)ErrorCodeID.AccountIsOnline;
        }else
        {
            session = loginEvent.Session;
            uint sessionId = session.GetSessionID();
            // 账号不在线, 创建默认账号
            UserDto userData = new UserDto
            {
                UserId = sessionId,
                Username = $"龙傲天_{sessionId}",
                Level = 999,
                Exp = 999999,
            };

            loginResponse.UserData = userData;
            
            // 创建默认英雄数据
            var allHero = CfgService.Instance.Tables.TbUnit;
            for (int i = 0; i < allHero.DataList.Count; i++)
            {
                var heroDto = new HeroDto
                {
                    HeroId = allHero.DataList[i].Id,
                };
                loginResponse.HeroData.Add(heroDto);
            }
        }
        
        session.SendMsg((ushort)ProtocolID.LoginResponse, loginResponse);
    }
}
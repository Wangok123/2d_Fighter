using GameProtocol;
using LatProtocol;
using LatServer.Core.Common;
using LatServer.Core.Service.NetService;
using LatServer.Core.System.EventSys;
using LatServer.Core.System.EventSys.Events;
using LatServer.Core.System.RoomSys;
using LogLib;
using MatchType = GameProtocol.MatchType;

namespace LatServer.Core.System.MatchSys;

public class MatchSystem : SystemRoot<MatchSystem>
{
    private Queue<ServerSession> _queue1v1;

    public override void Init()
    {
        _queue1v1 = new Queue<ServerSession>();
        BindEvent();
        GameDebug.Log("MatchSystem Init Done!!!!!");
    }

    public override void Update()
    {
        while (_queue1v1.Count >= 2)
        {
            ServerSession[] sessionArray = new ServerSession[2];
            for (int i =0; i < 2; i++)
            {
                sessionArray[i] = _queue1v1.Dequeue();
            }
            
            RoomSystem.Instance.AddPvpRoom(sessionArray, MatchType._1Vs1);
        }
    }

    private void BindEvent()
    {
        EventManager.Subscribe<MatchArgs>(OnMatchEvent);
    }
    
    public void UnBindEvent()
    {
        EventManager.Unsubscribe<MatchArgs>(OnMatchEvent);
    }
    
    private void OnMatchEvent(MatchArgs matchArgs)
    {
        var matchType = matchArgs.MatchType;
        switch (matchType)
        {
            case MatchType._1Vs1:
                _queue1v1.Enqueue(matchArgs.Session);
                break;
            default:
                GameDebug.LogError($"MatchSystem: Unknow MatchType {matchType}");
                break;
        }
        
        MatchResponse response = new MatchResponse
        {
            PredictionTime = 5,
        };
        
        matchArgs.Session.SendMsg((ushort)ProtocolID.MatchResponse, response);
    }
}
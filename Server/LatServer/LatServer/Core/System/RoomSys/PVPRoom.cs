using GameProtocol;
using LatProtocol;
using LatServer.Core.Common;
using LatServer.Core.Service.NetService;
using LatServer.Core.System.RoomSys.Fsm;
using LatServer.Core.Tools.StateMachine;
using LogLib;
using MatchType = GameProtocol.MatchType;

namespace LatServer.Core.System.RoomSys;

public class PVPRoom
{
    public uint RoomId;
    public MatchType MatchType;
    public ServerSession[] SessionArray;
    
    private SelectDto[] _selectArray;
    public SelectDto[] SelectArray
    {
        get => _selectArray;
        set => _selectArray = value;
    }
    
    private StateMachine _machine;
    
    public PVPRoom(uint roomId, MatchType matchType, ServerSession[] sessionArray)
    {
        RoomId = roomId;
        MatchType = matchType;
        SessionArray = sessionArray;
        
        _machine = new StateMachine(this);
        _machine.SetBlackboardValue("RoomData", this);
        _machine.AddNode<RoomStateConfirm>();
        _machine.AddNode<RoomStateEnd>();
        _machine.AddNode<RoomStateFighting>();
        _machine.AddNode<RoomStateLoading>();
        _machine.AddNode<RoomStateSelecting>();
        
        _machine.Run<RoomStateConfirm>();
    }
    
    public void BroadcastToNtfConfirm(ConfirmNotification msg)
    {
        foreach (var session in SessionArray)
        {
            session.SendMsg((ushort)ProtocolID.ConfirmNotification, msg);
        }
    }
    
    public void BroadcastToNtfSelect(SelectNotification msg)
    {
        foreach (var session in SessionArray)
        {
            session.SendMsg((ushort)ProtocolID.SelectNotification, msg);
        }
    }
    
    public void BroadcastLoadProgress(LoadProgressNotification ntf)
    {
        foreach (var session in SessionArray)
        {
            session.SendMsg((ushort)ProtocolID.LoadProgressNotification, ntf);
        }
    }
    
    public void BroadcastLoadFinish(LoadingFinishNotification ntf)
    {
        foreach (var session in SessionArray)
        {
            session.SendMsg((ushort)ProtocolID.LoadingFinishNotification, ntf);
        }
    }
    
    private int GetPosIndex(ServerSession session)
    {
        int posIndex = -1;
        for (int i = 0; i < SessionArray.Length; i++)
        {
            if (Equals(SessionArray[i], session))
            {
                posIndex = i;
                break;
            }
        }

        return posIndex;
    }

    public void SendConfirm(ServerSession session)
    {
        var node = _machine.CurrentNode;
        if (node is RoomStateConfirm state)
        {
            var index = GetPosIndex(session);
            state.UpdateConfirmState(index);
        }
        else
        {
            GameDebug.LogError($"PVPRoom: SendConfirm: State not confirm {RoomId}");
        }
    }
    
    public void SendSelect(ServerSession session, int heroId)
    {
        var node = _machine.CurrentNode;
        if (node is RoomStateSelecting state)
        {
            var index = GetPosIndex(session);
            state.UpdateHeroSelect(index, heroId);
        }
        else
        {
            GameDebug.LogError($"PVPRoom: SendSelect: State not selecting {RoomId}");
        }
    }
    
    public void ChangeState<T>() where T : IStateNode
    {
        _machine.ChangeState<T>();
    }

    public void SendLoadProgress(ServerSession session, int percent)
    {
        var node = _machine.CurrentNode;
        if (node is RoomStateLoading state)
        {
            var index = GetPosIndex(session);
            state.UpdateLoadProgress(index, percent);
        }
        else
        {
            GameDebug.LogError($"PVPRoom: SendLoadProgress: State not loading {RoomId}");
        }
    }
    
    public void SendLoadComplete(ServerSession session)
    {
        var node = _machine.CurrentNode;
        if (node is RoomStateLoading state)
        {
            var index = GetPosIndex(session);
            state.UpdateLoadComplete(index);
        }
        else
        {
            GameDebug.LogError($"PVPRoom: SendLoadComplete: State not loading {RoomId}");
        }
    }
}
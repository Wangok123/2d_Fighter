using LatServer.Core.Common;
using LatServer.Core.Service.NetService;
using LatServer.Core.System.EventSys;
using LatServer.Core.System.EventSys.Events;
using LogLib;
using MatchType = GameProtocol.MatchType;

namespace LatServer.Core.System.RoomSys;

public class RoomSystem : SystemRoot<RoomSystem>
{
    List<PVPRoom> _pvpRoomList;
    Dictionary<uint, PVPRoom> _pvpRoomDict;

    public override void Init()
    {
        _pvpRoomList = new List<PVPRoom>();
        _pvpRoomDict = new Dictionary<uint, PVPRoom>();

        BindEvent();

        GameDebug.Log("RoomSystem Init Done!!!!!");
    }

    private void BindEvent()
    {
        EventManager.Subscribe<SendConfirmArgs>(OnSendConfirmEvent);
        EventManager.Subscribe<SendSelectArgs>(OnSendSelectEvent);
        EventManager.Subscribe<SendLoadProgressArgs>(OnSendLoadProgress);
        EventManager.Subscribe<SendLoadFinishArgs>(OnSendLoadFinish);
    }

    public void AddPvpRoom(ServerSession[] sessions, MatchType matchType)
    {
        uint roomId = GetRoomId();
        PVPRoom pvpRoom = new PVPRoom(roomId, matchType, sessions);
        _pvpRoomList.Add(pvpRoom);
        _pvpRoomDict.Add(roomId, pvpRoom);

        GameDebug.Log($"AddPVPRoom: RoomId {roomId}, MatchType {matchType}");
    }

    public override void Update()
    {
        base.Update();
    }

    uint _roomId; // 从1开始计数， 0是测试用房间

    public uint GetRoomId()
    {
        return ++_roomId;
    }


    private void OnSendConfirmEvent(SendConfirmArgs sendConfirmArgs)
    {
        uint roomId = sendConfirmArgs.RoomId;
        ServerSession session = sendConfirmArgs.Session;
        if (_pvpRoomDict.TryGetValue(roomId, out var pvpRoom))
        {
            pvpRoom.SendConfirm(session);
        }
        else
        {
            GameDebug.LogError($"RoomSystem: RoomId {roomId} not found");
        }
    }
    
    private void OnSendSelectEvent(SendSelectArgs sendSelectArgs)
    {
        uint roomId = sendSelectArgs.RoomId;
        int heroId = sendSelectArgs.HeroId;
        ServerSession session = sendSelectArgs.Session;

        if (_pvpRoomDict.TryGetValue(roomId, out var pvpRoom))
        {
            pvpRoom.SendSelect(session, heroId);
        }
        else
        {
            GameDebug.LogError($"RoomSystem: RoomId {roomId} not found");
        }
    }

    private void OnSendLoadProgress(SendLoadProgressArgs sendLoadProgressArgs)
    {
        uint roomId = sendLoadProgressArgs.RoomId;
        int percent = sendLoadProgressArgs.Percent;
        ServerSession session = sendLoadProgressArgs.Session;

        if (_pvpRoomDict.TryGetValue(roomId, out var pvpRoom))
        {
            pvpRoom.SendLoadProgress(session, percent);
        }
        else
        {
            GameDebug.LogError($"RoomSystem: RoomId {roomId} not found");
        }
    }

    private void OnSendLoadFinish(SendLoadFinishArgs sendLoadFinishArgs)
    {
        uint roomId = sendLoadFinishArgs.RoomId;
        ServerSession session = sendLoadFinishArgs.Session;
        
        if (_pvpRoomDict.TryGetValue(roomId, out var pvpRoom))
        {
            pvpRoom.SendLoadComplete(session);
        }
        else
        {
            GameDebug.LogError($"RoomSystem: RoomId {roomId} not found");
        }
    }
}
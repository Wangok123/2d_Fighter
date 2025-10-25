using GameProtocol;
using LatProtocol;
using LatServer.Core.Common;
using LatServer.Core.Service.CacheService;
using LatServer.Core.Tools.StateMachine;

namespace LatServer.Core.System.RoomSys.Fsm;

public class RoomStateLoading : IStateNode
{
    private PVPRoom _roomData;
    private int[] _percentArray;
    private bool[] _loadingCompleteArray;
    private int _ntfCount;

    public void OnCreate(StateMachine machine)
    {
        _roomData = (PVPRoom)machine.GetBlackboardValue("RoomData");
    }

    public void OnEnter()
    {
        int length = _roomData.SessionArray.Length;
        _percentArray = new int[length];
        _loadingCompleteArray = new bool[length];

        LoadResourceNotification ntf = new LoadResourceNotification()
        {
            MapId = 101
        };

        for (int i = 0; i < length; i++)
        {
            SelectDto selectData = _roomData.SelectArray[i];
            BattleHeroDto heroData = new BattleHeroDto()
            {
                HeroId = selectData.SelectID,
                UserName = GetDefaultUserName(i),
            };

            ntf.HeroDataList.Add(heroData);
        }

        for (int i = 0; i < length; i++)
        {
            ntf.PosIndex = i;
            var session = _roomData.SessionArray[i];
            session.SendMsg((ushort)ProtocolID.LoadResourceNotification, ntf);
        }
    }

    public void OnExit()
    {
    }

    public void OnUpdate()
    {
    }

    private string GetDefaultUserName(int posIndex)
    {
        var userData = CacheService.Instance.GetUserDataBySession(_roomData.SessionArray[posIndex]);
        if (userData == null)
        {
            return "DefaultHero"; // Replace with actual default hero ID
        }

        return userData.Username; // Assuming UserData has a DefaultHeroId property
    }

    public void UpdateLoadProgress(int index, int percent)
    {
        _percentArray[index] = percent;
        ++_ntfCount;
        LoadProgressNotification ntf = new LoadProgressNotification();
        ntf.PercentList.AddRange(_percentArray);
        
        _roomData.BroadcastLoadProgress(ntf);
    }

    public void UpdateLoadComplete(int index)
    {
        _loadingCompleteArray[index] = true;
        foreach (var complete in _loadingCompleteArray)
        {
            if (!complete)
            {
                return;
            }
        }
        
        LoadingFinishNotification ntf = new LoadingFinishNotification();
        _roomData.BroadcastLoadFinish(ntf);
    }
}
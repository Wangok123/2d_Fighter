using GameProtocol;
using LatProtocol;
using LatServer.Core.Service.TimerService;
using LatServer.Core.Tools.StateMachine;
using LogLib;

namespace LatServer.Core.System.RoomSys.Fsm;

public class RoomStateSelecting : IStateNode
{
    private SelectDto[] _selectArray;
    private PVPRoom _roomData;
    
    private int _checkTaskId = -1;
    private bool _isAllSelectDone = false;
    
    public void OnCreate(StateMachine machine)
    {
        _roomData = (PVPRoom)machine.GetBlackboardValue("RoomData");
    }

    public void OnEnter()
    {
        int length = _roomData.SessionArray.Length;
        _selectArray = new SelectDto[length];
        for (int i = 0; i < length; i++)
        {
            var data = new SelectDto();
            data.SelectID = 0;
            data.SelectDone = false;
            _selectArray[i] = data;
        }

        SelectNotification ntf = new SelectNotification();

        _roomData.BroadcastToNtfSelect(ntf);

        _checkTaskId = TimerService.Instance.AddTask(ServerConfig.SelectionTime * 1000 + 2000, ReachTimeLimit);
    }

    private void ReachTimeLimit(int tid)
    {
        if (_isAllSelectDone)
        {
            return;
        }
        
        GameDebug.LogColor(LogColorType.Red, $"RoomID: {_roomData.RoomId} Select Time Out" +
                                             $" SelectCount: {_selectArray.Length}");
        for (int i = 0; i < _selectArray.Length; i++)
        {
            if (!_selectArray[i].SelectDone)
            {
                _selectArray[i].SelectID = GetDefaultHeroId();
                _selectArray[i].SelectDone = true;
            }
        }
    }
    
    private int GetDefaultHeroId()
    {
        return 1001;
    }
    
    private void CheckAllSelectDone()
    {
        if (_isAllSelectDone)
        {
            return;
        }

        _isAllSelectDone = true;
        foreach (var selectData in _selectArray)
        {
            if (!selectData.SelectDone)
            {
                _isAllSelectDone = false;
                break;
            }
        }
    }

    public void UpdateHeroSelect(int posIndex, int heroId)
    {
        _selectArray[posIndex].SelectID = heroId;
        _selectArray[posIndex].SelectDone = true;
        CheckAllSelectDone();
        if (_isAllSelectDone)
        {
            if (TimerService.Instance.DeleteTask(_checkTaskId))
            {
                GameDebug.LogColor(LogColorType.Green, $"RoomID: {_roomData.RoomId} 所有玩家已选择英雄");
            }
            else
            {
                GameDebug.LogColor(LogColorType.Red, $"RoomID: {_roomData.RoomId} 删除选择英雄定时器失败");
            }
            
            _roomData.SelectArray = _selectArray;
            _roomData.ChangeState<RoomStateLoading>();
        }
    }

    public void OnExit()
    {
        if (_checkTaskId != -1)
        {
            TimerService.Instance.DeleteTask(_checkTaskId);
            _checkTaskId = -1;
        }
        
        _selectArray = null;
        _isAllSelectDone = false;
    }

    public void OnUpdate()
    {
    }
}
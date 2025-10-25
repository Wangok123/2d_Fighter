using GameProtocol;
using LatProtocol;
using LatServer.Core.Service.TimerService;
using LatServer.Core.Tools.StateMachine;
using LogLib;

namespace LatServer.Core.System.RoomSys.Fsm;

public class RoomStateConfirm : IStateNode
{
    private StateMachine _machine;
    private ConfirmDto[] _confirmArray;
    private PVPRoom _roomData;

    private int _checkTaskId = -1;
    private bool _isAllConfrimDone = false;

    public void OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    public void OnEnter()
    {
        _roomData = (PVPRoom)_machine.GetBlackboardValue("RoomData");
        int length = _roomData.SessionArray.Length;
        _confirmArray = new ConfirmDto[length];
        for (int i = 0; i < length; i++)
        {
            var data = new ConfirmDto();
            data.IconIndex = (uint)i;
            data.ConfirmDone = false;
            _confirmArray[i] = data;
        }

        ConfirmNotification ntf = new ConfirmNotification();
        ntf.RoomId = _roomData.RoomId;
        foreach (var confirmData in _confirmArray)
        {
            ntf.ConfirmData.Add(confirmData);
        }

        ntf.Dismiss = false;

        _roomData.BroadcastToNtfConfirm(ntf);

        _checkTaskId = TimerService.Instance.AddTask(ServerConfig.ConfirmationTime * 1000, ReachTimeLimit);
    }

    private void ReachTimeLimit(int id)
    {
        if (_isAllConfrimDone)
        {
            return;
        }
        
        GameDebug.LogColor(LogColorType.Red, $"RoomID: {_roomData.RoomId} Confirm Time Out" +
                                             $" ConfirmCount: {_confirmArray.Length}");
        ConfirmNotification ntf = new ConfirmNotification();
        ntf.Dismiss = true;
        
        _roomData.BroadcastToNtfConfirm(ntf);
        _machine.ChangeState<RoomStateEnd>();
    }

    private void CheckAllConfirmDone()
    {
        foreach (var confirmData in _confirmArray)
        {
            if (!confirmData.ConfirmDone)
            {
                return;
            }
        }

        _isAllConfrimDone = true;
    }

    public void UpdateConfirmState(int posIndex)
    {
        _confirmArray[posIndex].ConfirmDone = true;
        CheckAllConfirmDone();
        if (_isAllConfrimDone)
        {
            if (TimerService.Instance.DeleteTask(_checkTaskId))
            {
                GameDebug.LogColor(LogColorType.Green, $"RoomID: {_roomData.RoomId} Confirm Done" +
                                                       $" ConfirmCount: {_confirmArray.Length}");
            }
            else
            {
                GameDebug.LogError($"Remove Task Failed, TaskId: {_checkTaskId}");
            }

            _machine.ChangeState<RoomStateSelecting>();
        }else
        {
            ConfirmNotification ntf = new ConfirmNotification();
            ntf.RoomId = _roomData.RoomId;
            
            foreach (var confirmData in _confirmArray)
            {
                ntf.ConfirmData.Add(confirmData);
            }

            _roomData.BroadcastToNtfConfirm(ntf);
        }
    }

    public void OnExit()
    {
        if (_checkTaskId != -1)
        {
            TimerService.Instance.DeleteTask(_checkTaskId);
            _checkTaskId = -1;
        }
        
        _confirmArray = null;
        _roomData = null;
        _isAllConfrimDone = false;
    }
}
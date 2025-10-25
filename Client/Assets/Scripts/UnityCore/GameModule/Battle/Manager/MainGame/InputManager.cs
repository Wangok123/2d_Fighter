using LATMath;
using UnityCore.Base;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.MainGame
{
    public class InputManager
    {
        
        private MainGameManager _mainGameManager;
        private MainGameManager MainGameManager
        {
            get
            {
                if (_mainGameManager == null)
                {
                    _mainGameManager = GameModuleManager.GetModule<MainGameManager>();
                }
                return _mainGameManager;
            }
        }
        
        private Vector2 _lastMoveDirection;
        public void OnMove(Vector2 dir)
        {
            if (dir != _lastMoveDirection)
            {
                InputMoveKey(dir);
                _lastMoveDirection = dir;
            }
        }

        private Vector2 _lastStickDir = Vector2.zero;
        
        private void InputMoveKey(Vector2 dir)
        {
            if (dir == _lastStickDir)
            {
                return;
            }
            
            Vector3 dirVector3 = new Vector3(dir.x, 0, dir.y);
            dirVector3 = Quaternion.Euler(0, 45, 0) * dirVector3; // Adjust for camera angle if needed1
            
            LATVector3 logicDir = LATVector3.Zero;
            if (dir != Vector2.zero)
            {
                logicDir.x = (LATInt)dirVector3.x;
                logicDir.y = (LATInt)dirVector3.y;
                logicDir.z = (LATInt)dirVector3.z;
            }
            
            bool isSend = MainGameManager.SendMoveKey(logicDir);
            if (isSend)
            {
                _lastStickDir = dir;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using LATLog;
using LATNet;
using LatProtocol;
using UnityCore.Base;

namespace UnityCore.Network
{
    public class NetworkComponent : LatComponent
    {
        /// <summary>
        /// 是否使用公网
        /// </summary>
        public bool IsPublicNetwork { get; set; }

        public bool Connected { get; set; }

        public static readonly string PkgqueLock = "pkgque_lock";
    
        private KCPNet<ClientSession> _client;
        private Queue<MsgPack> _msgPackQueue;
        private Task<bool> _checkTask;

        protected override void Awake()
        {
            base.Awake();
            
            MessageHandlerService.Instance.Init();
            
            IsInit = true;
        }
        
        public void Init()
        {
            _client = new KCPNet<ClientSession>();
            _msgPackQueue = new Queue<MsgPack>(); 
        
            KCPTool.LogFunc = GameDebug.Log;
            KCPTool.ErrorFunc = GameDebug.LogError;
            KCPTool.WarnFunc = GameDebug.LogWarning;
            KCPTool.ColorLogFunc = (color, s) =>
            {
                GameDebug.LogColor((LogColorType)color, s);
            };

            string srvIP = ServerConfig.LocalDevInnerIp;
            if (IsPublicNetwork)
            {
                
            }
        
            _client.StartAsClient("172.16.1.31", ServerConfig.UdpPort);
            _checkTask = _client.ConnectServer(100);
            GameDebug.Log("NetService Init Done!!!!");
        }
        
        private int _counter = 0;
        public void Update()
        {
            if (_checkTask != null && _checkTask.IsCompleted)
            {
                if (_checkTask.Result)
                {
                    Game.UI.OpenTipsUI("连接服务器成功");
                    GameDebug.Log("Connect Server Success");
                    _checkTask = null;
                    
                    // 连接成功后,发送ping包
                }
                else
                {
                    _counter++;
                    if (_counter > 4)
                    {
                        Game.UI.OpenTipsUI("连接服务器失败");
                        GameDebug.LogError($"Connect Server Failed, Times{_counter}, please check your network");
                        _checkTask = null;
                    }
                    else
                    {
                        GameDebug.Log($"Connect Server Failed, Times{_counter},retry...");
                        _checkTask = _client.ConnectServer(100);
                    }
                }
            }
            
            if (_client != null && _client.ClientSession != null)
            {
                if (_msgPackQueue.Count > 0)
                {
                    lock (PkgqueLock)
                    {
                        MsgPack msg = _msgPackQueue.Dequeue();
                        HandOutMsg(msg);
                    }
                }
            }
        }
    
        public void AddMsgQueue(ushort cmdId, byte[] msg)
        {
            lock (PkgqueLock)
            {
                _msgPackQueue.Enqueue(new MsgPack(cmdId, msg));
            }
        }

        /// <summary>
        /// 分发消息
        /// </summary>
        private void HandOutMsg(MsgPack msg)
        {
            MessageHandlerService.Instance.Dispatch(msg);
        }
        
        public void SendMsg(ushort cmdId, IMessage msg)
        {
            // 发送之前使用 ProtocolProcessor进行处理
            var buffer = ProtocolProcessor.SendMessage(cmdId, msg);
            _client.ClientSession.SendMsg(buffer);
        }

        public static void PrintBytes(string tag, byte[] bytes, int startIndex=0, int endIndex=-1)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(tag);
            sb.Append("] ");
            sb.AppendFormat("len:{0} >>> ", bytes.Length);
            if (endIndex == -1)
            {
                endIndex = bytes.Length;
            }

            for (var i = 0; i < bytes.Length; i++)
            {
                if (i < startIndex)
                    continue;
                if (i > endIndex)
                    break;
                sb.Append(bytes[i]);
                sb.Append(",");
            }

            GameDebug.Log(sb.ToString());
        }
    }
}
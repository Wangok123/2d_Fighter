using LatCfg;
using LatServer.Core.Common;
using Luban;

namespace LatServer.Core.Service.ConfigService;

public class CfgService : Singleton<CfgService>
{
    private Tables _tables;
    public Tables Tables => _tables;
    
    public override void Init()
    {
        _tables = new Tables(file =>
            new ByteBuf(File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "GenerateDatas/bytes/" + file + ".bytes")));
    }
}
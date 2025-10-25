using System.IO;
using YooAsset.Editor;

namespace MyProject.Editor.YooAsset
{
    public class GroupAndFileLowerRules : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            string result = $"{data.GroupName}_{fileName}";
            result = result.ToLower();
            
            return result;
        }
    }
}
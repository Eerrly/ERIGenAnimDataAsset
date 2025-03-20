using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationFBXFileHandler
{
    [UnityEditor.MenuItem("Tools/处理选中的FBX")]
    public static void HandleSelected()
    {
        var guids = Selection.assetGUIDs;
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log("通过GUID获取的路径: " + assetPath);
            if (assetPath.EndsWith(".FBX") || assetPath.EndsWith(".fbx"))
            {
                var destDir = Path.GetDirectoryName(assetPath);
                AnimationUtil.ImportAnimationDataAndClip(assetPath, destDir);
            }
        }
    }
}
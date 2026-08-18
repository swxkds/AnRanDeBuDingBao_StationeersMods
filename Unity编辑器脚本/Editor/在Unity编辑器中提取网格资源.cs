using UnityEngine;
using UnityEditor;

public class 在Unity编辑器中提取网格资源
{
    [MenuItem("Tools/在Unity编辑器中提取网格资源")]
    static void SaveSelectedMesh()
    {
        var 当前焦点选择 = Selection.activeGameObject as GameObject;
        if (当前焦点选择 == null)
        {
            Debug.LogWarning("请先选择一个包含MeshFilter组件的游戏对象");
            return;
        }

        var 路径 = AssetDatabase.GetAssetPath(当前焦点选择);
        var 目录 = System.IO.Path.GetDirectoryName(路径);

        // 如果选中的是个文件夹，就直接用它
        if (AssetDatabase.IsValidFolder(路径))
        {
            目录 = 路径;
        }

        // 2. 获取MeshFilter组件
        foreach (var 当前 in 当前焦点选择.GetComponentsInChildren<MeshFilter>(true))
        {
            var 网格 = 当前.sharedMesh;
            if (网格 == null)
            {
                Debug.LogWarning("MeshFilter中的Mesh为空。");
                continue;
            }
            else
            {
                // 【重要】创建独立副本（解决 FBX 只读问题）
                var 新网格 = Object.Instantiate(网格);
                新网格.name = 网格.name;

                var path = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(目录, 新网格.name + "网格.asset"));
                AssetDatabase.CreateAsset(新网格, path);
                Debug.Log("Mesh已成功提取并保存到: " + path);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
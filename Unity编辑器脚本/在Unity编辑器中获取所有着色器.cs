using UnityEngine;
using UnityEditor;

public class 在Unity编辑器中获取所有着色器 : EditorWindow
{
    [MenuItem("Tools/List All Shaders")]
    static void Init()
    {
        // 获取所有可用着色器的信息数组
        var shaderInfos = ShaderUtil.GetAllShaderInfo();

        Debug.Log($"总共找到 {shaderInfos.Length} 个着色器");

        foreach (var info in shaderInfos)
        {
            // 输出每个着色器的名称
            Debug.Log($"着色器名称: {info.name}");

            // 也可以通过 Shader.Find 来获取实际的 Shader 对象
            // Shader shader = Shader.Find(info.name);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;

[ExecuteInEditMode]
public class 在Unity编辑器中创建网格合并的预制体 : MonoBehaviour
{
    public static GameObject 休眠的根节点 { get; private set; }
    public static Material 测试用实体材质 { get; private set; }
    public static Material 测试用蓝图材质 { get; private set; }
    public Mesh 多边形网格与材质_已合并Mesh;
    public Material[] 多边形网格与材质_所有subMesh材质;
    public static Texture2DArray UV纹理数组;
    public static GameObject 实体预制体 { get; private set; }
    public static GameObject 蓝图预制体 { get; private set; }
    public string 物体名称 = "矿物扫描眼镜";
    public void Awake()
    {
        if (休眠的根节点 != null && 测试用实体材质 != null && 测试用蓝图材质 != null && 多边形网格与材质_已合并Mesh != null && 多边形网格与材质_所有subMesh材质 != null && 实体预制体 != null && 蓝图预制体 != null)
        {
            var 根节点 = new GameObject();
            var 实体 = UnityEngine.Object.Instantiate(实体预制体, 根节点.transform, false);
            var 蓝图 = UnityEngine.Object.Instantiate(蓝图预制体, 根节点.transform, false);
            return;
        }

        if (休眠的根节点 == null)
        {
            休眠的根节点 = new GameObject("休眠的根节点")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            休眠的根节点.SetActive(false);
        }

        if (测试用实体材质 == null)
        {
            var shader = Shader.Find("Legacy Shaders/Diffuse Fast");
            if (shader == null)
            {
                Debug.Log("提供的路径处找不到实体shader");
                return;
            }
            测试用实体材质 = new Material(shader)
            {
                mainTexture = UV纹理数组
            };
        }

        if (测试用蓝图材质 == null)
        {
            var shader = Shader.Find("VR/SpatialMapping/Wireframe");
            if (shader == null)
            {
                Debug.Log("提供的路径处找不到蓝图shader");
                return;
            }
            测试用蓝图材质 = new Material(shader);
        }

        {
            var Unity项目Assets目录 = Application.dataPath;

            var AssetBundle字节序列文件完整路径 = Path.Combine(Unity项目Assets目录, $"{物体名称}fbx_AssetBundle");

            var 当前路径 = FileUtil.GetProjectRelativePath(AssetBundle字节序列文件完整路径);
            Debug.Log(当前路径);

            var 资源视图 = AssetBundle.LoadFromFile(当前路径);

            if (资源视图 == null)
            {
                Debug.Log("提供的路径处找不到AssetBundle字节序列文件");
                return;
            }

            var 所有Mesh = 资源视图.LoadAllAssets<Mesh>();

            var 已合并Mesh = 合并多边形网格(所有Mesh, Arg_保留子网格么: true);
            多边形网格与材质_已合并Mesh = 已合并Mesh;

            注销AssetBundle(资源视图);
        }

        {
            if (UV纹理数组 == null)
            {
                var Unity项目Assets目录 = Application.dataPath;

                var AssetBundle字节序列文件完整路径 = Path.Combine(Unity项目Assets目录, $"{物体名称}_uv纹理数组_AssetBundle");

                var 当前路径 = FileUtil.GetProjectRelativePath(AssetBundle字节序列文件完整路径);
                Debug.Log(当前路径);

                var 资源视图 = AssetBundle.LoadFromFile(当前路径);

                if (资源视图 == null)
                {
                    Debug.Log("提供的路径处找不到AssetBundle字节序列文件");
                    return;
                }

                var 所有UV纹理数组 = 资源视图.LoadAllAssets<Texture2DArray>();

                UV纹理数组 = 所有UV纹理数组[0];

                注销AssetBundle(资源视图, AssetBundle注销方式.仅注销资源视图_资源依旧保留在Unity资源管理器中);

                if (测试用蓝图材质 != null)
                {
                    测试用蓝图材质.mainTexture = UV纹理数组;
                }
            }
        }

        {
            Material[] materials = new Material[多边形网格与材质_已合并Mesh.subMeshCount];

            for (var i = 0; i < materials.Length; i++)
            {
                materials[i] = 测试用实体材质;
            }

            多边形网格与材质_所有subMesh材质 = materials;
        }

        {
            if (实体预制体 != null)
            {
                UnityEngine.Object.DestroyImmediate(实体预制体);
            }

            实体预制体 = new GameObject();
            实体预制体.transform.SetParent(休眠的根节点.transform, worldPositionStays: false);
            为实体添加基本组件(实体预制体, 多边形网格与材质_已合并Mesh, 多边形网格与材质_所有subMesh材质);
        }

        {
            if (蓝图预制体 != null)
            {
                UnityEngine.Object.DestroyImmediate(蓝图预制体);
            }

            蓝图预制体 = new GameObject();
            蓝图预制体.transform.SetParent(休眠的根节点.transform, worldPositionStays: false);
            为蓝图添加高亮全息投影组件(蓝图预制体, 多边形网格与材质_已合并Mesh, 测试用蓝图材质);
        }

        if (休眠的根节点 != null && 测试用实体材质 != null && 测试用蓝图材质 != null && 多边形网格与材质_已合并Mesh != null && 多边形网格与材质_所有subMesh材质 != null && 实体预制体 != null && 蓝图预制体 != null)
        {
            var 根节点 = new GameObject();
            var 实体 = UnityEngine.Object.Instantiate(实体预制体, 根节点.transform, false);
            var 蓝图 = UnityEngine.Object.Instantiate(蓝图预制体, 根节点.transform, false);
            return;
        }
    }

    public static Mesh 合并多边形网格(Mesh[] Arg_所有Mesh, bool Arg_保留子网格么 = true)
    {
        for (var i = 0; i < Arg_所有Mesh.Length; i++)
        {
            Arg_所有Mesh[i] = 复制多边形网格(Arg_所有Mesh[i]);
        }

        var 待合并 = new List<CombineInstance>(Arg_所有Mesh.Length);
        for (var i = 0; i < Arg_所有Mesh.Length; ++i)
        {
            // subMeshIndex: 建模时, 可以人为的将三角形索引数组划分片段, 每个片段有着自己的数组地址偏移和数据长度。通过subMeshIndex可以获取该片段的数组地址偏移和数据长度, 同时通过sharedMaterials[subMeshIndex]找到该片段的材质
            //               建模软件导出的片段划分信息, Unity引擎不一定能正确识别, 因此 Arg_所有Mesh 中的所有元素都只有一个subMesh(即数组地址偏移=0,数据长度=三角形索引数组长度), 通过在此函数中合并多边形网格, 将多个Mesh变成一个Mesh中的多个subMesh
            // transform: 多边形网格对象使用世界坐标系1:1比例    例: 使用Blender建模, 有导出前执行命令 "物体/应用/位置" "物体/应用/旋转" "物体/应用/缩放" , 让变换矩阵中的位置和旋转变成(0,0,0)、缩放变成(1,1,1), 此时法向、顶点坐标.....就变成世界坐标系1:1比例下的数据
            待合并.Add(new CombineInstance
            {
                mesh = Arg_所有Mesh[i],
                subMeshIndex = 0,
                transform = Matrix4x4.identity
            });
        }

        var Result = new Mesh() { name = 待合并.First().mesh.name + "已合并" };
        Result.CombineMeshes(待合并.ToArray(), mergeSubMeshes: !Arg_保留子网格么, useMatrices: true);
        Result.RecalculateNormals();
        Result.RecalculateBounds();
        return Result;
    }


    public static Mesh 复制多边形网格(Mesh Arg_Mesh, bool Arg_保留子网格么 = false)
    {
        if (Arg_Mesh == null)
        {
            Debug.Log("传入的Mesh为空, 无法复制多边形网格");
            return null;
        }

        List<CombineInstance> list = new List<CombineInstance>(Arg_Mesh.subMeshCount);
        for (int i = 0; i < Arg_Mesh.subMeshCount; i++)
        {
            list.Add(new CombineInstance
            {
                mesh = Arg_Mesh,
                subMeshIndex = i,
                transform = Matrix4x4.identity
            });
        }

        Mesh mesh = new Mesh();
        mesh.name = Arg_Mesh.name + "已复制";
        mesh.CombineMeshes(list.ToArray(), !Arg_保留子网格么, useMatrices: true);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static void 为实体添加基本组件(GameObject Arg_由AssetBundle加载的空预制体资源_实体, Mesh Arg_ThingMesh, Material[] Arg_所有subMesh材质)
    {
        var 实体 = Arg_由AssetBundle加载的空预制体资源_实体;

        var 多边形网格配置 = 实体.AddComponent<MeshFilter>();
        多边形网格配置.sharedMesh = Arg_ThingMesh;

        var 碰撞体配置 = 实体.AddComponent<BoxCollider>();
        碰撞体配置.center = Arg_ThingMesh.bounds.center;
        碰撞体配置.size = Arg_ThingMesh.bounds.size;

        var 渲染配置 = 实体.AddComponent<MeshRenderer>();
        渲染配置.sharedMaterials = Arg_所有subMesh材质;
    }

    [Tooltip("AssetBundle.LoadFrom...系列命令会调用Unity资源管理器API将资源加载到Unity资源管理器中, 同时构造一个AssetBundle自己的资源管理器(资源视图)实例用来额外保存资源索引\n一般通过AssetBundle.LoadAsset或者AssetBundle.LoadAllAssets获取到资源引用后, 就可以注销资源视图了\n注: 调用Unity资源管理器API方法(Resources.FindObjectsOfTypeAll)查找第三方模组加载的资源时, 记得要等游戏初始化完成, 否则查找资源的时候第三方模组还没有加载, 是找不到资源的")]
    public enum AssetBundle注销方式
    {
        仅注销资源视图_资源依旧保留在Unity资源管理器中,
        我已经通过复制手段对需要的资源进行了单独创建_请将此资源视图连同资源一起注销掉
    }

    public static void 注销AssetBundle(AssetBundle Arg_资源视图, AssetBundle注销方式 注销方式 = AssetBundle注销方式.我已经通过复制手段对需要的资源进行了单独创建_请将此资源视图连同资源一起注销掉)
    {
        if (Arg_资源视图 == null)
        {
            Debug.Log("传入的AssetBundle资源视图为空, 无法注销");
            return;
        }

        try
        {
            switch (注销方式)
            {
                case AssetBundle注销方式.仅注销资源视图_资源依旧保留在Unity资源管理器中:
                    Arg_资源视图.Unload(unloadAllLoadedObjects: false);
                    break;
                case AssetBundle注销方式.我已经通过复制手段对需要的资源进行了单独创建_请将此资源视图连同资源一起注销掉:
                    Arg_资源视图.Unload(unloadAllLoadedObjects: true);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log($"AssetBundle注销失败,错误信息->{Arg_资源视图} , {e}\nUnity引擎发癫,不用管");
        }
    }

    public static void 打印AssetBundle中所有的资源索引路径_资源索引路径传参给LoadAsset方法会返回该资源的引用(AssetBundle Arg_资源视图)
    {
        if (Arg_资源视图 == null)
        {
            Debug.Log("传入的AssetBundle资源视图为空, 无法打印已加载资源索引路径");
            return;
        }

        // AssetBundle.GetAllAssetNames方法返回的并不是资源名称, 而是资源索引路径, 传参给AssetBundle.LoadAsset方法就可以返回该资源的引用
        var 所有的资源索引路径 = Arg_资源视图.GetAllAssetNames();
        Debug.Log($"AssetBundle正在打印以下已加载资源索引路径:\n{string.Join("\n", 所有的资源索引路径)}");
    }

    private static void 为蓝图添加高亮全息投影组件(GameObject Arg_由AssetBundle加载的空预制体资源_蓝图, Mesh Arg_ThingMesh, Material Arg_材质)
    {
        // 请在Unity编辑器中将多边形网格读写模式打开, <线框生成和子网格合并>会读取所有子网格并合并成一个新的多边形网格, 并遍历所有三角形生成线框绘制表(在Wireframe.OnRenderObject方法中遍历WireframeEdges并绘制)

        var 蓝图 = Arg_由AssetBundle加载的空预制体资源_蓝图;

        var 多边形网格配置 = 蓝图.AddComponent<MeshFilter>();
        多边形网格配置.sharedMesh = Arg_ThingMesh;

        var 渲染配置 = 蓝图.AddComponent<MeshRenderer>();
        渲染配置.sharedMaterial = Arg_材质;

        var 线框生成和子网格合并 = new WireframeGenerator(蓝图.transform);      // 本级必须有MeshRenderer组件才会将MeshFilter视为有效
        var 已合并Mesh = 线框生成和子网格合并.CombinedMesh;
        多边形网格配置.sharedMesh = 已合并Mesh;
    }
}

public class Triangle
{
    public Vector3 Point1;

    public Vector3 Point2;

    public Vector3 Point3;

    public Edge Edge1;

    public Edge Edge2;

    public Edge Edge3;

    public Vector3 Normal;

    public Vector3 Center;

    public Transform Parent;

    public static Vector3 ApplyLocalScale(Vector3 point, Transform t)
    {
        point.x *= t.localScale.x;
        point.y *= t.localScale.y;
        point.z *= t.localScale.z;
        return point;
    }

    public static Vector3 RemoveLocalScale(Vector3 point, Transform t)
    {
        point.x /= t.localScale.x;
        point.y /= t.localScale.y;
        point.z /= t.localScale.z;
        return point;
    }

    public Triangle SetPoints(MeshFilter meshFilter, int triangle)
    {
        Mesh sharedMesh = meshFilter.sharedMesh;
        Point1 = ApplyLocalScale(sharedMesh.vertices[sharedMesh.triangles[triangle]], meshFilter.transform);
        Point2 = ApplyLocalScale(sharedMesh.vertices[sharedMesh.triangles[triangle + 1]], meshFilter.transform);
        Point3 = ApplyLocalScale(sharedMesh.vertices[sharedMesh.triangles[triangle + 2]], meshFilter.transform);
        Edge1 = new Edge
        {
            Point1 = Point1,
            Point2 = Point2,
            Triangle = this
        };
        Edge2 = new Edge
        {
            Point1 = Point2,
            Point2 = Point3,
            Triangle = this
        };
        Edge3 = new Edge
        {
            Point1 = Point3,
            Point2 = Point1,
            Triangle = this
        };
        Center = (Point1 + Point2 + Point3) / 3f;
        Normal = ApplyLocalScale(sharedMesh.normals[sharedMesh.triangles[triangle]], meshFilter.transform);
        Parent = meshFilter.transform;
        return this;
    }

    public Triangle SetPoints(Mesh mesh, int triangle)
    {
        Point1 = mesh.vertices[mesh.triangles[triangle]];
        Point2 = mesh.vertices[mesh.triangles[triangle + 1]];
        Point3 = mesh.vertices[mesh.triangles[triangle + 2]];
        Edge1 = new Edge
        {
            Point1 = Point1,
            Point2 = Point2,
            Triangle = this
        };
        Edge2 = new Edge
        {
            Point1 = Point2,
            Point2 = Point3,
            Triangle = this
        };
        Edge3 = new Edge
        {
            Point1 = Point3,
            Point2 = Point1,
            Triangle = this
        };
        Center = (Point1 + Point2 + Point3) / 3f;
        Normal = mesh.normals[mesh.triangles[triangle]];
        return this;
    }

    public bool IsValid()
    {
        if (!Edge1.IsValid())
        {
            return false;
        }

        if (Edge2.IsValid())
        {
            return Edge3.IsValid();
        }

        return false;
    }

    public Edge[] GetShortestEdges()
    {
        float num = Vector3.Distance(Point1, Point2);
        float num2 = Vector3.Distance(Point2, Point3);
        float num3 = Vector3.Distance(Point3, Point1);
        if (num > num2 && num > num3)
        {
            return new Edge[2] { Edge2, Edge3 };
        }

        if (num2 > num && num2 > num3)
        {
            return new Edge[2] { Edge1, Edge3 };
        }

        return new Edge[2] { Edge2, Edge1 };
    }

    public Edge GetShortestEdge()
    {
        float num = Vector3.Distance(Point1, Point2);
        float num2 = Vector3.Distance(Point2, Point3);
        float num3 = Vector3.Distance(Point3, Point1);
        if (num < num2 && num < num3)
        {
            return Edge1;
        }

        if (num2 < num && num2 < num3)
        {
            return Edge2;
        }

        return Edge3;
    }
}

public class Edge
{
    public static readonly float MinDistance = 0.005f;
    public Triangle Triangle;
    public Vector3 Point1;
    public Vector3 Point2;
    public Vector3 CachedPoint1;
    public Vector3 CachedPoint2;

    public Vector3[] WorldEdge()
    {
        return new Vector3[2]
        {
            Triangle.Parent.TransformPoint(Point1),
            Triangle.Parent.TransformPoint(Point2)
        };
    }

    public Vector3 WorldCenter()
    {
        return Triangle.Parent.TransformPoint(Center());
    }

    public Vector3 WorldNormal()
    {
        return Triangle.Parent.TransformPoint(Center() + Triangle.Normal);
    }

    public Vector3 Center()
    {
        return (Point1 + Point2) / 2f;
    }

    public bool IsValid()
    {
        return Vector3.Distance(Point1, Point2) > MinDistance;
    }

    public bool IsShortEdge()
    {
        Edge[] shortestEdges = Triangle.GetShortestEdges();
        if (shortestEdges[0] == this)
        {
            return true;
        }

        if (shortestEdges[1] == this)
        {
            return true;
        }

        return false;
    }
}

public class WireframeGenerator
{
    public List<Edge> Edges = new List<Edge>();

    private List<Edge> _checkedEdges = new List<Edge>();

    public Mesh CombinedMesh;

    private Transform _transform;

    public WireframeGenerator(Transform transform)
    {
        _transform = transform;
        GenerateEdges();
    }

    public bool IsBadEdge(Edge edge)
    {
        foreach (Edge checkedEdge in _checkedEdges)
        {
            if (edge != checkedEdge && edge.Triangle != checkedEdge.Triangle && edge.Center() == checkedEdge.Center() && Vector3.Distance(checkedEdge.Triangle.Normal, edge.Triangle.Normal) < Edge.MinDistance)
            {
                return true;
            }

            if (!edge.IsValid())
            {
                return true;
            }

            if (!edge.Triangle.IsValid())
            {
                return true;
            }
        }

        return false;
    }

    public Vector3 GetOffset(Transform transform)
    {
        Vector3 result = -transform.position;
        while ((bool)transform.parent)
        {
            result -= transform.position;
            transform = transform.parent;
        }

        return result;
    }

    public void GenerateEdges()
    {
        Edges = new List<Edge>();
        _checkedEdges = new List<Edge>();
        CombinedMesh = new Mesh();
        MeshFilter[] componentsInChildren = _transform.GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> list = new List<CombineInstance>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            Mesh sharedMesh = componentsInChildren[i].sharedMesh;
            if (!componentsInChildren[i].GetComponent<Renderer>() || sharedMesh == null)
            {
                continue;
            }

            _ = componentsInChildren[i].transform.position;
            for (int j = 0; j < sharedMesh.subMeshCount; j++)
            {
                List<Vector3> list2 = new List<Vector3>();
                List<int> list3 = new List<int>();
                for (int k = 0; k < sharedMesh.triangles.Length; k++)
                {
                    Vector3 item = sharedMesh.vertices[sharedMesh.triangles[k]];
                    list2.Add(item);
                    list3.Add(k);
                }

                Mesh mesh = new Mesh
                {
                    vertices = list2.ToArray(),
                    triangles = list3.ToArray()
                };
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                CombineInstance item2 = new CombineInstance
                {
                    mesh = mesh,
                    subMeshIndex = j,
                    transform = componentsInChildren[i].transform.localToWorldMatrix
                };
                list.Add(item2);
            }
        }

        CombinedMesh.CombineMeshes(list.ToArray(), mergeSubMeshes: true, useMatrices: true);
        Mathf.RoundToInt((float)CombinedMesh.vertexCount / 3f);
        for (int l = 0; l < CombinedMesh.triangles.Length - 2; l += 3)
        {
            Triangle triangle = new Triangle();
            triangle.SetPoints(CombinedMesh, l);
            _checkedEdges.Add(triangle.Edge1);
            _checkedEdges.Add(triangle.Edge2);
            _checkedEdges.Add(triangle.Edge3);
        }

        int num = 0;
        foreach (Edge checkedEdge in _checkedEdges)
        {
            if (!IsBadEdge(checkedEdge))
            {
                Edges.Add(checkedEdge);
            }

            num++;
        }
    }
}
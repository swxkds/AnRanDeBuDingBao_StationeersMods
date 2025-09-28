Shader "Custom/AnRanDeBuDingBao/Hologram"
{
    Properties
    {
        [Header(Color)]
        _Color ("表面颜色", Color) =  (0, 1, 1, 1)
        _Brightness ("表面透明度", Range(0.1, 1)) = 0.6
        _Alpha ("整体透明度", Range(0.1, 1)) = 0.6

        [Header(Scanlines)]
        _ScanlineColor ("扫描线颜色", Color) =  (0, 1, 1, 1)
        _ScanEnabled ("扫描线透明度", Range(0.1, 1)) = 1
        _ScanSpeed ("扫描线步进速度", Range(-2, 2)) = -1
        _Direction ("扫描线旋转角度", Vector) =  (0.45, 0.45, 0, 0)
        _ScanTiling ("扫描线密度", Range(0.01, 100)) = 30
        _ScanWidth ("扫描线宽度比例", Range(0.4, 0.7)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100

        Pass
        {
            ZTest Always
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
            ColorMask RGB 0
            Offset -1,-1 

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup         // 1. 开启程序化实例化支持，并指定 setup函数 为读取矩阵的入口

            #include "UnityCG.cginc"

            struct vertex_uv
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct vertex_objPos
            {
                float4 vertex : SV_POSITION;
                float3 objPos : TEXCOORD0;                // 模型空间坐标，用于扫描线
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 2. 映射 C# 中的「变换矩阵」结构体与 ComputeBuffer (_InstanceData)
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED     
            struct TransformMatrix
            {
                float4x4 ObjectToWorldMatrix;
                float4x4 WorldToObjectMatrix;
            };
            StructuredBuffer<TransformMatrix> _InstanceData;
            #endif

            // 3. 实现 setup 函数。在顶点着色器初始化时，Unity 会自动调用它来替换当前实例的变换矩阵
            void setup()
            {
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                unity_ObjectToWorld = _InstanceData[unity_InstanceID].ObjectToWorldMatrix;
                unity_WorldToObject = _InstanceData[unity_InstanceID].WorldToObjectMatrix;
                #endif
            }

            // 4. 声明变量引用外部的同名属性, 比如Properties里的
            float4 _Color;
            float4 _ScanlineColor;
            float _Brightness;
            float _Alpha;
            float _ScanEnabled;
            float _ScanSpeed;
            float4 _Direction;
            float _ScanTiling;
            float _ScanWidth;

            vertex_objPos vert(vertex_uv input)
            {
                vertex_objPos o;
                
                UNITY_SETUP_INSTANCE_ID(input);                 // UNITY_SETUP_INSTANCE_ID 内部会自动触发上面定义的 setup() 从而应用正确的世界矩阵
                UNITY_TRANSFER_INSTANCE_ID(input, o);

                o.vertex = UnityObjectToClipPos(input.vertex);
                o.objPos = input.vertex.xyz;

                return o;


            }

            float4 frag(vertex_objPos input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // ---- 纯色底色 ----
                // 直接使用全局变量
                float4 baseColor = _Color;
                float3 base = baseColor.rgb * _Brightness;

                // ---- 扫描线 ----
                float scan = dot(input.objPos, _Direction.xyz) * _ScanTiling + _Time.y * _ScanSpeed;
                // 基础波形：0 到 1 之间的三角波
                float wave = abs(frac(scan) - 0.5) * 2.0;
                // 使用 smoothstep 重新映射波形，控制线宽与间距比例
                float scanLine = smoothstep(_ScanWidth - 0.05, _ScanWidth + 0.05, wave);
                float scanMask = lerp(1.0, scanLine, _ScanEnabled);

                float4 scanColor = _ScanlineColor;
                float3 finalRGB = lerp(scanColor.rgb, base, scanMask);
                // scanMask = 1 时显示底色，scanMask = 0 时显示深色扫描线

                float finalAlpha = baseColor.a * _Alpha;
                return float4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
}
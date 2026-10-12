Shader "Horsecremental/UI/Upgrade Tree Background"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BaseColor ("Night Color", Color) = (0.055, 0.043, 0.09, 1)
        _MistColor ("Purple Mist", Color) = (0.20, 0.14, 0.26, 1)
        _SilhouetteColor ("Forest Shadows", Color) = (0.028, 0.033, 0.052, 1)
        _Aspect ("Panel Aspect", Float) = 2.2
        _PixelResolution ("Pixel Width", Range(160, 1024)) = 512
        _LandscapeScale ("Terrain Scale", Range(2, 12)) = 6
        _Vignette ("Dark Edges", Range(0, 1)) = 0.72
        _Grain ("Pixel Grain", Range(0, 0.12)) = 0.025
        _DriftSpeed ("Mist Drift Speed", Range(0, 0.1)) = 0.012
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct AppData { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float4 localPosition : TEXCOORD1; fixed4 color : COLOR; UNITY_VERTEX_OUTPUT_STEREO };
            sampler2D _MainTex;
            fixed4 _BaseColor, _MistColor, _SilhouetteColor;
            float4 _ClipRect;
            float _Aspect, _PixelResolution, _LandscapeScale, _Vignette, _Grain, _DriftSpeed;

            Varyings Vert(AppData input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.localPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }
            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }
            float Noise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), f.x),
                    lerp(Hash(cell + float2(0, 1)), Hash(cell + float2(1, 1)), f.x), f.y);
            }
            float Terrain(float2 p)
            {
                return Noise(p) * 0.57 + Noise(p * 2.03 + 19.2) * 0.29 + Noise(p * 4.01 + 7.3) * 0.14;
            }
            float Pine(float2 p, float seed)
            {
                float height = 0.45 + seed * 0.25;
                float y = p.y + 0.38;
                float width = 0.20 * (1.0 - y / height);
                float tier = frac(y / height * 3.0);
                width *= 0.55 + tier * 0.8;
                float leaves = step(0.08, y) * step(y, height) * step(abs(p.x), width);
                float trunk = step(abs(p.x), 0.021) * step(0.0, y) * step(y, 0.15);
                return max(leaves, trunk);
            }
            fixed4 Frag(Varyings input) : SV_Target
            {
                float aspect = max(_Aspect, 0.1);
                float2 resolution = float2(_PixelResolution, _PixelResolution / aspect);
                float2 uv = floor(input.uv * resolution) / resolution;
                float2 world = uv * float2(aspect, 1.0);
                float terrain = Terrain(world * _LandscapeScale);
                // The terrain is fixed; only the low-contrast mist moves.
                float drift = _Time.y * _DriftSpeed;
                float mist = Noise(world * 2.6 + float2(drift, -drift * 0.3));
                float light = smoothstep(0.17, 0.81, terrain) * 0.65 + mist * 0.22;
                float3 color = lerp(_BaseColor.rgb, _MistColor.rgb, light);
                float ridge = smoothstep(0.49, 0.54, Terrain(world * 3.3 + 25.7));
                color = lerp(color, _SilhouetteColor.rgb, ridge * 0.23);

                float2 forest = world * 5.2;
                float2 cell = floor(forest);
                float seed = Hash(cell + 8.1);
                float2 local = frac(forest) - 0.5;
                local.x -= (Hash(cell + 2.1) - 0.5) * 0.35;
                float tree = Pine(local, seed) * step(0.35, seed);
                float clearing = smoothstep(0.12, 0.5, length((uv - 0.5) * float2(1.0, 0.7)));
                color = lerp(color, _SilhouetteColor.rgb, tree * (0.23 + clearing * 0.39));
                // Relative grain stays subtle in a project using linear color space.
                color *= 1.0 + (Hash(floor(uv * resolution)) - 0.5) * _Grain * 2.0;
                color *= 1.0 - frac(uv.y * resolution.y * 0.5) * 0.035;
                float edge = smoothstep(0.16, 0.69, length((uv - 0.5) * float2(1.05, 1.15)));
                color *= 1.0 - edge * _Vignette;
                fixed4 result = fixed4(color, tex2D(_MainTex, input.uv).a) * input.color;
                #ifdef UNITY_UI_CLIP_RECT
                    result.a *= UnityGet2DClipping(input.localPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                    clip(result.a - 0.001);
                #endif
                return result;
            }
            ENDCG
        }
    }
}

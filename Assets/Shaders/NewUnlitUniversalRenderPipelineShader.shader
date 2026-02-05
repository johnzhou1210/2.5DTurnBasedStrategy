Shader "Custom/URP_Sprite_Shadow"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _BaseColor("Tint", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            
            
            Cull Off 
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _BaseColor;
                half _Cutoff;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // This was missing! It defines the data passed from Vertex to Fragment stage.
            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : TEXCOORD1;
                float3 positionWS : TEXCOORD2; // Added for distance calculations later
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // This was missing! The actual vertex math.
            Varyings vert(Attributes input) {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(input);
                
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 finalColor = texColor * input.color * _BaseColor;
                
                // Alpha Clipping (Cutout)
                clip(finalColor.a - _Cutoff); 
                
                return finalColor;
            }
            ENDHLSL
        }

        // Shadow pass
        Pass
{
    Name "ShadowCaster"
    Tags { "LightMode" = "ShadowCaster" }

    ZWrite On
    ZTest LEqual
    ColorMask 0 // We only care about depth, not color
    Cull Off

    HLSLPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #pragma multi_compile_instancing
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

    // Use the same CBUFFER to keep the SRP Batcher happy
    CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        half4 _BaseColor;
        half _Cutoff;
    CBUFFER_END

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);

    struct Attributes {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    Varyings vert(Attributes input) {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);

        // Standard URP shadow position logic
        float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
        float3 normalWS = TransformObjectToWorldNormal(float3(0, 0, 1)); 
        output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));
        
        output.uv = TRANSFORM_TEX(input.uv, _MainTex);
        return output;
    }

    half4 frag(Varyings input) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(input);
        // This clip() is what prevents the "blob"
        half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
        clip(alpha - _Cutoff); 
        return 0;
    }
    ENDHLSL
}
    }
}
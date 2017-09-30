// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/Internal/GUI/CheckerBoard"
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "white" {}
		_Alpha ("Alpha", float) = 0
		_Size ("Size", float) = 0.15
		_ColorA ("ColorA", color) = (1,1,1,1)
		_ColorB ("ColorB", color) = (0.7,0.7,0.7,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 worldPos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};
			sampler2D _MainTex;
			float _Alpha;
			float _Size;
			float4 _ColorA;
			float4 _ColorB;

			uniform float4x4 clipMatrix;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float4 texc = tex2D(_MainTex, i.uv);
				float2 c = i.worldPos.xy*_Size;
				float4 clipPos = mul(clipMatrix, i.worldPos);
				c = floor(c) / 2;
				float checker = frac(c.x + c.y) * 2;
				if (clipPos.x > 1 || clipPos.x < 0 || clipPos.y>1 || clipPos.y < 0)
					discard;
				return lerp(_ColorA,_ColorB,checker)*(1-_Alpha)+texc*_Alpha;
			}
			ENDCG
		}
	}
}

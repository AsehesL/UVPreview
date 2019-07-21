Shader "Hidden/Internal/GUI/UVRender"
{
	Properties
	{
		_Color ("Color", color) = (0,1,0,1)
		_UVIndex ("UVIndex", float) = 0.5
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue"="Transparent" "ForceSupported"="True" }
		Pass
		{
			cull off
			zwrite off
			blend srcalpha oneminussrcalpha
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma exclude_renderers opengl
			#pragma target 4.0

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoord2 : TEXCOORD1;
				float2 texcoord3 : TEXCOORD2;
				float2 texcoord4 : TEXCOORD3;
			};

			struct v2g
			{
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD0;
			};

			struct g2f {
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD0;
			};

			float4 _Color;
			float _UVIndex;
			uniform float4x4 clipMatrix;

			v2g vert(appdata v)
			{
				v2g o;
				if (_UVIndex < 0.5) {
					o.vertex = UnityObjectToClipPos(float4(v.texcoord.xy, 0, 1));
					o.worldPos = mul(unity_ObjectToWorld, float4(v.texcoord.xy, 0, 1));
				}
				else if (_UVIndex >= 0.5 && _UVIndex < 1.5) {
					o.vertex = UnityObjectToClipPos(float4(v.texcoord2.xy, 0, 1));
					o.worldPos = mul(unity_ObjectToWorld, float4(v.texcoord2.xy, 0, 1));
				}
				else if (_UVIndex >= 1.5 && _UVIndex < 2.5) {
					o.vertex = UnityObjectToClipPos(float4(v.texcoord3.xy, 0, 1));
					o.worldPos = mul(unity_ObjectToWorld, float4(v.texcoord3.xy, 0, 1));
				}
				else if (_UVIndex >= 2.5) {
					o.vertex = UnityObjectToClipPos(float4(v.texcoord4.xy, 0, 1));
					o.worldPos = mul(unity_ObjectToWorld, float4(v.texcoord4.xy, 0, 1));
				}
				return o;
			}

			[maxvertexcount(6)]
			void geom(triangle v2g i[3], inout LineStream<g2f> os)
			{
				g2f o;

				o.vertex = i[0].vertex;
				o.worldPos = i[0].worldPos;
				os.Append(o);

				o.vertex = i[1].vertex;
				o.worldPos = i[1].worldPos;
				os.Append(o);

				os.RestartStrip();

				o.vertex = i[0].vertex;
				o.worldPos = i[0].worldPos;
				os.Append(o);

				o.vertex = i[2].vertex;
				o.worldPos = i[2].worldPos;
				os.Append(o);

				os.RestartStrip();

				o.vertex = i[1].vertex;
				o.worldPos = i[1].worldPos;
				os.Append(o);

				o.vertex = i[2].vertex;
				o.worldPos = i[2].worldPos;
				os.Append(o);

				os.RestartStrip();
		
			}

			fixed4 frag(g2f i) : SV_Target
			{  
				float4 clipPos = mul(clipMatrix, i.worldPos);
				if (clipPos.x > 1 || clipPos.x < 0 || clipPos.y>1 || clipPos.y < 0)
					discard;
				return _Color;
			}
			ENDCG
		}
		Pass 
		{
			cull off
			zwrite off
			blend srcalpha oneminussrcalpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers opengl

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord2 : TEXCOORD1;
				float2 texcoord3 : TEXCOORD2;
				float2 texcoord4 : TEXCOORD3;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float4 worldPos : TEXCOORD0;
			};

			uniform float4x4 clipMatrix;
			float _UVIndex;

			v2f vert(appdata v)
			{
				v2f o;
				if (_UVIndex < 0.5) {
					o.vertex = UnityObjectToClipPos(float4(v.texcoord.xy, 0, 1));
					o.worldPos = mul(unity_ObjectToWorld, float4(v.texcoord.xy, 0, 1));
				}
				else if (_UVIndex >= 0.5 && _UVIndex < 1.5) {
					o.vertex = UnityObjectToClipPos(float4(v.texcoord2.xy, 0, 1));
					o.worldPos = mul(unity_ObjectToWorld, float4(v.texcoord2.xy, 0, 1));
				}
				else if (_UVIndex >= 1.5 && _UVIndex < 2.5) {
					o.vertex = UnityObjectToClipPos(float4(v.texcoord3.xy, 0, 1));
					o.worldPos = mul(unity_ObjectToWorld, float4(v.texcoord3.xy, 0, 1));
				}
				else if (_UVIndex >= 2.5) {
					o.vertex = UnityObjectToClipPos(float4(v.texcoord4.xy, 0, 1));
					o.worldPos = mul(unity_ObjectToWorld, float4(v.texcoord4.xy, 0, 1));
				}
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = i.color;
				col.a = 0.5;
				float4 clipPos = mul(clipMatrix, i.worldPos);
				if (clipPos.x > 1 || clipPos.x < 0 || clipPos.y>1 || clipPos.y < 0)
					discard;
				return col;
			}
			ENDCG
		}
	}
}

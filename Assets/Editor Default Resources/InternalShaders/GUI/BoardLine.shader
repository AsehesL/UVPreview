// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/Internal/GUI/BoardLine"
{
	Properties
	{
		_Color ("Color", color) = (0,1,0,1)
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
			uniform float4x4 clipMatrix;

			v2g vert(appdata v)
			{
				v2g o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			[maxvertexcount(5)]
			void geom(triangle v2g i[3], inout LineStream<g2f> os)
			{
				g2f o;

				o.vertex = i[0].vertex;
				o.worldPos = i[0].worldPos;
				os.Append(o);

				o.vertex = i[1].vertex;
				o.worldPos = i[1].worldPos;
				os.Append(o);

				o.vertex = i[2].vertex;
				o.worldPos = i[2].worldPos;
				os.Append(o);

				os.RestartStrip();

				o.vertex = i[0].vertex;
				o.worldPos = i[0].worldPos;
				os.Append(o);

				o.vertex = i[2].vertex;
				o.worldPos = i[2].worldPos;
				os.Append(o);
		
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
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float4 worldPos : TEXCOORD0;
			};

			uniform float4x4 clipMatrix;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
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

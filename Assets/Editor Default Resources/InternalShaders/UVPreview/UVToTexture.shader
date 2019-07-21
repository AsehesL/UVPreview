Shader "Hidden/Internal/UVPreview/UVToTexture"
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
			};

			struct g2f {
				float4 vertex : SV_POSITION;
			};

			float4 _Color;
			float _UVIndex;

			v2g vert(appdata v)
			{
				v2g o;
				if (_UVIndex < 0.5) {
					o.vertex = float4(v.texcoord.x * 2 - 1, (1 - v.texcoord.y) * 2 - 1, 0, 1);
				}
				else if (_UVIndex >= 0.5 && _UVIndex < 1.5) {
					o.vertex = float4(v.texcoord2.x * 2 - 1, (1 - v.texcoord2.y) * 2 - 1, 0, 1);
				}
				else if (_UVIndex >= 1.5 && _UVIndex < 2.5) {
					o.vertex = float4(v.texcoord3.x * 2 - 1, (1 - v.texcoord3.y) * 2 - 1, 0, 1);
				}
				else if (_UVIndex >= 2.5) {
					o.vertex = float4(v.texcoord4.x * 2 - 1, (1 - v.texcoord4.y) * 2 - 1, 0, 1);
				}
				return o;
			}

			[maxvertexcount(6)]
			void geom(triangle v2g i[3], inout LineStream<g2f> os)
			{
				g2f o;

				o.vertex = i[0].vertex;
				os.Append(o);

				o.vertex = i[1].vertex;
				os.Append(o);

				os.RestartStrip();

				o.vertex = i[0].vertex;
				os.Append(o);

				o.vertex = i[2].vertex;
				os.Append(o);

				os.RestartStrip();

				o.vertex = i[1].vertex;
				os.Append(o);

				o.vertex = i[2].vertex;
				os.Append(o);

				os.RestartStrip();
		
			}

			fixed4 frag(g2f i) : SV_Target
			{  
				return _Color;
			}
			ENDCG
		}
	}
}

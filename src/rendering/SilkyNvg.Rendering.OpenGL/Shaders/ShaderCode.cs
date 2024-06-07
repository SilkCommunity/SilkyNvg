namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal static class ShaderCode
    {

        public static readonly string VERTEX_SHADER_CODE = @"
            #version 150 core

            in vec2 vertex;
            in vec4 tcoord;

            out vec2 pass_vertex;
            out vec2 pass_tcoord;
			out vec2 uv;

            uniform vec2 viewSize;

            void main(void) {
	            pass_vertex = vertex;
	            pass_tcoord = tcoord.xy;
				uv = 0.5 * tcoord.zw;
	            gl_Position = vec4(2.0 * vertex.x / viewSize.x - 1.0, 1.0 - 2.0 * vertex.y / viewSize.y, 0, 1);
            }";

        public static readonly string FRAGMENT_SHADER_CODE = @"
			#version 150 core

			in vec2 pass_vertex;
			in vec2 pass_tcoord;
			in vec2 uv;

			out vec4 out_Colour;

			layout (std140) uniform frag {
				mat3 scissorMat;
				mat3 paintMat;
				vec4 innerCol;
				vec4 outerCol;
				vec2 scissorExt;
				vec2 scissorScale;
				vec2 extent;
				float radius;
				float feather;
				float strokeMult;
				float strokeThr;
				int texType;
				int type;
			};

			uniform sampler2D tex;

			float sdroundrect(vec2 pt, vec2 ext, float rad) {
				vec2 ext2 = ext - vec2(rad, rad);
				vec2 d = abs(pt) - ext2;
				return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rad;
			}

			float scissorMask(vec2 p) {
				vec2 sc = (abs((scissorMat * vec3(p, 1.0)).xy) - scissorExt);
				sc = vec2(0.5, 0.5) - sc * scissorScale;
				return clamp(sc.x, 0.0, 1.0) * clamp(sc.y, 0.0, 1.0);
			}

			void main(void) {
				float scissor = scissorMask(pass_vertex);

				if (type == 0) { // Gradient
					vec2 pt = (paintMat * vec3(pass_vertex, 1.0)).xy;
					float d = clamp((sdroundrect(pt, extent, radius) + feather * 0.5) / feather, 0.0, 1.0);
					vec4 colour = mix(innerCol, outerCol, d);

					colour *= scissor;
					out_Colour = colour;
				} else if (type == 1) { // Image
    				vec2 pt = (paintMat * vec3(pass_vertex, 1.0)).xy / extent;

					vec4 colour = texture(tex, pt);

					if (texType == 1) {
						colour = vec4(colour.xyz * colour.w, colour.w);
					} else if (texType == 2) {
						colour = vec4(colour.x);
					}

					colour *= innerCol;
					colour *= scissor;
					out_Colour = colour;
				} else if (type == 2) { // Stencil Fill
					out_Colour = vec4(1, 1, 1, 1);
				} else if (type == 3) { // Textured Tris
					vec4 colour = texture(tex, pass_tcoord);

					if (texType == 1) {
						colour = vec4(colour.xyz * colour.w, colour.w);
					}
					if (texType == 2) {
						colour = vec4(colour.x);
					}

					colour *= scissor;
					out_Colour = colour * innerCol;
				}
			}";

		public static readonly string FRAGMENT_SHADER_EDGE_AA_CODE = @"
			#version 150 core

			in vec2 pass_vertex;
			in vec2 pass_tcoord;
			in vec2 uv;

			out vec4 out_Colour;

			layout (std140) uniform frag {
				mat3 scissorMat;
				mat3 paintMat;
				vec4 innerCol;
				vec4 outerCol;
				vec2 scissorExt;
				vec2 scissorScale;
				vec2 extent;
				float radius;
				float feather;
				float strokeMult;
				float strokeThr;
				int lineStyle;
				int texType;
				int type;
			};

			uniform sampler2D tex;

			float sdroundrect(vec2 pt, vec2 ext, float rad) {
				vec2 ext2 = ext - vec2(rad, rad);
				vec2 d = abs(pt) - ext2;
				return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rad;
			}

			float scissorMask(vec2 p) {
				vec2 sc = (abs((scissorMat * vec3(p, 1.0)).xy) - scissorExt);
				sc = vec2(0.5, 0.5) - sc * scissorScale;
				return clamp(sc.x, 0.0, 1.0) * clamp(sc.y, 0.0, 1.0);
			}

			float glow(vec2 uv) {
				return smoothstep(0.0, 1.0, 1.0 - 2.0 * abs(uv.x));
			}

			float dashed(vec2 uv) {
				float fy = fract(uv.y / 4.0);
				float w = step(fy, 0.5);
				fy *= 4.0;
				if (fy >= 1.5) {
					fy -= 1.5;
				} else if (fy <= 0.5) {
					fy = 0.5 - fy;
				} else {
					fy = 0.0;
				}
				w *= smoothstep(0.0, 1.0, 6.0 * (0.25 - (uv.x * uv.x + fy * fy)));
				return w;
			}

			float dotted(vec2 uv) {
				float fy = 4.0 * fract(uv.y / 4.0) - 0.5;
				return smoothstep(0.0, 1.0, 6 * (0.25 - (uv.x * uv.x + fy * fy)));
			}

			float strokeMask() {
				return min(1.0, (1.0 - abs(pass_tcoord.x * 2.0 - 1.0)) * strokeMult) * pass_tcoord.y;
			}

			void main(void) {
				float scissor = scissorMask(pass_vertex);

				float strokeAlpha = strokeMask();
				if (lineStyle == 2) strokeAlpha*=dashed(uv);
				if (lineStyle == 3) strokeAlpha*=dotted(uv);
				if (lineStyle == 4) strokeAlpha*=glow(uv);
				if (strokeAlpha < strokeThr) {
					discard;
				}

				if (type == 0) { // Gradient
					vec2 pt = (paintMat * vec3(pass_vertex, 1.0)).xy;
					float d = clamp((sdroundrect(pt, extent, radius) + feather * 0.5) / feather, 0.0, 1.0);
					vec4 colour = mix(innerCol, outerCol, d);

					colour *= strokeAlpha * scissor;
					out_Colour = colour;
				} else if (type == 1) { // Image
					vec2 pt = (paintMat * vec3(pass_vertex, 1.0)).xy / extent;

					vec4 colour = texture(tex, pt);

					if (texType == 1) {
						colour = vec4(colour.xyz * colour.w, colour.w);
					} else if (texType == 2) {
						colour = vec4(colour.x);
					}

					colour *= innerCol;
					colour *= strokeAlpha * scissor;
					out_Colour = colour;
				} else if (type == 2) { // Stencil Fill
					out_Colour = vec4(1, 1, 1, 1);
				} else if (type == 3) { // Textured Tris
					vec4 colour = texture(tex, pass_tcoord);

					if (texType == 1) {
						colour = vec4(colour.xyz * colour.w, colour.w);
					}
					if (texType == 2) {
						colour = vec4(colour.x);
					}

					colour *= scissor;
					out_Colour = colour * outerCol;
				}
			}";

    }
}

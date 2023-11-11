namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    internal static class ShaderCode
    {
	    public const string VertexShaderCode = @"
#version 450
struct FragUniform
{
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
};

struct FlatFragUniform
{
	int texType;
	int type;
};

layout (location = 0) in vec2 vertex;
layout (location = 1) in vec2 tcoord;

layout (location = 2) in vec3 Matrix11xx;
layout (location = 3) in vec3 Matrix12xx;
layout (location = 4) in vec3 Matrix13xx;
										
layout (location = 5) in vec3 Matrix21xx;
layout (location = 6) in vec3 Matrix22xx;
layout (location = 7) in vec3 Matrix23xx;

layout(location = 8) in vec4 innerCol;
layout(location = 9) in vec4 outerCol;
layout(location = 10) in vec2 scissorExt;
layout(location = 11) in vec2 scissorScale;
layout(location = 12) in vec2 extent;

//float radius;
//float feather;
//float strokeMult;
//float strokeThr;
layout(location = 13) in vec4 rfss;

//int texType;
//int type;
layout(location = 14) in ivec2 tt;

layout (location = 0) out vec2 pass_vertex;
layout (location = 1) out vec2 pass_tcoord;
layout (location = 2) out FragUniform pass_state;
layout (location = 17) out flat FlatFragUniform pass_flatState;

layout (set = 0, binding = 0) uniform VertexUniforms {
	vec2 viewSize;
};

void main(void) {
	pass_vertex = vertex;
	pass_tcoord = tcoord;
	
	pass_state.scissorMat = mat3(Matrix11xx, Matrix12xx, Matrix13xx);
	pass_state.paintMat = mat3(Matrix21xx, Matrix22xx, Matrix23xx);
	pass_state.innerCol = innerCol;
	pass_state.outerCol = outerCol;
	pass_state.scissorExt = scissorExt;
	pass_state.scissorScale = scissorScale;
	pass_state.extent = extent;
	pass_state.radius = rfss.x;
	pass_state.feather = rfss.y;
	pass_state.strokeMult = rfss.z;
	pass_state.strokeThr = rfss.w;
	pass_flatState.texType = tt.x;
	pass_flatState.type = tt.y;
	gl_Position = vec4(2.0 * vertex.x / viewSize.x - 1.0, 2.0 * vertex.y / viewSize.y - 1.0, 0.0, 1.0);
	gl_Position.y = -gl_Position.y;
}";

	    public const string FragmentShaderCode = @"
#version 450

struct FragUniform
{
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
};

struct FlatFragUniform
{
	int texType;
	int type;
};

layout (location = 0) in vec2 pass_vertex;
layout (location = 1) in vec2 pass_tcoord;
layout (location = 2) in FragUniform state;
layout (location = 17) in flat FlatFragUniform flatState;

layout (location = 0) out vec4 out_Colour;

layout(set = 0, binding = 1) uniform sampler texsampler;
layout(set = 0, binding = 2) uniform texture2D tex;

float sdroundrect(vec2 pt, vec2 ext, float rad) {
    vec2 ext2 = ext - vec2(rad, rad);
    vec2 d = abs(pt) - ext2;
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rad;
}

float scissorMask(vec2 p) {
    vec2 sc = (abs((state.scissorMat * vec3(p, 1.0)).xy) - state.scissorExt);
    sc = vec2(0.5, 0.5) - sc * state.scissorScale;
    return clamp(sc.x, 0.0, 1.0) * clamp(sc.y, 0.0, 1.0);
}

void main() 
{
	float radius = state.radius;
	float feather = state.feather;
	float strokeMult = state.strokeMult;
	float strokeThr = state.strokeThr;

	int texType = flatState.texType;
	int type = flatState.type;

	float scissor = scissorMask(pass_vertex);
	if (type == 0) { // Gradient
		vec2 pt = (state.paintMat * vec3(pass_vertex, 1.0)).xy;
		float d = clamp((sdroundrect(pt, state.extent, radius) + feather * 0.5) / feather, 0.0, 1.0);
		vec4 colour = mix(state.innerCol, state.outerCol, d);
		colour *= scissor;
		out_Colour = colour;
	} else if (type == 1) { // Image
		vec2 pt = (state.paintMat * vec3(pass_vertex, 1.0)).xy / state.extent;
		vec4 colour = texture(sampler2D(tex, texsampler), pt);
		if (texType == 1) {
			colour = vec4(colour.xyz * colour.w, colour.w);
		} else if (texType == 2) {
			colour = vec4(colour.x);
		}
		colour *= state.innerCol;
		colour *= scissor;
		out_Colour = colour;
	} else if (type == 2) { // Stencil Fill
		out_Colour = vec4(1, 1, 1, 1);
	} else if (type == 3) { // Textured Tris
		vec4 colour = texture(sampler2D(tex, texsampler), pass_tcoord);
		if (texType == 1) {
			colour = vec4(colour.xyz * colour.w, colour.w);
		} else if (texType == 2) {
			colour = vec4(colour.x);
		}
		colour *= scissor;
		out_Colour = colour * state.innerCol;
	}
}";

        public const string FragmentShaderEdgeAaCode = @"
#version 450

struct FragUniform
{
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
};

struct FlatFragUniform
{
	int texType;
	int type;
};


layout (location = 0) in vec2 pass_vertex;
layout (location = 1) in vec2 pass_tcoord;
layout (location = 2) in FragUniform state;
layout (location = 17) in flat FlatFragUniform flatState;

layout (location = 0) out vec4 out_Colour;

layout(set = 0, binding = 1) uniform sampler texsampler;
layout(set = 0, binding = 2) uniform texture2D tex;

float sdroundrect(vec2 pt, vec2 ext, float rad) {
    vec2 ext2 = ext - vec2(rad, rad);
    vec2 d = abs(pt) - ext2;
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rad;
}

float scissorMask(vec2 p) {
    vec2 sc = (abs((state.scissorMat * vec3(p, 1.0)).xy) - state.scissorExt);
    sc = vec2(0.5, 0.5) - sc * state.scissorScale;
    return clamp(sc.x, 0.0, 1.0) * clamp(sc.y, 0.0, 1.0);
}

float strokeMask() {
	return min(1.0, (1.0 - abs(pass_tcoord.x * 2.0 - 1.0)) * state.strokeMult) * pass_tcoord.y;
}

void main(void) {

	float radius = state.radius;
	float feather = state.feather;
	float strokeMult = state.strokeMult;
	float strokeThr = state.strokeThr;

	int texType = flatState.texType;
	int type = flatState.type;

	float scissor = scissorMask(pass_vertex);



	float strokeAlpha = strokeMask();
	if (strokeAlpha < strokeThr) {
		discard;
	}

	if (type == 0) { // Gradient
		vec2 pt = (state.paintMat * vec3(pass_vertex, 1.0)).xy;
		float d = clamp((sdroundrect(pt, state.extent, radius) + feather * 0.5) / feather, 0.0, 1.0);
		vec4 colour = mix(state.innerCol, state.outerCol, d);
		colour *= strokeAlpha * scissor;
		out_Colour = colour;
	} else if (type == 1) { // Image
		vec2 pt = (state.paintMat * vec3(pass_vertex, 1.0)).xy / state.extent;
		vec4 colour = texture(sampler2D(tex, texsampler), pt);
		if (texType == 1) {
			colour = vec4(colour.xyz * colour.w, colour.w);
		} else if (texType == 2) {
			colour = vec4(colour.x);
		}
		colour *= state.innerCol;
		colour *= strokeAlpha * scissor;
		out_Colour = colour;
	} else if (type == 2) { // Stencil Fill
		out_Colour = vec4(1, 1, 1, 1);
	} else if (type == 3) { // Textured Tris
		vec4 colour = texture(sampler2D(tex, texsampler), pass_tcoord);
		if (texType == 1) {
			colour = vec4(colour.xyz * colour.w, colour.w);
		} else if (texType == 2) {
			colour = vec4(colour.x);
		}
		colour *= scissor;
		out_Colour = colour * state.innerCol;
	}
}";
    }
}
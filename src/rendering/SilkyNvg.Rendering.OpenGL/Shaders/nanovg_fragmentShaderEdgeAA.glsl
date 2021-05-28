#version 150 core

in vec2 pass_vertex;
in vec2 pass_tcoord;
in vec2 pass_colour;

out vec4 out_Colour;

uniform mat3 scissorMat;
uniform vec2 scissorExt;
uniform vec2 scissorScale;
uniform mat3 paintMat;
uniform vec2 extent;
uniform float radius;
uniform float feather;
uniform vec4 innerCol;
uniform vec4 outerCol;
uniform float strokeMult;

uniform int texType;
uniform int type;

uniform sampler2D tex;

float sdroundrect(vec2 pt, vec2 ext, float rad) {
	vec2 ext2 = vec2(rad, rad);
	vec2 d = abs(pt) - ext2;
	return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rad;
}

float scissorMask(vec2 p) {
	vec2 sc = (abs((scissorMat * vec3(p, 1.0)).xy) - scissorExt);
	sc = vec2(0.5, 0.5) - sc * scissorScale;
	return clamp(sc.x, 0.0, 1.0) * clamp(sc.y, 0.0, 1.0);
}

float strokeMask() {
	return min(1.0, (1.0 - abs(pass_tcoord.x * 2.0 - 1.0)) * strokeMult) * pass_tcoord.y;
}

void main(void) {
	if (type == 0) { // Gradient
		float scissor = scissorMask(pass_vertex);
		float strokeAlpha = strokeMask();

		vec2 pt = (paintMat * vec3(pass_vertex, 1.0)).xy;
		float d = clamp((sdroundrect(pt, extent, radius) + feather * 0.5) / feather, 0.0, 1.0);
		vec4 colour = mix(innerCol, outerCol, d);

		colour.w *= strokeAlpha * scissor;
		out_Colour = colour;
	} else if (type == 1) { // Image
		out_Colour = vec4(0, 0, 0, 0);
	} else if (type == 2) { // Stencil Fill
		out_Colour = vec4(1, 1, 1, 1);
	} else if (type == 3) { // Textured Tris
		out_Colour = texture(tex, pass_tcoord);
	}
}
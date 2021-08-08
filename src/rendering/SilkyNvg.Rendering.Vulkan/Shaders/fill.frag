#version 400
#extension GL_ARB_separate_shader_objects  : enable
#extension GL_ARB_shading_language_420pack : enable

layout (location = 0) in vec2 pass_vertex;
layout (location = 1) in vec2 pass_tcoord;

layout (location = 0) out vec4 out_Colour;

void main(void) {
	out_Colour = vec4(1.0, 1.0, 1.0, 1.0);
}
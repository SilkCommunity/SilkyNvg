#version 150 core

in vec2 vertex;
in vec2 tcoord;
in vec2 colour;

out vec2 pass_vertex;
out vec2 pass_tcoord;
out vec2 pass_colour;

uniform vec2 viewSize;

void main(void) {
	pass_vertex = vertex;
	pass_tcoord = tcoord;
	pass_colour = colour;
	gl_Position = vec4(2.0 * vertex.x / viewSize.x - 1.0, 1.0 - 2.0 * vertex.y / viewSize.y, 0, 1);
}
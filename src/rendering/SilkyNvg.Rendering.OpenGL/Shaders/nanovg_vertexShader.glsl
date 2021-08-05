#version 150 core

in vec2 vertex;
in vec2 tcoord;

out vec2 pass_vertex;
out vec2 pass_tcoord;

uniform vec2 viewSize;

void main(void) {
	pass_vertex = vertex;
	pass_tcoord = tcoord;
	gl_Position = vec4(2.0 * vertex.x / viewSize.x - 1.0, 1.0 - 2.0 * vertex.y / viewSize.y, 0, 1);
}
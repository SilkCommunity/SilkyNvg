attribute vec2 vertex;
attribute vec2 tcoord;

varying vec2 pass_tcoord;
varying vec2 pass_pos;

uniform vec2 viewSize;

void main(void) {
	pass_tcoord = tcoord;
	pass_pos = vertex;
	gl_Position = vec4(2.0 * vertex.x / viewSize.x - 1.0, 1.0 - 2.0 * vertex.y / viewSize.y, 0, 1);
}
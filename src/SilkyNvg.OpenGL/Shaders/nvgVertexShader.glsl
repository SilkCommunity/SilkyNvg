#version 150 core

in vec2 vertex;
in vec2 textureCoord;

out vec2 parse_textureCoord;
out vec2 parse_vertex;

uniform vec2 viewSize;

void main(void) {
    parse_vertex = vertex;
    parse_textureCoord = textureCoord;
    gl_Position = vec4(2.0 * vertex.x / viewSize.x - 1.0, 1.0 - 2.0 * vertex.y / viewSize.y, 0, 1.0);
}
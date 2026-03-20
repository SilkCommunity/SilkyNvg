#version 420 core

layout(location = 0) in vec2 vertex;

uniform vec2 viewSize;

void main(void) {
    gl_Position = vec4(2.0 * vertex.x / viewSize.x - 1.0, 1.0 - 2.0 * vertex.y / viewSize.y, 0.0, 1.0);
}
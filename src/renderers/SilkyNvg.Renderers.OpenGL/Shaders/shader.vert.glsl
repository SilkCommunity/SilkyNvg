#version 330 core

layout (location = 0) in vec2 vert;

uniform vec2 viewExtent;

void main() {
    gl_Position = vec4(2.0 * vert.x / viewExtent.x - 1, 1.0 - 2.0 * vert.y / viewExtent.y, 0.0, 1.0);
}
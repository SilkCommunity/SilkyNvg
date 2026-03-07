#version 330 core

layout (location = 0) in vec2 vert;

void main() {
    gl_Position = vec4(vert.x, vert.y, 0.0, 1.0);
}
#version 430 core

layout(location = 0) in vec2 vert;
layout(location = 1) in vec2 texCoord;

out vec2 pass_texCoord;

void main() {
    pass_texCoord = texCoord;
    gl_Position = vec4(vert, 0.0, 1.0);
}
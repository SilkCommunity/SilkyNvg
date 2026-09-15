#version 430 core

layout(location = 0) in vec2 pos;

layout(location = 0) out vec2 pass_position;

void main(void) {
    gl_Position = vec4(pos, 0.0, 1.0);
    pass_position = pos;
}

#version 330 core

layout(location = 0) in vec2 pos;
layout(location = 1) in vec3 klm;
layout(location = 2) in uint flags;

out vec3 pass_klm;

uniform vec2 viewSize;

void main(void) {
    gl_Position = vec4(0.2 * vec2(2.0 * pos.x / viewSize.x - 1.0, -1.0 + 2.0 * pos.y / viewSize.y), 0.0, 1.0);
    pass_klm = klm;
}
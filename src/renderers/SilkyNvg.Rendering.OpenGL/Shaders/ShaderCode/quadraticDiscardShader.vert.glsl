#version 420 core

layout(location = 0) in vec2 vertex;
layout(location = 1) in uint canonicalFormIdx;

layout(location = 0) out vec2 implicitCoord;

uniform vec2 viewSize;

const vec2 canonicalImplicitForm[3] = vec2[](
    vec2(0.0, 0.0),
    vec2(0.5, 0.0),
    vec2(1.0, 1.0)
);

void main(void) {
    implicitCoord = canonicalImplicitForm[gl_VertexID - 1];
    gl_Position = vec4(2.0 * vertex.x / viewSize.x - 1.0, 1.0 - 2.0 * vertex.y / viewSize.y, 0.0, 1.0);
}
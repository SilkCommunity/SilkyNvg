#version 460 core

layout(location = 0) out vec2 implicitCoord;

layout(std430, binding = 0) readonly buffer PointBuffer {
    vec2 points[];
};

uniform vec2 viewSize;

const vec2 canonicalFormPositions[3] = vec2[](
    vec2(0.0, 0.0),
    vec2(0.5, 0.0),
    vec2(1.0, 1.0)
);

void main(void) {
    uint index = gl_VertexID;

    uint pointIdx = index & 0x00FFFFFF;
    uint canonicalFormPositionIdx = (index & 0xFF000000) >> 24;

    implicitCoord = canonicalFormPositions[canonicalFormPositionIdx];

    vec2 vertex = points[pointIdx];
    gl_Position = vec4(2.0 * vertex.x / viewSize.x - 1.0, 1.0 - 2.0 * vertex.y / viewSize.y, 0.0, 1.0);
}
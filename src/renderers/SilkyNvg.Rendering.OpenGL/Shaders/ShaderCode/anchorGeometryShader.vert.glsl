#version 460 core

layout(std430, binding = 0) readonly buffer PointBuffer {
    vec2 points[];
};

uniform vec2 viewSize;

void main(void) {
    vec2 vertex = points[gl_VertexID];
    gl_Position = vec4(2.0 * vertex.x / viewSize.x - 1.0, 1.0 - 2.0 * vertex.y / viewSize.y, 0.0, 1.0);
}
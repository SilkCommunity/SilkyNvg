#version 330 core

vec2 positions[4] = vec2[](
    vec2(0.5, 0.5),
    vec2(0.5, -0.5),
    vec2(-0.5, -0.5),
    vec2(-0.5, 0.5)
);

uvec2 boundPositions[4] = uvec2[](
    uvec2(2, 1),
    uvec2(2, 3),
    uvec2(0, 3),
    uvec2(0, 1)
);

uniform vec4 pathBounds;
uniform vec2 viewSize;

void main(void) {
    uvec2 positions = boundPositions[gl_VertexID];
    gl_Position = vec4(2.0 * pathBounds[positions.x] / viewSize.x - 1.0, 1.0 - 2.0 * pathBounds[positions.y] / viewSize.y, 0.0, 1.0);
}
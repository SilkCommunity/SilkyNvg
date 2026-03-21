#version 460 core

uvec2 boundPositions[4] = uvec2[](
    uvec2(0, 1),
    uvec2(0, 3),
    uvec2(2, 1),
    uvec2(2, 3)
);

uniform vec4 pathBounds;
uniform vec2 viewSize;

void main(void) {
    uvec2 positions = boundPositions[gl_VertexID];
    gl_Position = vec4(2.0 * pathBounds[positions.x] / viewSize.x - 1.0, 1.0 - 2.0 * pathBounds[positions.y] / viewSize.y, 0.0, 1.0);
}
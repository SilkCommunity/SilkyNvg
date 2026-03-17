#version 450

layout(set = 0, binding = 0) uniform ViewSize {
    vec4 viewSize; // xy = viewport size, z = Y clip-space multiplier (+1 or -1), w = unused
};

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 TexCoord;
layout(location = 2) in vec4 Color;

layout(location = 0) out vec2 frag_WorldPosition;
layout(location = 1) out vec2 frag_TexCoord;

void main() {
    frag_WorldPosition = Position;
    frag_TexCoord = TexCoord;
    gl_Position = vec4(2.0 * Position.x / viewSize.x - 1.0, (1.0 - 2.0 * Position.y / viewSize.y) * viewSize.z, 0.0, 1.0);
}
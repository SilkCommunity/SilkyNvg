#version 430 core

layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout(rgba32f, binding = 0) uniform image2D imgOutput;

layout(location = 0) uniform float t;

void main() {
    vec4 value = vec4(0.0, 0.0, 0.0, 1.0);
    ivec2 texelCoord = ivec2(gl_GlobalInvocationID.xy);

    const float speed = 100.0;
    const float width = 1000.0;

    value.x = (sin(t) + 1) / 2.0;
    value.y = float(texelCoord.y) / gl_NumWorkGroups.y;

    imageStore(imgOutput, texelCoord, vec(0,0));
}
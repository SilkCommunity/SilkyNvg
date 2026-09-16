#version 430 core

layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout (rgba32f, binding = 0) uniform image2D img_output;

void main() {
    vec4 pixel = vec4(0.0, 0.0, 0.0, 1.0);
    ivec2 texelCoord = ivec2(gl_GlobalInvocationID.xy);

    pixel.x = float(texelCoord.x) / (gl_NumWorkGroups.x);
    pixel.y = float(texelCoord.y) / (gl_NumWorkGroups.y);

    imageStore(img_output, texelCoord, pixel);
}
#version 460 core

layout(local_size_x=1, local_size_y=1, local_size_z=1) in;

layout(std430, binding=1) readonly buffer input_0_buffer {
    uint input_0[];
};

layout(std430, binding=2) readonly buffer input_1_buffer {
    uint input_1[];
};

layout(std430, binding=3) writeonly buffer output_buffer {
    uint data[];
};

uniform uint factor;

void main() {
    uint index = gl_GlobalInvocationID.x;
    data[index] = input_0[index] * input_1[index] * factor;
}
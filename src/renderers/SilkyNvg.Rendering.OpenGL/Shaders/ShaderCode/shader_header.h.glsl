#version 430 core

#define WORKGROUP_SIZE 256

layout(local_size_x = WORKGROUP_SIZE, local_size_y = 1, local_size_z = 1) in;

struct SegmentData {
    uint vertexIndex;
    int reversedType;
    float arcW1;
    float offset;
};

layout(binding = 0, std430) readonly buffer vertices {
    vec2 positions[];
};

layout(binding = 1, std430) readonly buffer segments {
    SegmentData segmentData[];
};
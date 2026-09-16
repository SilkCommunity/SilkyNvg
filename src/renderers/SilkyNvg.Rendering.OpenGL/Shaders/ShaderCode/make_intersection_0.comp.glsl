#version 430 core

layout(local_size_x = 256, local_size_y = 1, local_size_z = 1) in;

struct SegmentData {
    uint vertexIndex;
    int reversedType;
    float arcW1;
    float offset;
};

struct SegmentMonotonicCutpoints {
    float ts[4];
    int n_cuts;
};

layout(binding = 0, std430) readonly buffer vertices {
    vec2 positions[];
};

layout(binding = 1, std430) readonly buffer segments {
    SegmentData segmentData[];
};

layout(binding = 4, std430) writeonly buffer curve_pixel_count {
    int pcnt[];
};

layout(binding = 5, std430) writeonly buffer monotonic_cutpoint_cache {
    SegmentMonotonicCutpoints monotoneData[];
};

uniform uint nSegments;

void main() {

}
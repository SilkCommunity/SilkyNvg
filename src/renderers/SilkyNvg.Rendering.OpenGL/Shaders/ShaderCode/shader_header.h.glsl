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

/*
 * NOTE: This is another optimization to reduce nesting in the shader:
 *  The least significant three bits associated to each SegmentType are equal
 *  to the number of control points taken.
 *  Rational is also a cubic Bezier, however we add another flag in the fourth bit to mark it different from Quadratic.
 */
#define SEG_TYPE_LINEAR 2
#define SEG_TYPE_QUADRATIC 3
#define SEG_TYPE_CUBIC 4
#define SEG_TYPE_RATIONAL 11

#define SEG_TYPE_CONTROL_POINT_COUNT_MASK 7

int seg_type(uint segmentIndex) {
    return segmentData[segmentIndex].reversedType & 0x000000FF;
}

bool seg_reversed(uint segmentIndex) {
    return (segmentData[segmentIndex].reversedType & 0x0000FF00) != 0;
}

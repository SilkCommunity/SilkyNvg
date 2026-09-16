struct SegmentMonotonicCutpoints {
    float ts[4];
    uint n_cuts;
};

layout(binding = 4, std430) writeonly buffer curve_pixel_count {
    int pcnt[];
};

layout(binding = 5, std430) writeonly buffer monotonic_cutpoint_cache {
    SegmentMonotonicCutpoints monotoneData[];
};

uniform uint nSegments;

void main() {
    
    // We are running one kernel per segment
    uint threadID = gl_GlobalInvocationID.x;
    if (threadID >= nSegments) {
        return;
    }

    uint segmentIndex = threadID;

    uint i = threadID;
    uint i0 = i;
    uint p0 = 0;
    uint mode = 0;

    float arcw1 = 0.0;

    // Read info about this segment
    p0 = segmentData[segmentIndex].vertexIndex;

    monotoneData[segmentIndex].ts[0] = segmentData[0].arcW1;
    monotoneData[segmentIndex].ts[1] = 1.0;
    monotoneData[segmentIndex].ts[2] = 2.0;
    monotoneData[segmentIndex].ts[3] = 3.0;
    monotoneData[segmentIndex].n_cuts = segmentData[segmentIndex].vertexIndex;

}
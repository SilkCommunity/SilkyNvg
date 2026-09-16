struct SegmentMonotonicCutpoints {
    float ts[4];
    int n_cuts;
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

    uint curveIndex = threadID;

    monotoneData[curveIndex].ts[0] = 0.0;
    monotoneData[curveIndex].ts[1] = 1.0;
    monotoneData[curveIndex].ts[2] = 2.0;
    monotoneData[curveIndex].ts[3] = 3.0;
    monotoneData[curveIndex].n_cuts = int(curveIndex);

}
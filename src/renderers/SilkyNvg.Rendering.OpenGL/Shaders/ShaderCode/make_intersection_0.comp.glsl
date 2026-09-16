struct SegmentMonotonicCutpoints {
    float ts[4];
    int nCuts;
};

layout(binding = 4, std430) writeonly buffer curve_pixel_count {
    int pcnt[];
};

layout(binding = 5, std430) writeonly buffer monotonic_cutpoint_cache {
    SegmentMonotonicCutpoints monotoneData[];
};

// Uniform variables
uniform uint n_segments;
uniform vec2 output_size;

/*
 * This kernel finds the points where either the x or y slope change sign.
 * Notice there are at most 4 such points per segment of type (Linear, Quadratic, Cubic, Rational) Bezier.
 * The following is about 16 kB of shared memory. Minimum required by spec. in OpenGL is 32 kB so we should be fine.
 */
shared float shared_t1_queue[5 * WORKGROUP_SIZE];
shared float shared_point_coords[10 * WORKGROUP_SIZE];

// Access into the shared_* variables.
// Each kernel gets 5 t1_queue floats and 10 point_coords floats
//  where point_coords 8 and 9 are temporary slots for merge sorting.
uint t1_queue_index;
uint point_coords_index;

// Observe the non-conventional ordering!
#define T1_QUEUE(i) shared_t1_queue[WORKGROUP_SIZE * t1_queue_index + i]
#define POINT_COORDS(i) shared_point_coords[WORKGROUP_SIZE * point_coords_index + i]

void makeIntersections(int segmentType, uint segIdx, float arcw1) {
    vec2 v0 = vec2(POINT_COORDS(0), POINT_COORDS(1));

    int p = 0;

    // TODO: Monotonize

    pcnt[segIdx] = p;
}

void main() {
    
    // We are running one kernel per segment
    uint threadID = gl_GlobalInvocationID.x;
    if (threadID >= n_segments) {
        return;
    }

    uint segmentIndex = threadID;

    uint i = threadID;
    uint i0 = i;
    uint p0 = 0;
    int mode = 0;

    t1_queue_index = gl_LocalInvocationID.x;
    point_coords_index = gl_LocalInvocationID.x;
    float arcw1 = 0.0;

    // Read info about this segment
    p0 = segmentData[segmentIndex].vertexIndex;
    mode = seg_type(segmentIndex);

    if (mode == SEG_TYPE_RATIONAL) {
        arcw1 = segmentData[segmentIndex].arcW1;
    }

    // load the vertices
    int controlPointCount = mode & SEG_TYPE_CONTROL_POINT_COUNT_MASK;
    #pragma unroll
    for (int i = 0; i < 4; i++) {
        if (i < controlPointCount) {
            vec2 cpi = positions[p0 + i];
            POINT_COORDS(2 * i + 0) = cpi.x;
            POINT_COORDS(2 * i + 1) = cpi.y;
        }
    }

    // compute the monotonic segments
    int nCuts = 0;
    bool isVisible = true;  // TODO: Check visibility!

    float q0 = 0.0, q1 = 0.0, q2 = 0.0, q3 = 0.0;
    if (isVisible) {
        switch (mode) {
            case SEG_TYPE_LINEAR:
                // is already monotone
                break;
            case SEG_TYPE_QUADRATIC:
                // TODO
                break;
            case SEG_TYPE_CUBIC:
                // TODO
                break;
            case SEG_TYPE_RATIONAL:
                // TODO
                break;
            default:
                break;
        }
        
        // TODO: Insertion Sort
    }

    // Cache calculated cutpoints in t_cuts
    T1_QUEUE(0) = q0;
    T1_QUEUE(1) = q1;
    T1_QUEUE(2) = q2;
    T1_QUEUE(3) = q3;
    monotoneData[segmentIndex].ts[0] = q0;
    monotoneData[segmentIndex].ts[1] = q1;
    monotoneData[segmentIndex].ts[2] = q2;
    monotoneData[segmentIndex].ts[3] = q3;
    monotoneData[segmentIndex].nCuts = nCuts;

    if (isVisible) {
        T1_QUEUE(nCuts) = 1.0f;
        nCuts++;
    }

    makeIntersections(mode, segmentIndex, arcw1);
}
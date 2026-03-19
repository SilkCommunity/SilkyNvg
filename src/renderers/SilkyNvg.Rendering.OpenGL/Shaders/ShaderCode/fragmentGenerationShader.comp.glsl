#version 460 core

#define COMMAND_TYPE_LINE_TO            0
#define COMMAND_TYPE_QUADRATIC_CURVE_TO 1
#define COMMAND_TYPE_BEZIER_CURVE_TO    2

layout(local_size_x = 32, local_size_y = 1, local_size_z = 1) in;

struct CommandData {
    uint pointIndex;
    uint intersectionsIndex;
    uint nIntX;
    uint nIntY;
    uint pathIndex;
    uint type;
};

struct FragmentData {
    int pixelX;
    int pixelY;
    int windingNumber;
    uint pathID;
};

layout(std430, binding = 0) readonly buffer VertexBuffer {
    vec2 vertices[];
};

layout(std430, binding = 1) readonly buffer CommandBuffer {
    CommandData commands[];
};

layout(std430, binding = 4) readonly buffer IntersectionsBuffer {
    uvec2 intersections[];
};

layout(std430, binding = 5) writeonly buffer FragmentBuffer {
    uint emptyFragmentCount;
    FragmentData data[];
};

uniform uint intersectionCount;
uniform uvec2 screenDimensions;

vec2 interpolateCommand(uint type, float t, vec2 p0, vec2 p1, vec2 p2, vec2 p3) {
    vec2 q1, q2, q3;
    vec2 r1, r2;
    vec2 p;
    switch (type) {
        case COMMAND_TYPE_LINE_TO:
            p = mix(p0, p1, t);
            break;
        case COMMAND_TYPE_QUADRATIC_CURVE_TO:
            q1 = mix(p0, p1, t);
            q2 = mix(p1, p2, t);
            p = mix(q1, q2, t);
            break;
        case COMMAND_TYPE_BEZIER_CURVE_TO:
            q1 = mix(p0, p1, t);
            q2 = mix(p1, p2, t);
            q3 = mix(p2, p3, t);
            r1 = mix(q1, q2, t);
            r2 = mix(q2, q3, t);
            p = mix(r1, r2, t);
            break;
    }
    return p;
}

void main() {
    uint intersectionIndex = gl_GlobalInvocationID.x;

    // We consider as fragments the area in between the intersections.
    // Thus there are intersectionCount - 1 fragments to be generated.
    if (intersectionIndex >= intersectionCount - 1) {
        return;
    }

    uvec2 intersection0 = intersections[intersectionIndex];
    uvec2 intersection1 = intersections[intersectionIndex + 1];

    float t0 = uintBitsToFloat(intersection0.x);
    float t1 = uintBitsToFloat(intersection1.x);

    uint cmdId0 = intersection0.y;
    uint cmdId1 = intersection1.y;

    // Commands are defined back-to-back on closed curves.
    // Therefore we can ignore segments not defined on the same command.
    if (cmdId0 != cmdId1) {
        atomicAdd(emptyFragmentCount, 1);
        return;
    }

    CommandData cmd = commands[cmdId0];
    uint pathID = cmd.pathIndex;

    vec2 p0, p1, p2, p3;
    switch (cmd.type) {
        case COMMAND_TYPE_LINE_TO:
            p0 = vertices[cmd.pointIndex - 1];
            p1 = vertices[cmd.pointIndex + 0];
            break;
        case COMMAND_TYPE_QUADRATIC_CURVE_TO:
            p0 = vertices[cmd.pointIndex - 1];
            p1 = vertices[cmd.pointIndex + 0];
            p2 = vertices[cmd.pointIndex + 1];
            break;
        case COMMAND_TYPE_BEZIER_CURVE_TO:
            p0 = vertices[cmd.pointIndex - 1];
            p1 = vertices[cmd.pointIndex + 0];
            p2 = vertices[cmd.pointIndex + 1];
            p3 = vertices[cmd.pointIndex + 2];
            break;
    }

    if (t0 < t1) {
        vec2 v0 = interpolateCommand(cmd.type, t0, p0, p1, p2, p3);
        vec2 v1 = interpolateCommand(cmd.type, t1, p0, p1, p2, p3);

        // Define fragment pixel coordinate by the pixel coordinate in-between the two fragment intersections.
        vec2 centre = 0.5 * v0 + 0.5 * v1;

        data[intersectionIndex].pixelX = int(floor(centre.x));
        data[intersectionIndex].pixelY = int(floor(centre.y));

        // Let right-to-left be positive, left-to-right be negative
        if (v0.y < v1.y) { // Left-to-right crossing <=> line goes down
            data[intersectionIndex].windingNumber = -1;
        } else if (v1.y < v0.y) { // Right-to-left crossing <=> line goes up
            data[intersectionIndex].windingNumber = 1;
        } else { // Line is parallel to ray, no intersection (even if they are congruent on the fragment)
            data[intersectionIndex].windingNumber = 0;
        }

        // Save path ID for paint
        data[intersectionIndex].pathID = pathID;
    } else {
        atomicAdd(emptyFragmentCount, 1);
    }
}
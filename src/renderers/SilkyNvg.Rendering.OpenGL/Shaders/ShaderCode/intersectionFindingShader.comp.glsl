#version 460 core

#define COMMAND_TYPE_LINE_TO            0
#define COMMAND_TYPE_QUADRATIC_CURVE_TO 1
#define COMMAND_TYPE_BEZIER_CURVE_TO    2

#define CUBIC_ITERATION_COUNT 24

layout(local_size_x=32, local_size_y=1, local_size_z=1) in;

struct CommandData {
    uint pointIndex;
    uint intersectionsIndex;
    uint nIntX;
    uint nIntY;
    uint pathIndex;
    uint type;
};

layout(std430, binding=0) readonly buffer VertexBuffer {
    vec2 vertices[];
};

layout(std430, binding=1) readonly buffer CommandBuffer {
    CommandData commands[];
};

layout(std430, binding=4) buffer IntersectionsBuffer {
    uvec2 intersectionsTimes[];
};

uniform float fpTol;

uint intersectionCount = 0;
void addTime(float t, uint curveIndex, uint intersectionsIndex) {
    uint i = intersectionCount;
    while (i > 0) {
        if (uintBitsToFloat(intersectionsTimes[intersectionsIndex + i - 1].x) <= t)
            break;
        intersectionsTimes[intersectionsIndex + i] = intersectionsTimes[intersectionsIndex + i - 1]; // Sufficient space is guaranteed to be allocated
        i--;
    }

    intersectionsTimes[intersectionsIndex + i] = uvec2(floatBitsToUint(t), curveIndex);
    intersectionCount++;
}

// f(t) = at + b
float solveLinear(float a, float b) {
    // If the line is without slope, don't add.
    // Start and end points are added anyways.
    return (abs(a) < fpTol) ? -1 : -b / a;
}

// f(t) = at^2 + bt + c
float solveQuadratic(float a, float b, float c) {
    float t1, t2;

    // Degenerates to a line
    if (abs(a) < fpTol) {
        return solveLinear(b, c);
    }

    float discriminant = b * b - 4 * a * c;
    if (discriminant < 0)
    {
        t1 = t2 = -1;
    }
    else if (discriminant == 0)
    {
        t1 = -b / (2 * a); // Note a != 0 here
        t2 = -1;
    }
    
    float sqrtOfDiscriminant = sqrt(discriminant);
    if (b >= 0)
    {
        t1 = (-b - sqrtOfDiscriminant) / (2 * a);
        t2 = (2 * c) / (-b - sqrtOfDiscriminant);
    }
    else
    {
        t1 = (2 * c) / (-b + sqrtOfDiscriminant);
        t2 = (-b + sqrtOfDiscriminant) / (2 * a);
    }


    // Note there can only be one solution, since the segments are monotonic
    if (t1 > 0 && t1 < 1.0)
        return t1;
    else if (t2 > 0 && t2 < 1.0)
        return t2;
    else
        return -1;
}

float interpolateCubic(float p0, float p1, float p2, float p3, float t) {
    float q1 = mix(p0, p1, t);
    float q2 = mix(p1, p2, t);
    float q3 = mix(p2, p3, t);

    float r1 = mix(q1, q2, t);
    float r2 = mix(q2, q3, t);

    return mix(r1, r2, t);
}

float bisectCubic(float p0, float p1, float p2, float p3, float x) {
    float t0 = 0.0, v0 = p0 - x;
    float t1 = 1.0, v1 = p3 - x;
    float m, vm;
    for (uint iter = 0; iter < CUBIC_ITERATION_COUNT; iter++) {
        // If p0 = p3 the segment is not strictly monotonic but constant
        // Thus we just return -1
        // t=0 and t=1 are added anyway.
        if (abs(v0 - v1) < fpTol)
            return -1;
        
        m = (t0 + t1) / 2.0;
        vm = interpolateCubic(p0, p1, p2, p3, m) - x; // Don't calculate using polynomial expression because that's slow

        if (abs(vm) < fpTol)
            return m;

        if (v0 * vm < 0) { // root lies in [t0, m]
            t1 = m;
            v1 = vm;
        }
        else if (v1 * vm < 0) { // root lies in [m, t1]
            t0 = m;
            v0 = vm;
        }
        else { // No root, should never occur since segments are monotonic
            return -1;
        }
    }
    // Return m, even if the precission we want has not been achieved.
    return m;
}

void main() {
    uint commandIndex = gl_GlobalInvocationID.x;

    if (commandIndex >= commands.length()) {
        return;
    }

    CommandData command = commands[commandIndex];

    vec2 p0, p1, p2, p3;
    vec2 a, b, c, d;
    vec2 topLeft;
    float currentGridLineX, currentGridLineY;
    float tx, ty;

    addTime(0.0, commandIndex, command.intersectionsIndex);
    switch (command.type) {
        case COMMAND_TYPE_LINE_TO:
            p0 = vertices[command.pointIndex - 1];
            p1 = vertices[command.pointIndex + 0];

            a = p1 - p0;
            b = p0;

            // Segment is monotonic. Therefore min coordinates are either p0 or p1 respectively.
            topLeft = floor(min(p0, p1));

            for (int i = 1; i <= command.nIntX; i++) {
                currentGridLineX = topLeft.x + i;
                tx = solveLinear(a.x, b.x - currentGridLineX);
                if (tx > 0.0 && tx < 1.0)
                    addTime(tx, commandIndex, command.intersectionsIndex);
            }
            for (int i = 1; i <= command.nIntY; i++) {
                currentGridLineY = topLeft.y + i;
                ty = solveLinear(a.y, b.y - currentGridLineY);
                if (ty > 0.0 && ty < 1.0)
                    addTime(ty, commandIndex, command.intersectionsIndex);
            }
            break;
        case COMMAND_TYPE_QUADRATIC_CURVE_TO:
            p0 = vertices[command.pointIndex - 1];
            p1 = vertices[command.pointIndex + 0];
            p2 = vertices[command.pointIndex + 1];

            a = p0 - 2 * p1 + p2;
            b = 2 * (-p0 + p1);
            c = p0;

            // Segment is monotonic. Therefore min coordinates are either p0 or p2 respectively.
            topLeft = floor(min(p0, p2));

            for (int i = 1; i <= command.nIntX; i++) {
                currentGridLineX = topLeft.x + i;
                tx = solveQuadratic(a.x, b.x, c.x - currentGridLineX);
                if (tx > 0.0 && tx < 1.0)
                    addTime(tx, commandIndex, command.intersectionsIndex);
            }
            for (int i = 1; i <= command.nIntY; i++) {
                currentGridLineY = topLeft.y + i;
                ty = solveQuadratic(a.y, b.y, c.y - currentGridLineY);
                if (ty > 0.0 && ty < 1.0)
                    addTime(ty, commandIndex, command.intersectionsIndex);
            }
            break;
        case COMMAND_TYPE_BEZIER_CURVE_TO:
            p0 = vertices[command.pointIndex - 1];
            p1 = vertices[command.pointIndex + 0];
            p2 = vertices[command.pointIndex + 1];
            p3 = vertices[command.pointIndex + 2];

            // Segment is monotonic. Therefore min coordinates are either p0 or p3 respectively.
            topLeft = floor(min(p0, p3));

            for (int i = 1; i <= command.nIntX; i++) {
                currentGridLineX = topLeft.x + i;
                tx = bisectCubic(p0.x, p1.x, p2.x, p3.x, currentGridLineX);
                if (tx > 0.0 && tx < 1.0)
                    addTime(tx, commandIndex, command.intersectionsIndex);
            }
            for (int i = 1; i <= command.nIntY; i++) {
                currentGridLineY = topLeft.y + i;
                ty = bisectCubic(p0.y, p1.y, p2.y, p3.y, currentGridLineY);
                if (ty > 0.0 && ty < 1.0)
                    addTime(ty, commandIndex, command.intersectionsIndex);
            }
            break;
    }
    addTime(1.0, commandIndex, command.intersectionsIndex);
}
#version 460 core

layout(local_size_x = 32, local_size_y = 1, local_size_z = 1) in;

struct FragmentData {
    int pixelX;
    int pixelY;
    int deltaWindingNumber;
    uint pathID;
};

layout(std430, binding = 5) buffer FragmentBuffer {
    uint emptyFragmentCount;
    FragmentData fragments[];
};

uniform uint fragmentCount;
uniform uint stride;
uniform uint strideTrailingZeros;
uniform uint innerReminder;
uniform uint innerLastIdx;

bool isLeftIndex(uint i) {
    uint innerIndex = i >> strideTrailingZeros;
    return (innerIndex & 1) == innerReminder && (innerIndex & innerLastIdx) < innerLastIdx;
}

uint getRightIndex(uint i) {
    return i + stride;
}

bool geq(FragmentData a, FragmentData b) {
    return a.pathID > b.pathID || (a.pathID == b.pathID && a.pixelY > b.pixelY) || (a.pathID == b.pathID && a.pixelY == b.pixelY && a.pixelX > b.pixelX);
}

void compareAndSwap(uint i, uint j) {
    if (j < fragmentCount) {
        if (geq(fragments[i], fragments[j])) {
            FragmentData temp = fragments[i];
            fragments[i] = fragments[j];
            fragments[j] = temp;
        }
    }
}

void main() {
    uint i = gl_GlobalInvocationID.x;

    if (i >= fragmentCount) {
        return;
    }

    if (isLeftIndex(i)) {
        uint j = getRightIndex(i);
        compareAndSwap(i, j);
    }
}
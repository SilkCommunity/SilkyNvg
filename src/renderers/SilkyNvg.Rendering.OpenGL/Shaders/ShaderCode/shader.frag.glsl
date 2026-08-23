#version 330 core

#define FALSE 0
#define TRUE 1

#define GEOMETRY_TYPE_LINEAR 0
#define GEOMETRY_TYPE_RATIONAL_QUADRATIC 1
#define GEOMETRY_TYPE_CUBIC 2

// Parenthesis are necessary! Otherwise we get, for example: (flags & (1 << 0)) | (1 << 1)
#define MASK_GEOMETRY_TYPE ((1 << 0) | (1 << 1))
#define MASK_STENCIL (1 << 2)

in vec3 pass_klm;
flat in int pass_flags;

out vec4 out_color;

void stencil(int geometryType) {
    float barrycentric_test;

    if (geometryType == GEOMETRY_TYPE_RATIONAL_QUADRATIC) {
        barrycentric_test = pow(pass_klm.x, 2) - pass_klm.y * pass_klm.z;
        if (barrycentric_test > 0.0) {
            discard;
        }
    } else if (geometryType == GEOMETRY_TYPE_CUBIC) {
        barrycentric_test = pow(pass_klm.x, 3) - pass_klm.y * pass_klm.z;
        if (barrycentric_test > 0.0) {
            discard;
        }
    } else if (geometryType == -1) {   // ellipse geometry
        barrycentric_test = pow(pass_klm.x, 2) + pow(pass_klm.y, 2);
        if (barrycentric_test > 1) {
            discard;
        }
    } else {
        return;
    }
}

void cover(void) {
    out_color = vec4(1.0, 1.0, 1.0, 1.0);
}

int getGeometryType() {
    return pass_flags & MASK_GEOMETRY_TYPE;
}

bool getStencil() {
    int truthValue = (pass_flags & MASK_STENCIL) >> 2;
    return truthValue == TRUE;
}

void main(void) {
    bool stencilValue = getStencil();
    int geometryTypeValue = getGeometryType();

    if (stencilValue) {    // stencil pass
        stencil(geometryTypeValue);
    } else {    // rendering (very) conservative conver geometry (axis-aligned bounding rectangle)
        cover();
    }
}
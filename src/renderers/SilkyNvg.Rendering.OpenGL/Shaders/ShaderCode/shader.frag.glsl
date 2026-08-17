#version 330 core

#define FLAG_GEOMETRY_TYPE 1 << 0
#define FLAG_STENCIL_PASS 1 << 1

in vec3 pass_klm;
flat in int pass_flags;

out vec4 out_color;

void stencil(void) {
    float barrycentric_test;

    if ((pass_flags & FLAG_GEOMETRY_TYPE) == 0) {    // polynomial geometry
        barrycentric_test = pow(pass_klm.x, 3) - pass_klm.y * pass_klm.z;
        if (barrycentric_test > 0.0) {
            discard;
        }

    } else if ((pass_flags & FLAG_GEOMETRY_TYPE) != 0) {   // ellipse geometry
        barrycentric_test = pow(pass_klm.x, 2) + pow(pass_klm.y, 2);
        if (barrycentric_test > 1) {
            discard;
        }
    }
    else {  // failed to classify. Render in red!
        out_color = vec4(1.0, 0.0, 0.0, 1.0);
        return;
    }

    out_color = vec4(1.0, 1.0, 1.0, 1.0);
}

void cover(void) {
    out_color = vec4(0.0, 0.0, 1.0, 1.0);
}

void main(void) {
    if ((pass_flags & FLAG_STENCIL_PASS) != 0) {    // stencil pass
        stencil();
    } else {    // rendering (very) conservative conver geometry (axis-aligned bounding rectangle)
        cover();
    }
}
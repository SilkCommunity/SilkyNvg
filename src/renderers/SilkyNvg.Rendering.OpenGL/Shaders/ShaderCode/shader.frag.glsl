#version 330 core

in vec3 pass_klm;

out vec4 out_color;

void main(void) {
    float barrycentric_test = pow(pass_klm.x, 3) - pass_klm.y * pass_klm.z;
    if (barrycentric_test > 0.0) {
        discard;
    }

    out_color = vec4(1.0, 1.0, 1.0, 1.0);
}
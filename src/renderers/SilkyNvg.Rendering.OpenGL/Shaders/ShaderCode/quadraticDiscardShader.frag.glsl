#version 460 core

layout(location = 0) in vec2 implicitCoord;

layout(location = 0) out vec4 out_color;

bool pointInCurve(void) {
    return pow(implicitCoord.x, 2) - implicitCoord.y <= 0;
}

void main(void) {
    if (!pointInCurve()) {
        discard;
    }

    out_color = vec4(1, 1, 1, 1);
}
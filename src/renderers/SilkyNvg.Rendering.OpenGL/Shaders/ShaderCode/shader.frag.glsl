#version 430 core

layout(location = 0) in vec2 pass_position;

out vec4 out_color;

uniform sampler2D textureSampler;

void main(void) {
    out_color = texture(textureSampler, pass_position);
}

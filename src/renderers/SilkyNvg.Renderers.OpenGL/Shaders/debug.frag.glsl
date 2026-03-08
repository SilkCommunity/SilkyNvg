#version 430 core

in vec2 pass_texCoord;

out vec4 colour;

uniform sampler2D tex;

void main() {
    vec3 texCol = texture(tex, pass_texCoord).rgb;
    colour = vec4(texCol, 1.0);
}
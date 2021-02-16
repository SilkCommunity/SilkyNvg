#version 150 core

precision highp float;

in vec2 pass_textureCoord;
in vec2 pass_vertex;

out vec4 out_colour;

uniform mat3 scissorMatrix;
uniform mat3 paintMatrix;
uniform vec4 innerColour;
uniform vec4 outerColour;
uniform vec2 scissorExtent;
uniform vec2 scissorScale;
uniform vec2 extent;
uniform float radius;
uniform float feather;
uniform float strokeMultiplier;
uniform float strokeThreshold;
uniform int texType;
uniform int type;

uniform sampler2D textureSampler;

// a boolean!
uniform int useEdgeAA;

float sdRoundRect(vec2 point, vec2 ext, float rad) {
    vec2 ext2 = ext - vec2(rad, rad);
    vec2 d = abs(point) - ext2;
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rad;
}

float scissorMask(vec2 point) {
    vec2 scissor = (abs((scissorMatrix * vec3(point, 1.0)).xy) - scissorExtent);
    scissor = vec2(0.5, 0.5) - scissor * scissorScale;
    return clamp(scissor.x, 0.0, 1.0) * clamp(scissor.y, 0.0, 1.0);
}

float strokeMask() {
    return min(1.0, (1.0 - abs(pass_textureCoord.x * 2.0 - 1.0)) * strokeMultiplier) * min(1.0, pass_textureCoord.y);
}

void main(void) {
    vec4 outColour;

    float scissor = scissorMask(pass_vertex);

    float strokeAlpha = 1.0f;
    if (useEdgeAA > 0) {
        strokeAlpha = strokeMask();
        if (strokeAlpha < strokeThreshold) {
            discard;
        }
    }

    // Fill gradiant
    if (type == 0) {
        vec2 point = (paintMatrix * vec3(pass_vertex, 1.0)).xy;
        float d = clamp((sdRoundRect(point, extent, radius) + feather * 0.5) / feather, 0.0, 1.0);
        vec4 colour = mix(innerColour, outerColour, d);
        colour *= strokeAlpha * scissor;
        outColour = colour;
    // Fill image
    } else if (type == 1) {
        vec2 point = (paintMatrix * vec3(pass_vertex, 1.0)).xy / extent;
        vec4 colour = texture(textureSampler, point);
        if (texType == 1) {
            colour = vec4(colour.xyz * colour.w, colour.w);
        } else if (texType == 2) {
            colour = vec4(colour.x);
        }
        colour *= innerColour;
        colour *= strokeAlpha * scissor;
        outColour = colour;
    // Simple
    } else if (type == 2) {
        outColour = vec4(1, 1, 1, 1);
    // Image
    } else if (type == 3) {
        vec4 colour = texture(textureSampler, pass_textureCoord);
        if (texType == 1) {
            colour = vec4(colour.xyz * colour.w, colour.w);
        } else if (texType == 2) {
            colour = vec4(colour.x);
        }
        colour *= scissor;
        outColour = colour * innerColour;
    }

    out_colour = outColour;

}
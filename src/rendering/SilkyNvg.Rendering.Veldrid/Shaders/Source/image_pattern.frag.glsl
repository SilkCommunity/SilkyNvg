#version 450

layout(set = 0, binding = 1) uniform ImagePatternParams {
    mat4 paintMat;      // 64 bytes
    vec4 innerCol;      // 16 bytes — tint color
    vec4 outerCol;      // 16 bytes — unused
    vec2 extent;        //  8 bytes — image size for UV normalization
    float radius;       //  4 bytes — unused
    float feather;      //  4 bytes — unused
};

layout(set = 0, binding = 2) uniform texture2D PatternTexture;
layout(set = 0, binding = 3) uniform sampler PatternSampler;

layout(location = 0) in vec2 frag_WorldPosition;
layout(location = 1) in vec2 frag_TexCoord;

layout(location = 0) out vec4 out_Color;

void main() {
    // Transform world position into paint space, normalize to UV by dividing by extent
    vec2 paintSpacePosition = (paintMat * vec4(frag_WorldPosition, 1.0, 1.0)).xy / extent;

    // Sample the pattern texture
    vec4 texColor = texture(sampler2D(PatternTexture, PatternSampler), paintSpacePosition);

    // Apply tint color (innerCol) and AA coverage from fringe vertices
    float fillCoverage = min(1.0, frag_TexCoord.y);
    float strokeCoverage = min(1.0, (1.0 - abs(frag_TexCoord.x * 2.0 - 1.0)) * 2.0);
    float aaCoverage = fillCoverage * strokeCoverage;

    out_Color = texColor * innerCol * aaCoverage;
}
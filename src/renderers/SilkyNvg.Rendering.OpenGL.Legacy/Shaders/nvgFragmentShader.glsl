precision mediump float;

uniform mat3x2 scissorMat;
uniform mat3x2 paintMat;
uniform vec4 innerCol;
uniform vec4 outerCol;
uniform vec2 scissorExt;
uniform vec2 scissorScale;
uniform vec2 extent;
uniform float radius;
uniform float feather;
uniform float strokeMult;
uniform float strokeThr;
uniform int texType;
uniform int type;

uniform sampler2D tex;

varying vec2 pass_tcoord;
varying vec2 pass_pos;

// TODO: The functions here

void main(void) {
    gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);
}
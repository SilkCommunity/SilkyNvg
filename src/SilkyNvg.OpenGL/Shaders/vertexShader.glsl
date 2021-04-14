// vec4(X, Y, U, V)
in vec4 vertexData;

out vec2 pass_texCoord;
out vec2 pass_position;

uniform vec2 viewSize;

void main(void) {
    pass_texCoord = vec2(vertexData.z, vertexData.w);
    pass_position = vec2(vertexData.x, vertexData.y);
    gl_Position = vec4(2.0 * vertexData.x / viewSize.x - 1.0, 1.0 - 2.0 * vertexData.y / viewSize.y, 0.0, 1.0);
}
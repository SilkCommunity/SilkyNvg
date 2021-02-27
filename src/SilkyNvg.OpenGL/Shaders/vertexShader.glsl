in vec2 vertexPosition;
in vec2 texCoord;

out vec2 pass_texCoord;
out vec2 pass_position;

uniform vec2 viewSize;

void main(void) {
    pass_texCoord = texCoord;
    pass_position = vertexPosition;
    gl_Position = vec4(2.0 * vertexPosition.x / viewSize.x - 1.0, 1.0 - 2.0 * vertexPosition.y / viewSize.y, 0, 1);
}
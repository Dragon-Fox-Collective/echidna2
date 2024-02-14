#version 330 core

in vec2 texCoord;
in vec3 localPosition;

out vec4 FragColor;

uniform vec4 color;

void main()
{
    vec2 size = localPosition.xy / (texCoord - 0.5);
    float thickness = 5;
    bool isInBorder = size.x / 2 - abs(localPosition.x) < thickness || size.y / 2 - abs(localPosition.y) < thickness;
    FragColor = isInBorder ? vec4(0, 0, 0, 1) : color;
    // FragColor = vec4((localPosition.x - size.x / 2), (localPosition.y - size.y / 2), 0, 1) / 20 * color;
}
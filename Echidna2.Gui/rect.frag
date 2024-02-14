#version 330 core

in vec2 texCoord;
in vec3 localPosition;

out vec4 FragColor;

uniform vec4 color;

void main()
{
    vec2 size = localPosition.xy / (texCoord - 0.5);
    float thickness = 5;
    bool isXDivideByZeroError = abs(texCoord.x - 0.5) < 0.001;
    bool isYDivideByZeroError = abs(texCoord.y - 0.5) < 0.001;
    bool isXInBorder = size.x / 2 - abs(localPosition.x) < thickness;
    bool isYInBorder = size.y / 2 - abs(localPosition.y) < thickness;
    bool isInBorder = (!isXDivideByZeroError && isXInBorder) || (!isYDivideByZeroError && isYInBorder);
    FragColor = isInBorder ? vec4(0, 0, 0, 1) : color;
}
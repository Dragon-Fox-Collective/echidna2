#version 330 core

in vec2 TexCoords;
in vec3 LocalPosition;

out vec4 FragColor;

uniform vec4 color;

void main()
{
    vec2 size = LocalPosition.xy / (TexCoords - 0.5);
    float thickness = 2;
    bool isXDivideByZeroError = abs(TexCoords.x - 0.5) < 0.001;
    bool isYDivideByZeroError = abs(TexCoords.y - 0.5) < 0.001;
    bool isXInBorder = size.x / 2 - abs(LocalPosition.x) < thickness;
    bool isYInBorder = size.y / 2 - abs(LocalPosition.y) < thickness;
    bool isInBorder = (!isXDivideByZeroError && isXInBorder) || (!isYDivideByZeroError && isYInBorder);
    FragColor = isInBorder ? vec4(0, 0, 0, 1) : color;
}
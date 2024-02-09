#version 330 core

in vec3 worldPosition;

out vec4 FragColor;

void main()
{
    FragColor = vec4(worldPosition, 1.0f);
}
#version 330 core

in vec2 TexCoords;

out vec4 FragColor;

void main()
{
    FragColor = vec4(TexCoords, 0.0f, 1.0f);
}
#version 430 core

in vec3 TexCoords;

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

uniform samplerCube skybox;

void main()
{
    vec3 color = texture(skybox, TexCoords).rgb;
    
    // Un Gamma correction
    color = pow(color, vec3(2.2));
    // Un Tone mapping
    color = color / (vec3(1.0) - color);
    
    FragColor = vec4(color, 1.0);
    BrightColor = vec4(0.0, 0.0, 0.0, 1.0);
}
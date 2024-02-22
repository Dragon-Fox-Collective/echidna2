#version 330 core

// https://learnopengl.com/PBR/Lighting

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

in vec2 TexCoords;
in vec3 GlobalPosition;
in vec3 Normal;

uniform vec3 albedo;
uniform float roughness;
uniform float metallic;
uniform float ao;

uniform int numLights;
uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

uniform vec3 cameraPosition;

const float PI = 3.14159265359;

float DistributionGGX(vec3 normal, vec3 cameraLightNormal, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(normal, cameraLightNormal), 0.0);
    float NdotH2 = NdotH*NdotH;
    
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
    
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;
    
    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
    
    return num / denom;
}

float GeometrySmith(vec3 normal, vec3 directionToCamera, vec3 directionToLight, float roughness)
{
    float NdotV = max(dot(normal, directionToCamera), 0.0);
    float NdotL = max(dot(normal, directionToLight), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
    
    return ggx1 * ggx2;
}

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float luminance(vec3 color)
{
    return dot(color, vec3(0.299, 0.587, 0.114));
}

void main()
{
    vec3 normal = normalize(Normal);
    vec3 directionToCamera = normalize(cameraPosition - GlobalPosition);
    
    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);
    
    // reflectance equation
    vec3 Lo = vec3(0.0);
    for(int i = 0; i < numLights; i++)
    {
        // calculate per-light radiance
        vec3 directionToLight = normalize(lightPositions[i] - GlobalPosition);
        vec3 cameraLightNormal = normalize(directionToCamera + directionToLight);
        float distanceToLight = length(lightPositions[i] - GlobalPosition);
        float attenuation = 1.0 / (distanceToLight * distanceToLight);
        vec3 radiance = lightColors[i] * attenuation;
        
        // cook-torrance brdf
        float NDF = DistributionGGX(normal, cameraLightNormal, roughness);
        float G = GeometrySmith(normal, directionToCamera, directionToLight, roughness);
        vec3 F = fresnelSchlick(max(dot(cameraLightNormal, directionToCamera), 0.0), F0);
        
        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;
        
        vec3 numerator = NDF * G * F;
        float denominator = 4.0 * max(dot(normal, directionToCamera), 0.0) * max(dot(normal, directionToLight), 0.0) + 0.0001;
        vec3 specular = numerator / denominator;
        
        // add to outgoing radiance Lo
        float NdotL = max(dot(normal, directionToLight), 0.0);
        Lo += (kD * albedo / PI + specular) * radiance * NdotL;
    }
    
    vec3 ambient = vec3(0.03) * albedo * ao;
    vec3 color = ambient + Lo;
    
    FragColor = vec4(color, 1.0);
    BrightColor = vec4(luminance(color) > 1.0 ? color : vec3(0.0), 1.0);
}
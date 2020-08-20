float4x4 World;
float4x4 View;
float4x4 Projection;
//float specularIntensity = 0.8f;
//float specularPower = 0.5f; 
float FogNear=50;
float FogFar=70;
float4 FogColor;//=float4(0.5f, 0.5f, 0.5f, 1);//grey
float4 vecEye;

texture Texture;
/*
sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};
*/
sampler diffuseSampler = sampler_state { texture = <Texture>; magfilter = ANISOTROPIC; minfilter = ANISOTROPIC; mipfilter=ANISOTROPIC; AddressU = WRAP; AddressV = WRAP;};

texture SpecularMap;
/*
sampler specularSampler = sampler_state
{
    Texture = (SpecularMap);
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};
*/
sampler specularSampler = sampler_state { texture = <SpecularMap>; magfilter = ANISOTROPIC; minfilter = ANISOTROPIC; mipfilter=ANISOTROPIC; AddressU = WRAP; AddressV = WRAP;};


texture NormalMap;
/*
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};
*/
sampler normalSampler = sampler_state { texture = <NormalMap>; magfilter = ANISOTROPIC; minfilter = ANISOTROPIC; mipfilter=ANISOTROPIC; AddressU = WRAP; AddressV = WRAP;};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent : TANGENT0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float2 Depth : TEXCOORD1;
    float3x3 tangentToWorld : TEXCOORD2;
	float4 Pos3D   : TEXCOORD5;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
    //float4 Normal : COLOR1;
    //float4 Depth : COLOR2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(float4(input.Position.xyz,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors
	output.tangentToWorld[0] = mul(input.Tangent, (float3x3)World);
	output.tangentToWorld[1] = mul(input.Binormal, (float3x3)World);
	output.tangentToWorld[2] = mul(input.Normal, (float3x3)World);

	output.Pos3D = worldPosition;
	
    return output;
}


PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;
    output.Color = tex2D(diffuseSampler, input.TexCoord);
    
	
    float4 specularAttributes = tex2D(specularSampler, input.TexCoord);
    //specular Intensity
	
	float dist = distance(vecEye, input.Pos3D);
	float fog = saturate((dist - FogNear) / (FogNear-FogFar)); 
	output.Color = lerp(FogColor, output.Color, fog);
	
    output.Color.a = specularAttributes.r;
    
    return output;
}
PixelShaderOutput PixelShaderFunctionNormal(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;
	
	float4 specularAttributes = tex2D(specularSampler, input.TexCoord);
	
    // read the normal from the normal map
    float3 normalFromMap = tex2D(normalSampler, input.TexCoord).rgb;
    //tranform to [-1,1]
    normalFromMap = 2.0f * normalFromMap - 1.0f;
    //transform into world space
    normalFromMap = mul(normalFromMap, input.tangentToWorld);
    //normalize the result
    normalFromMap = normalize(normalFromMap);
    //output the normal, in [0,1] space
    output.Color.rgb = 0.5f * (normalFromMap + 1.0f);

    //specular Power
    output.Color.a = specularAttributes.a;

    return output;
}

PixelShaderOutput PixelShaderFunctionDepth(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;

    output.Color = input.Depth.x / input.Depth.y;
	
    return output;
}
technique Color
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
technique Normal
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionNormal();
    }
}
technique Depth
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionDepth();
    }
}
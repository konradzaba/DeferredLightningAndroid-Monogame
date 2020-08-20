struct VertexShaderInput
{
	float2 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position.xy,1, 1);
	return output;
}
float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET0
{
	return float4(0,0,0,0);
}
//------
/*
float4 VertexShaderFunction(float3 inPos : SV_POSITION) : SV_POSITION
{
    float4 output = float4(inPos,1);
    return output;
}

PixelShaderOutput PixelShaderFunction(float4 input : SV_POSITION) : SV_TARGET0
{
    PixelShaderOutput output;
    //black color
    output.Color = 0.0f;
    output.Color.a = 0.0f;
    //when transforming 0.5f into [-1,1], we will get 0.0f
    output.Normal.rgb = 0.5f;
    //no specular power
    output.Normal.a = 0.0f;
    //max depth
    output.Depth = 0.0f;
    return output;
}
*/
technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
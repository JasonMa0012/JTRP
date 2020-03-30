

// daoguang
void sceneUVs_float(float4 screenPos, float4 _Diffuse_var, float4 vertexColor, float _4_var, out float4 sceneUVs)
{
    float4 screenPos_var = screenPos;
    // screenPos_var.y *= _ProjectionParams.x;
    sceneUVs = float4(screenPos_var.xy + ((_Diffuse_var.rg * vertexColor.a) * (_4_var * 0.1)), screenPos.zw);
}

void daoguang_float(float4 _Diffuse_var, float4 vertexColor, float3 emissive, float4 _Noise_var, float size, float luminance, out float4 color)
{
    ////// Emissive:
    // float3 emissive = ((_2_diffuse * (_3_color.rgb * _Diffuse_var.rgb)) * _Diffuse_var.r);
    // float4 _Noise_var = tex2D(_Noise, TRANSFORM_TEX(i.uv0, _Noise));
    float node_459_if_leA = step((size + vertexColor.r), _Noise_var.r);
    float node_459_if_leB = step(_Noise_var.r, (size + vertexColor.r));
    float node_7299 = 0.0;
    float node_1623 = 1.0;
    float node_459 = lerp((node_459_if_leA * node_7299) + (node_459_if_leB * node_1623), node_1623, node_459_if_leA * node_459_if_leB);
    float node_2124_if_leA = step(vertexColor.r, _Noise_var.r);
    float node_2124_if_leB = step(_Noise_var.r, vertexColor.r);
    
    float a = (vertexColor.a * (_Diffuse_var.a * (node_459 + ((node_459 - lerp((node_2124_if_leA * node_7299) + (node_2124_if_leB * node_1623), node_1623, node_2124_if_leA * node_2124_if_leB)) * luminance))));
    color = float4(emissive, a);
}

void Trail_float(float3 normal, float3 V, float4 mask, float4 _Color, float4 _ColorOut, float4 _ColorIn, float4 _Width, out float4 color)
{
    float VdotN = dot(V, normal);
    color = lerp(_ColorOut, _ColorIn, pow(abs(VdotN), _Width)) * _Color * mask;
    clip(color.a - 0.01);
}
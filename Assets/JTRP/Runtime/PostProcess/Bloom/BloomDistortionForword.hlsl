#ifndef BLOOMDISTORTIONFORWORD
    #define BLOOMDISTORTIONFORWORD
    
    struct GraphVertexInput
    {
        float4 vertex: POSITION;
        float4 texcoord0: TEXCOORD0;
        // float3 normal: NORMAL;
        // float4 color: COLOR;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    struct GraphVertexOutput
    {
        float4 position: POSITION;
        half4 uv0: TEXCOORD0;
        // float3 positionWS: TEXCOORD1;
        // float3 normal: TEXCOORD2;
        // float4 color: COLOR;
    };
    GraphVertexOutput vert(GraphVertexInput v)
    {
        GraphVertexOutput o;
        float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
        o.position = TransformWorldToHClip(positionWS);
        // o.positionWS = positionWS;
        // o.normal = TransformObjectToWorldNormal(v.normal);
        o.uv0 = v.texcoord0;
        // o.color = v.color;
        
        return o;
    }
    
    float4 frag(GraphVertexOutput i): SV_Target0
    {
        float4 uv0 = i.uv0;
        
        float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv0.xy) * _Color;
        
        float4 finalColor = baseColor;
        
        return float4(0, 1, 0, 1);
    }
    
    
#endif // BLOOMDISTORTIONFORWORD
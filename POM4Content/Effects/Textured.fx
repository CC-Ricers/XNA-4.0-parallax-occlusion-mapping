
float4 g_materialAmbientColor;      
float4 g_materialDiffuseColor;      
float4 g_materialSpecularColor;  

float  g_fSpecularExponent;         
bool   g_bAddSpecular;              

float4 g_LightDiffuse;              
float4 g_LightAmbient;              

float    g_fHeightMapScale;        
bool     g_bDisplayShadows = true;        
float    g_fShadowSoftening = 0.5f;       

int      g_nMinSamples;            
int      g_nMaxSamples;

float4x4 World;
float4x4 WorldViewProjection;
float3 LightPosition;
float3 CameraPos;
float linearAttenuation = 0.0008f;
float quadraticAttenuation = 0.00015f;

texture Texture;              
texture NormalMap;               
texture HeightMap; 

sampler2D baseSampler = sampler_state
{
	Texture = <Texture>;
    ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

sampler2D normalSampler = sampler_state
{
	Texture = <NormalMap>;
    ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

sampler2D heightSampler = sampler_state
{
	Texture = <HeightMap>;
    ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};


struct VS_INPUT
{
	float4 Position : POSITION0; 
	float3 Normal	: NORMAL; 
	float2 Texcoord	: TEXCOORD0;
	float3 Binormal : BINORMAL0;
    float3 Tangent  : TANGENT0;
};

struct SPOT_VS_OUTPUT
{
	float4 Position			: POSITION0;
	float2 Texcoord			: TEXCOORD0;
    float3 Normal			: TEXCOORD1;
    float3 LightDir			: TEXCOORD2;
    float3 ViewDir			: TEXCOORD3;
    float dist				: TEXCOORD4;
    float3x3 TBN			: TEXCOORD5;
};

struct VS_OUTPUT_POM
{
    float4 position          : POSITION0;
    float2 Texcoord          : TEXCOORD0;
    float3 vLightTS          : TEXCOORD1;   
    float3 vViewTS           : TEXCOORD2;  
    float2 vParallaxOffsetTS : TEXCOORD3;   
    float3 vNormalWS         : TEXCOORD4;   
    float3 vViewWS           : TEXCOORD5; 
    float4 dist				 : TEXCOORD6;
};  

struct PS_INPUT_POM
{
   float2 texCoord          : TEXCOORD0;
   float3 vLightTS          : TEXCOORD1_centroid;
   float3 vViewTS           : TEXCOORD2_centroid;
   float2 vParallaxOffsetTS : TEXCOORD3_centroid;
   float3 vNormalWS         : TEXCOORD4_centroid;
   float3 vViewWS           : TEXCOORD5_centroid;
   float3 dist				: TEXCOORD6_centroid;
};


VS_OUTPUT_POM VertexShaderPOM (VS_INPUT Input)
{
	VS_OUTPUT_POM Output; 
	
	Output.position = mul(Input.Position, WorldViewProjection); 
	Output.Texcoord = Input.Texcoord * 2.0;
	float3 vNormalWS   = mul( Input.Normal,   World );
    float3 vTangentWS  = mul( Input.Tangent,  World );
    float3 vBinormalWS = mul( Input.Binormal, World );

    Output.vNormalWS = vNormalWS;
    vNormalWS   = normalize( vNormalWS );
    vTangentWS  = normalize( vTangentWS );
    vBinormalWS = normalize( vBinormalWS );

    float4 vPositionWS = mul( Input.Position, World );
    float3 vViewWS = CameraPos - vPositionWS.xyz;
    Output.vViewWS = vViewWS;

    float3 vLightWS = LightPosition- vPositionWS;//LightDirection;
    Output.dist = length(vLightWS);

    float3x3 mWorldToTangent = float3x3( vTangentWS, vBinormalWS, vNormalWS );

    Output.vLightTS = mul( mWorldToTangent, vLightWS );
    Output.vViewTS  = mul( mWorldToTangent, vViewWS  );

    float2 vParallaxDirection = normalize(  Output.vViewTS.xy );
    float fLength = length( Output.vViewTS );
    float fParallaxLength = sqrt( fLength * fLength - Output.vViewTS.z * Output.vViewTS.z ) / Output.vViewTS.z; 

    Output.vParallaxOffsetTS = vParallaxDirection * fParallaxLength;
    Output.vParallaxOffsetTS *= g_fHeightMapScale;
    
	return Output;
}


float4 ComputeIllumination( float2 texCoord, float3 vLightTS, float3 vViewTS, float fOcclusionShadow, float dist )
{
	float3 vNormalTS = normalize( tex2D( normalSampler, texCoord ) * 2 - 1 );
	float4 cBaseColor = tex2D( baseSampler, texCoord );
	float att = dist * linearAttenuation;
	att += dist * dist * quadraticAttenuation;
	att = 1 / att;
	float4 cDiffuse = saturate( dot( vNormalTS, vLightTS )) * g_materialDiffuseColor
	* g_LightDiffuse * att;
	float3 vReflectionTS = normalize( 2 * dot( vViewTS, vNormalTS ) * vNormalTS - vViewTS );
	float fRdotL = dot( vReflectionTS, vLightTS );
	float4 cSpecular = 0;
	if ( g_bAddSpecular )
	{
		cSpecular = saturate( pow( fRdotL, g_fSpecularExponent )) * g_materialSpecularColor;
	}
	float4 cFinalColor = (( g_materialAmbientColor + cDiffuse ) * cBaseColor + cSpecular ) * fOcclusionShadow; 
	
	return cFinalColor;
} 
 
float4 PixelShaderPOM( PS_INPUT_POM i ) : COLOR0
{   
	
	float3 vViewTS   = normalize( i.vViewTS  );
	float3 vViewWS   = normalize( i.vViewWS  );
	float3 vLightTS  = normalize( i.vLightTS );
	float3 vNormalWS = normalize( i.vNormalWS );
     
	float4 cResultColor = float4( 0, 0, 0, 1 );

	float2 dx = ddx( i.texCoord );
	float2 dy = ddy( i.texCoord );
                  
	int nNumSteps = (int) lerp( g_nMaxSamples, g_nMinSamples, dot( vViewWS, vNormalWS ) );

	float fCurrHeight = 0.0;
	float fStepSize   = 1.0 / (float) nNumSteps;
	float fPrevHeight = 1.0;
	float fNextHeight = 0.0;

	int    nStepIndex = 0;
	bool   bCondition = true;

	float2 vTexOffsetPerStep = fStepSize * i.vParallaxOffsetTS;
	float2 vTexCurrentOffset = i.texCoord;
	float  fCurrentBound     = 1.0;
	float  fParallaxAmount   = 0.0;

	float2 pt1 = 0;
	float2 pt2 = 0;

	float2 texOffset2 = 0;

	while ( nStepIndex < nNumSteps ) 
	{
		vTexCurrentOffset -= vTexOffsetPerStep;

		fCurrHeight = tex2Dgrad( heightSampler, vTexCurrentOffset, dx, dy ).r;

		fCurrentBound -= fStepSize;

		if ( fCurrHeight > fCurrentBound ) 
		{     
			pt1 = float2( fCurrentBound, fCurrHeight );
			pt2 = float2( fCurrentBound + fStepSize, fPrevHeight );

			texOffset2 = vTexCurrentOffset - vTexOffsetPerStep;

			nStepIndex = nNumSteps + 1;
		}
		else
		{
			nStepIndex++;
			fPrevHeight = fCurrHeight;
		}
	}   

	float fDelta2 = pt2.x - pt2.y;
	float fDelta1 = pt1.x - pt1.y;
	fParallaxAmount = (pt1.x * fDelta2 - pt2.x * fDelta1 ) / ( fDelta2 - fDelta1 );
   
	float2 vParallaxOffset = i.vParallaxOffsetTS * (1 - fParallaxAmount );

	float2 texSample = i.texCoord - vParallaxOffset;
	float fOcclusionShadow = 1;
   
	if ( g_bDisplayShadows == true )
	{
		float2 vLightRayTS = vLightTS.xy * g_fHeightMapScale;
	      
		float sh0 =  tex2Dgrad( heightSampler, texSample, dx, dy ).r;
		float shA = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.88, dx, dy ).r - sh0 - 0.88 ) *  1 * g_fShadowSoftening;
		float sh9 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.77, dx, dy ).r - sh0 - 0.77 ) *  2 * g_fShadowSoftening;
		float sh8 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.66, dx, dy ).r - sh0 - 0.66 ) *  4 * g_fShadowSoftening;
		float sh7 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.55, dx, dy ).r - sh0 - 0.55 ) *  6 * g_fShadowSoftening;
		float sh6 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.44, dx, dy ).r - sh0 - 0.44 ) *  8 * g_fShadowSoftening;
		float sh5 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.33, dx, dy ).r - sh0 - 0.33 ) * 10 * g_fShadowSoftening;
		float sh4 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.22, dx, dy ).r - sh0 - 0.22 ) * 12 * g_fShadowSoftening;
	   
		fOcclusionShadow = 1 - max( max( max( max( max( max( shA, sh9 ), sh8 ), sh7 ), sh6 ), sh5 ), sh4 );
		fOcclusionShadow = fOcclusionShadow * 0.65 + 0.35; 
	}   
   
	cResultColor = ComputeIllumination( texSample, vLightTS, vViewTS, fOcclusionShadow, i.dist );
           
	return cResultColor;
}

 
SPOT_VS_OUTPUT VertexShaderFunction (VS_INPUT Input)
{
	SPOT_VS_OUTPUT Output;
	
	Output.Position = mul(Input.Position, WorldViewProjection); 
	Output.Texcoord = Input.Texcoord * 2.0;
	Output.Normal = mul(Input.Normal, World);
	float3 aux =  LightPosition - mul(Input.Position,World);
	Output.LightDir = normalize(aux);
	Output.ViewDir =  CameraPos - mul(Input.Position,World);
	Output.dist = length(aux);
	Output.TBN[0] = mul(Input.Tangent, World);
    Output.TBN[1] = mul(Input.Binormal, World);
    Output.TBN[2] = mul(Input.Normal, World);
    
    return Output;
}

float4 PixelShaderTextured (SPOT_VS_OUTPUT Input) : COLOR0
{
	float4 texColor = tex2D(baseSampler, Input.Texcoord);
	float3 Normal = normalize(Input.Normal);
	float4 Diffuse = saturate(dot(normalize(Input.Normal), normalize(Input.LightDir)));
	Diffuse = Diffuse * g_LightDiffuse * g_materialDiffuseColor;
	float4 finalColor = 1;
	float att = Input.dist * linearAttenuation;
	att += Input.dist * Input.dist * quadraticAttenuation;
	att = 1 / att;
	finalColor = ((Diffuse * att)
		 +  g_materialAmbientColor) * texColor;
	
	return finalColor;
}

float3 ComputeNormal(sampler2D normalSampler, float2 TexCoord, float3x3 TBN)
{
    float3 normal = tex2D(normalSampler, TexCoord);
    normal = 2.0f * normal - 1.0f;
    normal = mul(normal, TBN);
    normal = normalize(normal);
    return normal;
}


float4 PixelShaderNormal (SPOT_VS_OUTPUT Input) : COLOR0
{
	float4 finalColor = 1;
	float4 texColor = tex2D(baseSampler, Input.Texcoord);
	float3 Normal = ComputeNormal(normalSampler,Input.Texcoord, Input.TBN);
	float4 Diffuse = saturate(dot(normalize(Normal), normalize(Input.LightDir)));
	Diffuse = Diffuse * g_LightDiffuse * g_materialDiffuseColor;
	float att = Input.dist * linearAttenuation;
	att += Input.dist * Input.dist * quadraticAttenuation;
	att = 1 / att;
	finalColor = ((Diffuse * att) + g_materialAmbientColor) * texColor;
		
	return finalColor;
}



technique POM
{
    pass P0
    {
		VertexShader = compile vs_3_0 VertexShaderPOM();
        PixelShader = compile ps_3_0 PixelShaderPOM();
    }
}

technique NormalMapping
{
    pass P0
    {
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderNormal();
    }
}

technique Textured
{
    pass P0
    {
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderTextured();
    }
}
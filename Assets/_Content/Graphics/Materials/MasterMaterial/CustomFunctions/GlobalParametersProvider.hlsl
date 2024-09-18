#ifndef GLOBAL_TEXTURE_PROVIDER_INCLUDED
#define GLOBAL_TEXTURE_PROVIDER_INCLUDED

float3 _GlowOrigin;
float _GlowHeight;
float _GlowRadius;
float _GlowStrength;
float4 _GlowColor;
float _GlowPercentStartFade;

void GetGlowOrigin_float(out float3 glowOrigin){
    glowOrigin = _GlowOrigin;
}

void GetGlowHeight_float(out float glowHeight){
    glowHeight = _GlowHeight;
}

void GetGlowRadius_float(out float glowRadius){
    glowRadius = _GlowRadius;
}

void GetGlowStrength_float(out float glowStrength){
    glowStrength = _GlowStrength;
}

void GetGlowColor_float(out float4 glowColor){
    glowColor = _GlowColor;
}

void GetGlowPercentStartFade_float(out float glowPercentStartFade){
    glowPercentStartFade = _GlowPercentStartFade;
}

#endif
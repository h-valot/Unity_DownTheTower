#ifndef CUBIC_REMAP_INCLUDED
#define CUBIC_REMAP_INCLUDED

void CubicRemap_float(float In, float MinIn, out float Out)
{
    Out = 1 - pow(In / (1 - MinIn) - MinIn / (1 - MinIn), 3);
}

#endif
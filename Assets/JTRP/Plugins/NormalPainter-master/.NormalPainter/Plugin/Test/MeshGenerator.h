#pragma once

#include "MeshUtils/MeshUtils.h"
using namespace mu;

void GenerateWaveMesh(
    RawVector<int>& counts,
    RawVector<int>& indices,
    RawVector<float3> &points,
    RawVector<float2> &uv,
    float size, float height,
    const int resolution,
    float angle,
    bool triangulate = false);


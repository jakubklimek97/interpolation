#pragma once
extern "C" __declspec(dllexport) int returnNumber();
extern "C" __declspec(dllexport) int interpolate(char* ptr);
extern "C" __declspec(dllexport) void interpolateC(char* src, int width, int height, char* dst, int widthD, int heightD);
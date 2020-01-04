#pragma once
extern "C" __declspec(dllexport) int returnNumber();
extern "C" __declspec(dllexport) int interpolate(char* ptr);
extern "C" __declspec(dllexport) int interpolateC(char* src, char* dst, int width, int height, int newWidth, int newHeight);
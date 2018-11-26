#include "Platform.h"

uint32_t getSystemTime() {
#if defined(_WINDOWS) || defined(__linux)
	return (uint32_t)1000 * 1000 / CLOCKS_PER_SEC * clock();
#else
	return micros();
#endif // _WINDOWS || __linux
}
#include "SystemTime.h"

SystemTime::SystemTime() {
	time.lowerTime = micros();
	time.upperTime = 0;
}

void SystemTime::update() {
	uint32_t t = micros();
	time.lowerTime = t;
	if (t > time.lowerTime) {
		time.upperTime++;
	}
}

BigTimeStamp BigTimeStamp::operator-(const BigTimeStamp & b)
{
	BigTimeStamp result;
	result.lowerTime = lowerTime - b.lowerTime;
	result.upperTime = upperTime - b.upperTime;

	if (b.lowerTime > lowerTime) {
		result.upperTime--;
	}
}

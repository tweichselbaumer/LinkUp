#include "LinkUpRaw.h"
#include "MPU9250.h"
#include "SystemTime.h"

#define BUFFER_SIZE 64

#define DataStream Serial
#define DataBaud 2000000

IntervalTimer imuTimer;

uint8_t pBuffer[BUFFER_SIZE];
LinkUpRaw linkUpConnector;

uint32_t nNextUpdate = 0;

MPU9250 IMU(10);
SystemTime systemTime;


int beginStatus;

struct PACKED DATA {
	uint32_t timeL;
	uint32_t timeH;
	int16_t gx;
	int16_t gy;
	int16_t gz;
	int16_t ax;
	int16_t ay;
	int16_t az;
	int16_t mx;
	int16_t my;
	int16_t mz;
	int16_t t;
	int16_t p;
};


void setup()
{
	DataStream.begin(DataBaud);
	DataStream.setTimeout(1);
	pinMode(10, OUTPUT);
	IMU.begin(ACCEL_RANGE_16G, GYRO_RANGE_2000DPS);
	imuTimer.begin(readImu, 500);
	imuTimer.priority(0);
}

void readImu() {
	DATA data;
	systemTime.update();
	data.timeL = systemTime.time.lowerTime;
	data.timeH = systemTime.time.upperTime;

	IMU.getMotion10Counts(&data.ax, &data.ay, &data.az, &data.gx, &data.gy, &data.gz, &data.mx, &data.my, &data.mz, &data.t);

	LinkUpPacket packet;
	packet.pData = (uint8_t*)malloc(sizeof(DATA));
	*(DATA*)packet.pData = data;
	packet.nLength = sizeof(DATA);
	linkUpConnector.send(packet);
}

void loop()
{
	systemTime.update();
	uint32_t nBytesToSend;

	nBytesToSend = linkUpConnector.getRaw(pBuffer, BUFFER_SIZE);

	if (nBytesToSend > 0) {
		DataStream.write(pBuffer, nBytesToSend);
		DataStream.send_now();
	}
}
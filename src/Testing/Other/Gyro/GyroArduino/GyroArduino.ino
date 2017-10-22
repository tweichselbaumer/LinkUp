#include "LinkUpRaw.h"
#include "MPU9250.h"

#define BUFFER_SIZE 1024

#define DebugStream Serial
#define DataStream Serial1

#define DebugBaud 115200
#define DataBaud 2000000

#define PinLed 13

uint8_t nLedStatus = 1;

uint8_t pBuffer[BUFFER_SIZE];
uint32_t nLastTicks = 0;
uint32_t nLastTicksFast = 0;
LinkUpRaw linkUpConnector;

MPU9250 IMU((uint8_t)0x68, (uint8_t)0);


int beginStatus;

struct DATA {
	uint32_t time;
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

DATA data;

void setup()
{
	DebugStream.begin(DebugBaud);
	DebugStream.setTimeout(1);
	DataStream.begin(DataBaud);
	DataStream.setTimeout(1);

	pinMode(PinLed, OUTPUT);

	IMU.begin(ACCEL_RANGE_4G, GYRO_RANGE_250DPS);
	//IMU.setFilt((mpu9250_dlpf_bandwidth)-1, 255);
}

void loop()
{
	uint32_t nBytesToSend;
	uint32_t nTime = micros();

	if (nTime - nLastTicks > 1000 * 1000 * 1)
	{
		nLastTicks = nTime;
		nLedStatus = !nLedStatus;
		digitalWrite(PinLed, nLedStatus);
	}

	if (nTime - nLastTicksFast >= 1000 * 5/* && IMU.checkDataReady()*/)
	{
		DebugStream.println(nTime);
		nLastTicksFast = nTime;
		data.time = nTime;
		IMU.getMotion10Counts(&data.ax, &data.ay, &data.az, &data.gx, &data.gy, &data.gz, &data.mx, &data.my, &data.mz, &data.t);
		LinkUpPacket packet;
		packet.pData = (uint8_t*)malloc(sizeof(DATA));
		*(DATA*)packet.pData = data;
		packet.nLength = sizeof(DATA);
		linkUpConnector.send(packet);
	}

	nBytesToSend = linkUpConnector.getRaw(pBuffer, BUFFER_SIZE);

	if (nBytesToSend > 0) {
		DataStream.write(pBuffer, nBytesToSend);
	}
}
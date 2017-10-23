#include "LinkUpRaw.h"
#include "MPU9250.h"

#define BUFFER_SIZE 1024

#define DataStream Serial
#define DataBaud 2000000

#define PinLed 13

uint8_t nLedStatus = 1;

uint8_t pBuffer[BUFFER_SIZE];
LinkUpRaw linkUpConnector;

uint32_t nLastTicks = 0;
uint32_t nLastTicksFast = 0;

MPU9250 IMU(10);


int beginStatus;

struct PACKED DATA {
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
	DataStream.begin(DataBaud);
	DataStream.setTimeout(1);
	pinMode(10, OUTPUT);
	//pinMode(PinLed, OUTPUT);
	IMU.begin(ACCEL_RANGE_4G, GYRO_RANGE_250DPS);
}

void loop()
{
	uint32_t nBytesToSend;
	uint32_t nTime = micros();

	if (nTime - nLastTicks > 1000 * 1000 * 1)
	{
		nLastTicks += 1000 * 1000 * 1;
		nLedStatus = !nLedStatus;
		//digitalWrite(PinLed, nLedStatus);
	}
	

	if (nTime - nLastTicksFast >= 1000)
	{
		nLastTicksFast += 1000;
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
	DataStream.flush();
}
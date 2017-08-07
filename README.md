![LinkUp](https://raw.githubusercontent.com/tweichselbaumer/LinkUp/master/Graphics/Raw/LinkUp-logo.png)

## About
LinkUp is a cross platform lightweight communication library. The framework is able to transmit data between different microcontrollers, mobile devices and computers.

## Build
[![Build status](https://ci.appveyor.com/api/projects/status/u8krtoysxm048o7x?svg=true)](https://ci.appveyor.com/project/tweichselbaumer/linkup)

## Installation
The current version of the library is released on [nuget](https://www.nuget.org/packages/LinkUp).

### Arduino/C/C++
If you use Visual Studio for developing your Arduino/C/C++ Project, you can install the nessesary files by adding a nuget package with the nuget packages manager.

Alternatively, download the package from [nuget](https://www.nuget.org/packages/LinkUp), unzip the file and extract the files from *content\native*. Add these files to your project.

### C\#/.net/Xamarin/Portable
The C# version of the library can be installed by the nuget package manager.
If you create a portable library, make sure to use this LinkUp nuget package also at the nativ project to support the full functionality.

## Architecture
The LinkUp protocol is splitted into several layer. The LinkUpRaw layer which provides a communication protocol between two endpoints. The LinkUpLogic organizes nodes in a hirachical tree. Each node can provide there one labels. Labels represents different kinds of functionalties and are accessable from there node and all it's parent nodes.

### LinkUpRaw Layer
LinkUpRaw provides basic data transmissions between two endpoints. It encapsulats binary data into a header with preamble, length, data, CRC16 and EOP(end of packet). It has the capability to detect bit errors. Bytes with the value of preamble, EOP, skip pattern are replaced by the skip pattern and the actual value XOR 0x20.

#### LinkUpPacket
Name | Size (Byte) | Offset (Byte)
---- | ---- | ----
Preamble | 1 | 0
Length | 2 | 1
Data | n (max 2^16) | 3
CRC16 | 2 | n + 3
EOP | 1 | n + 5

## Get Started
### C\# - LinkUpRaw (single process)
In this sample the basic usage of LinkUpRaw will be demonstrated. In this case we use the LinkUpMemoryConnector which only provieds communication in the same programm. This code is mostly useful for testing purpose.

[Code](https://github.com/tweichselbaumer/LinkUp/tree/master/src/Testing/Example/Example1)

#### Initialisation
```cs
BlockingCollection<byte[]> col1 = new BlockingCollection<byte[]>();
BlockingCollection<byte[]> col2 = new BlockingCollection<byte[]>();

LinkUpMemoryConnector slaveToMaster = new LinkUpMemoryConnector(col1, col2);
LinkUpMemoryConnector masterToSlave = new LinkUpMemoryConnector(col2, col1);
```

#### Send Packet
```cs
slaveToMaster.SendPacket(new LinkUpPacket() { Data = data });
```

#### Receive Packet
```cs
masterToSlave.ReveivedPacket += MasterToSlave_ReveivedPacket;
```

### C\#/Arduino - LinkUpRaw (Serial Port)
The next example will show a basic communication between a C\# programm and an Arduino microcontroller.

[Code](https://github.com/tweichselbaumer/LinkUp/tree/master/src/Testing/Example/Example2)

#### Initialisation

##### C\# Program
```cs
LinkUpSerialPortConnector dataPort = new LinkUpSerialPortConnector(portName, baudRate);
```

##### Arduino
```cpp
LinkUpRaw connector;
```

#### Send Packet

##### C\# Program
```cs
dataPort.SendPacket(new LinkUpPacket() { Data = data });
```

##### Arduino
```cpp
LinkUpPacket packet;
connector.send(packet);
nBytesToSend = connector.getRaw(pBuffer, BUFFER_SIZE);
```

#### Receive Packet

##### C\# Program
```cs
dataPort.ReveivedPacket += DataPort_ReveivedPacket;
```

##### Arduino
```cpp
connector.progress(pBuffer, nBytesRead);

if (connector.hasNext())
{
    LinkUpPacket packet = connector.next();
}
```

### C\#/C\# - LinkUpRaw (UDP)
The next example will show a basic communication between a C\# programm and an other C\# programm over UDP.

[Code](https://github.com/tweichselbaumer/LinkUp/tree/master/src/Testing/Example/Example4)

#### Initialisation

##### C\# Program
```cs
LinkUpUdpConnector connector = new LinkUpUdpConnector(sourceIp, destinationIp, sourcePort, destinationPort);
```

#### Send Packet

##### C\# Program
```cs
connector.SendPacket(new LinkUpPacket() { Data = data });
```

#### Receive Packet

##### C\# Program
```cs
connector.ReveivedPacket += DataPort_ReveivedPacket;
```

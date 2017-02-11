#![LinkUp](https://raw.githubusercontent.com/tweichselbaumer/LinkUp/master/Graphics/Raw/LinkUp-logo.png)

## About
LinkUp is a cross platform lightweight communication library. The framework is able to transmit data between different microcontrollers, mobile devices and computers.

## Installation
The current version of the library is released on [nuget](https://www.nuget.org/packages/LinkUp).

### Arduino/C/C++
If you use Visual Studio for developing your Arduino/C/C++ Project, you can install the nessesary files by adding a nuget package with the nuget packages manager.

Alternatively, download the package from [nuget](https://www.nuget.org/packages/LinkUp), unzip the file and extract the files from *content\native*. Add these files to your project.

### C#/.net/Xamarin/Portable
The C# version of the library can be installed by the nuget package manager.
If you create a portable library, make sure to use this LinkUp nuget package also at the nativ project to support the full functionality.

## Architecture
The LinkUp protocol is splitted into several layer. The LinkUpRaw layer which provides a communication protocol between two endpoints. The LinkUpLogic organizes nodes in a hirachical tree. Each node can provide there one labels. Labels represents different kinds of functionalties and are accessable from there node and all it's parent nodes.

### LinkUpRaw Layer
LinkUpRaw provides basic data transmissions between two endpoints. It encapsulats binary data into a header with preamble, length, data, CRC16 and EOP(end of packet). It has the capability to detect bit errors. Bytes with the value of preamble, EOP, skip pattern are replaced by the skip pattern and the actual value xor 0x20.

#### LinkUpPacket
Name | Size (Byte) | Offset (Byte)
---- | ---- | ----
Preamble | 1 | 0
Length | 2 | 1
Data | n (max 2^16) | 3
CRC16 | 2 | n + 3
EOP | 1 | n + 5

## Get Started
### C# Master Node - C# Slave Node

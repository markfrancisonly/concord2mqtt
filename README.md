# concord2mqtt

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://raw.githubusercontent.com/chkr1011/MQTTnet/master/LICENSE)

Home Assistant integration for GE Security Concord 4 panels installed by ADT, Brinks, and independent contractors that allows co-existance with Alarm.com cellular modules.

The embedded Concord library is a mature codebase that I wrote more than a decade ago and has been running continously in my home for the past 10 years. After recently discovering 
the [Home Assistant](https://www.home-assistant.io/), I decided to port the library to .net 5 and release it open source. The solution implements Home Assistant MQTT automatic discovery for zones and partitions, 
and provides an alarm control panel. Integration can be deployed in a docker container, and a sample docker-compose.yml file is included. 

The core concord library implements the GE Security Automation Module Protocol dated 12.15.2005. Three communication methods are provided, serial port, tcp server and tcp client.
Tcp server and client are to be used when connecting to a serial-to-ethernet adapter.


# Reference implementation 

The following part numbers for a reference implementation:
- GE Security Concord 4 80-860-4-KT (version 4.1+, 1/19/07)
- GE a GE Security 60-783-02 SuperBus 2000 RS-232 Automation Module
- MOXA NPort 5110 RS232 Serial to Ethernet adapter
- POE power adapter

Docker image runs in Ubunutu 20.04 and connects to a HASSO [Mosquitto](https://github.com/eclipse/mosquitto) MQTT broker add-on. 


Configuration is controlled via appsettings.json which may be mounted external to the container on the docker host, or environment variables

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },

  "Concord2Mqtt": {
    "MQTT": {
      "ClientId": "concord2mqtt",
      "Username": "mqtt username",
      "Password": "mqtt password",
      "Host": "homeassistant",
      "Port": "1883",
      "Secure": false
    },
    "Discovery": true,
    "DirectDisarm": false
  },

  "Concord": {
    "TcpAddress": "192.168.0.100",
    "TcpPort": 4001,
    "SerialPort": "COM1",
    "Connection": "TcpServer",
    "AutomationUserId": 0,
    "Users": [
      {
        "Name": "Home",
        "Id": 0
      }
    ]
  }
}
```

The following are required MQTT settings:

| Option       | Values         | Description                                                                                                                               | default |
| ------------ | -------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| `Username`   | string         | User defined in Mosquitto broker configuration                                                                                            |         |
| `Password`   | string         | Password defined in Mosquitto broker configurationr                                                                                       |         |
| `Host`       | string         | Hostname or ip address of MQTT broker                                                                                                     | homeassistant        |
| `Port`       | numeric        | TCP port for MQTT                                                                                                                         | 1883    |

Concord library is configured for connection method and the arm/disarm automation user id must be specified: 

| Option       | Values         | Description                                                                                                                               | default |
| ------------ | -------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| `TcpAddress` | string         | Required when using TCP client or server method                                                                                           |         |
| `TcpPort`    | numeric        | Required when using TCP client or server method                                                                                           |         |
| `Connection` | 'TcpServer','TcpClient','SerialPort'         | Communication method                                                                                        |     |
| `AutomationUserId`       | numeric        | User id for the code that will be used to arm or disarm security partition                                                    |      |



## Supported MQTT versions

* 5.0.0
* 3.1.1
* 3.1.0

see [MQTTnet](https://github.com/chkr1011/MQTTnet/)



## License

MIT License

Concord2mqtt Copyright (c) 2009-2021 Mark Berndt

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

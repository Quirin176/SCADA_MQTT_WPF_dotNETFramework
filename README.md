# MyWPFApp — SCADA System (C# .NET Framework)

This is a demo SCADA application built using **C# WPF (.NET Framework)**, designed for monitoring and control purposes.

## Features
- Real-time tag monitoring and control
- Analog & Digital alarm handling (`AnalogAlarmCollection.xml`, `DigitalAlarmCollection.xml`)
- MQTT protocol for IoT communication (`MQTT_Protocol` project)
- Reporting module (`HMI_Report`)
- Custom tool utilities (`HMI_Tool`)
- Central tag management (`TagManagement`)

## Solution Structure
| Project | Description |
|----------|--------------|
| **MyWPFApp** | Main WPF visualization and runtime |
| **HMI_Alarm** | Alarm system UI and logic |
| **HMI_Report** | Reporting and historical data management |
| **HMI_Tool** | Tools and utilities |
| **MQTT_Protocol** | Customized M2MQTT Library - MQTT communication module |
| **TagManagement** | Central tag handling and configuration |

## Technology Stack
- **C# .NET Framework**
- **WPF** (Windows Presentation Foundation)
- **MVVM pattern**
- **MQTT**
- **XML Configuration**
- **SQLite / File-based data logging**

## How to Run
1. Clone the repository:
   ```bash
   git clone https://github.com/Quirin176/MQTT_SCADA_WPF_DotNETFramework.git
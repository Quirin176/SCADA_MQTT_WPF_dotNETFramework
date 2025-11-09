# SCADA System (C# .NET Framework)

This is a demo SCADA application built using C# WPF (.NET Framework), designed for real-time monitoring and control in industrial or IoT environments.

---

## Features
- Real-time tag monitoring and control
- Analog & Digital alarm handling and XML-based alarms configuration (`HMI_Alarm` project)
- Custom MQTT protocol for IoT communication (`MQTT_Protocol` project, based on the M2MQTT library, extended for SCADA data exchange)
- Historical data logging and reporting (`HMI_Report` project)
- Custom tool utilities (`HMI_Tool` project)
- Central tag management and XML-based tags configuration (`TagManagement` project)
- Data storage in Microsoft SQL Server

---

## Solution Structure
| Project | Description |
|----------|--------------|
| **MyWPFApp** | Main WPF visualization and runtime |
| **HMI_Alarm** | Alarm system UI and logic |
| **HMI_Report** | Reporting and historical data management |
| **HMI_Tool** | Tools, symbols, controls and utilities... |
| **MQTT_Protocol** | Customized M2MQTT Library for SCADA-specific MQTT communication |
| **TagManagement** | Central tag handling, configuration, and XML serialization |

---

## Technology Stack
- **Language:** C# (.NET Framework)
- **UI Framework:** WPF
- **Design Pattern:** MVVM (adapted for WPF)
- **Communication:** MQTT (customized M2MQTT)
- **Database:** Microsoft SQL Server (optional file-based logging)
- **Configuration Files:** XML (tags, alarms, users, system settings)
- **Data Access:** LINQ
- **Reporting:** Grid & chart-based reporting system

---

## How to Run
1. Clone the repository:
   ```bash
   git clone https://github.com/Quirin176/MQTT_SCADA_WPF_DotNETFramework.git
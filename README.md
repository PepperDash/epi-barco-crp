# epi-barco-control_room_proxy
<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 1.9.7
<!-- END Minimum Essentials Framework Versions -->
<!-- START Config Example -->
### Config Example

```json
{
    "key": "GeneratedKey",
    "uid": 1,
    "name": "GeneratedName",
    "type": "BarcoCms",
    "group": "Group",
    "properties": {
        "HostId": "SampleString",
        "DisplayID": "SampleString",
        "DefaultPerspective": "SampleString",
        "NumberOfTiles": 0
    }
}
```
<!-- END Config Example -->
<!-- START Supported Types -->
### Supported Types

- BarcoCms
- barcocrp
- barcocms
<!-- END Supported Types -->
<!-- START Join Maps -->
### Join Maps

#### Digitals

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Is Online |
| 1 | R | Poll |

#### Analogs

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Socket SocketStatus |
| 2 | R | Monitor Status |

#### Serials

| Join | Type (RW) | Description |
| --- | --- | --- |
| 2 | R | LoadPerspective |
| 10 | R | LoadSource |
| 1 | R | Device Name |
<!-- END Join Maps -->
<!-- START Interfaces Implemented -->
### Interfaces Implemented

- ICommunicationMonitor
- IBridgeAdvanced
<!-- END Interfaces Implemented -->
<!-- START Base Classes -->
### Base Classes

- EssentialsBridgeableDevice
- JoinMapBaseAdvanced
<!-- END Base Classes -->
<!-- START Public Methods -->
### Public Methods

- public void SendCommand(string command)
- public void SendCommand(string command, string[] args)
- public void LoadPerspective(string preset)
- public void LoadSource(string source, int tile)
- public void GetCurrentRoutes()
- public void GetCurrentPerspective()
- public void Poll()
<!-- END Public Methods -->
<!-- START Bool Feedbacks -->

<!-- END Bool Feedbacks -->
<!-- START Int Feedbacks -->

<!-- END Int Feedbacks -->
<!-- START String Feedbacks -->
### String Feedbacks

- Feedback
<!-- END String Feedbacks -->

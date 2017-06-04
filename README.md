# Local Detect

This softe use for local area network device detect.

# Software Workflow

* PC C# progam send UDP broadcast to local area network;
* Device get UDP broadcast IP, send MAC address to PC C# progam with the broadcast IP;
* PC C# progam refresh the listview and you can see the device MAC, IP in UI;

# Software Desription 

## Server

* [Windows Server](server/LocalDetect): 

## Client

* [Python Client](client/LocalDetect.py): Working in Embeded Linux OS;
* [Windows Client Test](server/LocalDetectTest): Working in Windows;

# C# Progam UI
![](docs/image/localDetect.png)

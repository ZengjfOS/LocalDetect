#!/usr/bin/env python
# -*- coding: utf-8 -*-

import socket
import time
import os

runWhileTimes = 0

while True:
    runWhileTimes += 1

    mac = "";
    for line in os.popen("/sbin/ifconfig"):
        if 'ether' in line:
            mac = line.split()[1]
            break

    address = ('255.255.255.255', 50000)
    socketClient = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    socketClient.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
    try:
        socketClient.sendto(mac.encode('utf-8'), address)
    except:
        print("Please check your network.")
        time.sleep(1)
        continue

    address = ('0.0.0.0', 50001)
    socketServer = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    socketServer.settimeout(3);
    socketServer.bind(address)

    TCPPort = "";
    addr = ""
    try:
        TCPPort, addr = socketServer.recvfrom(2048)
        print("received:", TCPPort, "from", addr)
    except:
        print("client has exist")
        continue

    clientSock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    clientSock.settimeout(1)
    try:
        print("%s, %d" % (addr[0], int(TCPPort.decode("utf-8"))))
        clientSock.connect((addr[0], int(TCPPort.decode("utf-8"))))
    except:
        print("connect failed.")
        continue

    sendDataCount = 0
    while 1:
        try:
            sendlen = clientSock.send(bytes("ok", encoding="utf-8"))
            try:
                recvData = clientSock.recv(1024)
            except:
                print("receive data after ")

            print("runWhileTimes(%d) send data (%d) after %d" % (runWhileTimes, sendDataCount, sendlen))

            # sendDataCount += 1
            # if sendDataCount > 10 :
            #     break

            time.sleep(1)
        except:
            print("send data failed.")
            break;

    print("disconnet server")
    clientSock.close()

    time.sleep(5)
    print("over")

socketClient.close()

# if __name__ == "__main__" :
#     sys.exit(main())

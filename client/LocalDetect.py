#!/usr/bin/env python
# -*- coding: utf-8 -*-

import socket
import os
import sys

def main():

    mac = "";
    for line in os.popen("/sbin/ifconfig"):
        if 'ether' in line:
            mac = line.split()[1]
            break

    while True:

        address = ('0.0.0.0', 50000)
        socketServer = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        socketServer.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        socketServer.setblocking(True)
        socketServer.bind(address)

        try:
            data, addr = socketServer.recvfrom(2048)
            # print("received:", data, "from", addr)
        except ValueError:
            print(ValueError)
            continue

        address = (addr[0], 50001)
        # print(address)
        socketClient = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        try:
            socketClient.sendto(mac.encode(), address)
        except ValueError:
            print(ValueError)
            continue

if __name__ == "__main__":
    sys.exit(main())

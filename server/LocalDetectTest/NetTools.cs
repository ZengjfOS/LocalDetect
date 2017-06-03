using System;
using System.Collections.Generic;
using System.Text;
using System.Net.NetworkInformation; 

namespace LocalDetectTest
{
    class NetTools
    {
        /// <summary>
        /// Finds the MAC address of the NIC with maximum speed.
        /// </summary>
        /// <returns>The MAC address.</returns>
        public static void PrintAllMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                Console.WriteLine(
                    "Name: " + nic.Name + 
                    " Found MAC Address: " + nic.GetPhysicalAddress() +
                    " Type: " + nic.NetworkInterfaceType);

                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    Console.WriteLine("New Max Speed = " + nic.Speed + ", MAC: " + tempMac);
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            // return macAddress;
        }

        /// <summary>
        /// Finds the MAC address of the first operation NIC found.
        /// </summary>
        /// <returns>The MAC address.</returns>
        public static string GetFirstMacAddress()
        {
            string macAddresses = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    Console.WriteLine(macAddresses);
                    break;
                }
            }

            return macAddresses;
        }
    }
}

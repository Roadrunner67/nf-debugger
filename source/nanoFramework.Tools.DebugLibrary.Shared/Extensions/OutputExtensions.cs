﻿//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Tools.Debugger.WireProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace nanoFramework.Tools.Debugger.Extensions
{
    public static class OutputExtensions
    {
        /// <summary>
        /// Prints a nicely formatted output of a MemoryMap array.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static string ToStringForOutput(this List<Commands.Monitor_MemoryMap.Range> range)
        {
            StringBuilder output = new StringBuilder();

            try
            {
                if (range != null && range.Count > 0)
                {
                    // header
                    output.AppendLine("++++++++++++++++++++++++++++++++");
                    output.AppendLine("++        Memory Map          ++");
                    output.AppendLine("++++++++++++++++++++++++++++++++");
                    output.AppendLine("  Type     Start       Size");
                    output.AppendLine("++++++++++++++++++++++++++++++++");

                    foreach (Commands.Monitor_MemoryMap.Range item in range)
                    {
                        string mem = "";
                        switch (item.m_flags)
                        {
                            case Commands.Monitor_MemoryMap.c_FLASH:
                                mem = "  FLASH";
                                break;
                            case Commands.Monitor_MemoryMap.c_RAM:
                                mem = "  RAM  ";
                                break;
                        }

                        output.AppendLine(string.Format("{0,-6} 0x{1:x08}  0x{2:x08}", mem, item.m_address, item.m_length));
                    }
                    return output.ToString();
                }

                return "Empty map data.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception when parsing memory map data: {ex.Message + Environment.NewLine + ex.StackTrace}");
            }

            return "Exception when trying to parse memory map data.";
        }

        public static string ToStringForOutput(this List<Commands.Monitor_FlashSectorMap.FlashSectorData> range)
        {
            StringBuilder output = new StringBuilder();

            try
            {
                if (range != null && range.Count > 0)
                {
                    // header
                    output.AppendLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                    output.AppendLine("++                   Flash Sector Map                        ++");
                    output.AppendLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                    
                    // output in list format, ordered by region
                    output.AppendLine("  Region     Start      Blocks   Bytes/Block    Usage");
                    output.AppendLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

                    int i = 0;

                    foreach (Commands.Monitor_FlashSectorMap.FlashSectorData item in range)
                    {
                        output.AppendLine($"{string.Format("{0,7}", i++)}    {string.Format("0x{0:X08}", item.m_StartAddress)}   {string.Format("{0,5}", item.m_NumBlocks)}      {string.Format("0x{0:X06}", item.m_BytesPerBlock)}     {item.UsageAsString()}");
                    }

                    output.AppendLine();
                    output.AppendLine();

                    // header
                    output.AppendLine("+++++++++++++++++++++++++++++++++++++++++++++++++++");
                    output.AppendLine("++              Storage Usage Map                ++");
                    output.AppendLine("+++++++++++++++++++++++++++++++++++++++++++++++++++");

                    // output in list format, ordered by deployment usage

                    output.AppendLine("  Start        Size (kB)           Usage");
                    output.AppendLine("+++++++++++++++++++++++++++++++++++++++++++++++++++");

                    // output nanoBooter, only if it's available on the target
                    if (range.Exists(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_BOOTSTRAP))
                    {
                        output.AppendLine(
                            $" {string.Format(" 0x{0:X08}", range.First(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_BOOTSTRAP).m_StartAddress)}" +
                            $"   {string.Format(" 0x{0:X06}", range.Where(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_BOOTSTRAP).Sum(obj => obj.m_NumBlocks * obj.m_BytesPerBlock))}" +
                            $" {range.Where(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_BOOTSTRAP).Sum(obj => obj.m_NumBlocks * obj.m_BytesPerBlock).ToMemorySizeFormart()}" +
                            $"   {range.First(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_BOOTSTRAP).UsageAsString()}");
                    }

                    // output config line only if it's available on the target
                    if (range.Count(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CONFIG) > 0)
                    {
                        output.AppendLine(
                            $" {string.Format(" 0x{0:X08}", range.First(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CONFIG).m_StartAddress)}" +
                            $"   {string.Format(" 0x{0:X06}", range.Where(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CONFIG).Sum(obj => obj.m_NumBlocks * obj.m_BytesPerBlock))}" +
                            $" {range.Where(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CONFIG).Sum(obj => obj.m_NumBlocks * obj.m_BytesPerBlock).ToMemorySizeFormart()}" +
                            $"   {range.First(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CONFIG).UsageAsString()}");
                    }

                    // nanoCLR
                    output.AppendLine(
                        $" {string.Format(" 0x{0:X08}", range.First(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CODE).m_StartAddress)}" +
                        $"   {string.Format(" 0x{0:X06}", range.Where(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CODE).Sum(obj => obj.m_NumBlocks * obj.m_BytesPerBlock))}" +
                        $" {range.Where(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CODE).Sum(obj => obj.m_NumBlocks * obj.m_BytesPerBlock).ToMemorySizeFormart()}" +
                        $"   {range.First(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CODE).UsageAsString()}");

                    // deployment
                    output.AppendLine(
                        $" {string.Format(" 0x{0:X08}", range.First(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT).m_StartAddress)}" +
                        $"   {string.Format(" 0x{0:X06}", range.Where(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT).Sum(obj => obj.m_NumBlocks * obj.m_BytesPerBlock))}" +
                        $" {range.Where(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT).Sum(obj => obj.m_NumBlocks * obj.m_BytesPerBlock).ToMemorySizeFormart()}" +
                        $"   {range.First(item => (item.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT).UsageAsString()}");

                    return output.ToString();
                }

                return "Empty map data.";
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception when parsing flash map data: {ex.Message + Environment.NewLine + ex.StackTrace}");
            }

            return "Exception when trying to parse flash map data.";
        }

        public static string ToStringForOutput(this List<Commands.Monitor_DeploymentMap.DeploymentData> range)
        {
            StringBuilder output = new StringBuilder();

            try
            {
                if (range != null && range.Count > 0)
                {
                    int i = 0;

                    foreach (Commands.Monitor_DeploymentMap.DeploymentData item in range)
                    {
                        output.AppendLine("Assembly " + i++);
                        output.AppendLine("  Address: " + item.m_address.ToString());
                        output.AppendLine("  Size   : " + item.m_size.ToString());
                        output.AppendLine("  CRC    : " + item.m_CRC.ToString());
                    }

                    return output.ToString();
                }
                else
                {
                    return "No deployed assemblies";
                }
            }
            catch { }

            return "Invalid or empty map data.";
        }

        public static string ToStringForOutput(this DeviceConfiguration.NetworkConfigurationProperties networkConfiguration)
        {
            StringBuilder output = new StringBuilder();

            if (networkConfiguration.StartupAddressMode != AddressMode.Invalid)
            {
                output.AppendLine("IPv4 configuration");
                output.AppendLine("++++++++++++++++++++++++++++++++++++");
                output.AppendLine($"address: {networkConfiguration.IPv4Address.ToString()}");
                output.AppendLine($"subnet mask: {networkConfiguration.IPv4NetMask.ToString()}");
                output.AppendLine($"gateway: {networkConfiguration.IPv4GatewayAddress.ToString()}");
                output.AppendLine($"DNS server 1: {networkConfiguration.IPv4DNSAddress1.ToString()}");
                output.AppendLine($"DNS server 2: {networkConfiguration.IPv4DNSAddress2.ToString()}");

                output.AppendLine("");
                output.AppendLine("IPv6 configuration");
                output.AppendLine("++++++++++++++++++++++++++++++++++++");
                output.AppendLine($"address: {networkConfiguration.IPv6Address.ToString()}");
                output.AppendLine($"subnet mask: {networkConfiguration.IPv6NetMask.ToString()}");
                output.AppendLine($"gateway: {networkConfiguration.IPv6GatewayAddress.ToString()}");
                output.AppendLine($"DNS server 1: {networkConfiguration.IPv6DNSAddress1.ToString()}");
                output.AppendLine($"DNS server 2: {networkConfiguration.IPv6DNSAddress2.ToString()}");

                output.AppendLine("");
                output.Append("IP configuration: ");

                switch (networkConfiguration.StartupAddressMode)
                {
                    case AddressMode.Static:
                        output.AppendLine("IP configuration: Static");
                        break;

                    case AddressMode.DHCP:
                        output.AppendLine("IP configuration: DHCP");
                        break;

                    case AddressMode.AutoIP:
                        output.AppendLine("IP configuration: auto IP");
                        break;
                }

                return output.ToString();
            }
            else
            {
                return "IP configuration is invalid";
            }
        }

        private static string ToMemorySizeFormart(this long value)
        {
            // divide by 1kB size (binary)
            uint sizeInkB = (uint)value / 1024;

            string output;

            // value is in the kB range
            output = $"({sizeInkB.ToString()}kB)";

            // output width has to be '(0000kB)'
            string spacer = new string(new char[8 - output.Length]);
            return (output + spacer.Replace('\0', ' '));
        }
    }
}

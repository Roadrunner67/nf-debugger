﻿//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//
using nanoFramework.Tools.Debugger.Extensions;
using nanoFramework.Tools.Debugger.Serial;
using nanoFramework.Tools.Debugger.WireProtocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace nanoFramework.Tools.Debugger.PortSerial
{
    public partial class SerialPortManager : PortBase
    {
        /// <summary>
        /// Creates an Serial debug client
        /// </summary>
        public SerialPortManager(Application callerApp, bool startDeviceWatchers = true)
        {
            _mapDeviceWatchersToDeviceSelector = new Dictionary<DeviceWatcher, String>();
            NanoFrameworkDevices = new ObservableCollection<NanoDeviceBase>();
            _serialDevices = new List<Serial.SerialDeviceInformation>();

            // set caller app property
            CallerApp = callerApp;

            Task.Factory.StartNew(() =>
            {
                if (startDeviceWatchers)
                {
                    StartSerialDeviceWatchers();
                }
            });
        }
    }
}

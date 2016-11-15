// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace TMD26721
{
	struct Proximity
	{
		public double PROX;
	};

	// App that reads data over I2C from an TMD26721 Proximity Sensor
	public sealed partial class MainPage : Page
	{
		private const byte PROX_I2C_ADDR = 0x39;		// I2C address of the TMD26721
		private const byte PROX_REG_COMMAND = 0x80;		// Command register
		private const byte PROX_REG_ENABLE = 0x00;		// Enables states and interrupt register
        	private const byte PROX_REG_PTIME = 0x02;		// Proximity ADC time register
		private const byte PROX_REG_WTIME = 0x03;		// Wait time register
        	private const byte PROX_REG_PPULSE = 0x0E;      	// Proximity Pulse register
        	private const byte PROX_REG_CONTROL = 0x0F;		// Control register
		private const byte PROX_REG_PDATA = 0x18;		// Proximity ADC low data register

		private I2cDevice I2CProximity;
		private Timer periodicTimer;

		public MainPage()
		{
			this.InitializeComponent();

			// Register for the unloaded event so we can clean up upon exit
			Unloaded += MainPage_Unloaded;

			// Initialize the I2C bus, Proximity Sensor, and timer
			InitI2CProximity();
		}

		private async void InitI2CProximity()
		{
			string aqs = I2cDevice.GetDeviceSelector();		// Get a selector string that will return all I2C controllers on the system
			var dis = await DeviceInformation.FindAllAsync(aqs);	// Find the I2C bus controller device with our selector string
			if (dis.Count == 0)
			{
				Text_Status.Text = "No I2C controllers were found on the system";
				return;
			}

			var settings = new I2cConnectionSettings(PROX_I2C_ADDR);
			settings.BusSpeed = I2cBusSpeed.FastMode;
			I2CProximity = await I2cDevice.FromIdAsync(dis[0].Id, settings);	// Create an I2C Device with our selected bus controller and I2C settings
			if (I2CProximity == null)
			{
				Text_Status.Text = string.Format(
					"Slave address {0} on I2C Controller {1} is currently in use by " +
					"another application. Please ensure that no other applications are using I2C.",
				settings.SlaveAddress,
				dis[0].Id);
				return;
			}

			/*
				Initialize the Proximity Sensor:
				For this device, we create 2-byte write buffers:
				The first byte is the register address we want to write to
				The second byte is the contents that we want to write to the register
			*/
			byte[] WriteBuf_Enable = new byte[] { PROX_REG_ENABLE | PROX_REG_COMMAND, 0x0D };	// 0x0D sets Power ON, Wait and Proximity features are enabled
			byte[] WriteBuf_Ptime = new byte[] { PROX_REG_PTIME | PROX_REG_COMMAND, 0xFF };		// 0x00 sets PTIME : 2.73 ms, 1 cycle, 1023 max count
			byte[] WriteBuf_Wtime = new byte[] { PROX_REG_WTIME | PROX_REG_COMMAND, 0xFF };		// 0xFF sets WTIME : 2.73 ms (WLONG = 0), 1 wait time
            	byte[] WriteBuf_Ppulse = new byte[] { PROX_REG_PPULSE | PROX_REG_COMMAND, 0x20 };       	// 0x20 sets Proximity pulse count to 32
            	byte[] WriteBuf_Control = new byte[] { PROX_REG_CONTROL | PROX_REG_COMMAND, 0x20 };		// 0x20 sets Proximity uses CH1 diode, Proximity gain 1x

			// Write the register settings
			try
			{
				I2CProximity.Write(WriteBuf_Enable);
				I2CProximity.Write(WriteBuf_Ptime);
				I2CProximity.Write(WriteBuf_Wtime);
                		I2CProximity.Write(WriteBuf_Ppulse);
                		I2CProximity.Write(WriteBuf_Control);
			}
			// If the write fails display the error and stop running
			catch (Exception ex)
			{
				Text_Status.Text = "Failed to communicate with device: " + ex.Message;
				return;
			}

			// Create a timer to read data every 800ms
			periodicTimer = new Timer(this.TimerCallback, null, 0, 800);
		}

		private void MainPage_Unloaded(object sender, object args)
		{
			// Cleanup
			I2CProximity.Dispose();
		}

		private void TimerCallback(object state)
		{
			string proxText;
			string addressText, statusText;

			// Read and format Proximity Sensor data
			try
			{
				Proximity prox = ReadI2CProximity();
				addressText = "I2C Address of the Proximity Sensor TMD26721: 0x39";
				proxText = String.Format("Proximity of the Device: {0:F0}", prox.PROX);
				statusText = "Status: Running";
			}
			catch (Exception ex)
			{
				proxText = "Proximity of the Device: Error";
				statusText = "Failed to read from Proximity Sensor: " + ex.Message;
			}

			// UI updates must be invoked on the UI thread
			var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				Text_Proximity_of_the_Device.Text = proxText;
				Text_Status.Text = statusText;
			});
		}

		private Proximity ReadI2CProximity()
		{
			byte[] RegAddrBuf = new byte[] { PROX_REG_PDATA | PROX_REG_COMMAND };	// Read data from the register address
			byte[] ReadBuf = new byte[2];						// We read 2 bytes sequentially to get two-byte data registers in one read

			/*
				Read from the Proximity Sensor 
				We call WriteRead() so we first write the address of the Proximity data low register, then read all 2 values
			*/
			I2CProximity.WriteRead(RegAddrBuf, ReadBuf);

			/*
				In order to get the raw 16-bit data values, we need to concatenate two 8-bit bytes from the I2C read
			*/
			ushort proximity = (ushort)(ReadBuf[0] & 0xFF);
			proximity |= (ushort)((ReadBuf[1] & 0xFF) * 256);

			Proximity prox;
			prox.PROX = proximity;

			return prox;
		}
	}
}

// Distributed with a free-will license.
// Use it any way you want, profit or free, provided it fits in the licenses of its associated works.
// TMD26721
// This code is designed to work with the TMD26721_I2CS I2C Mini Module available from ControlEverything.com.
// https://www.controleverything.com/content/Proximity?sku=TMD26721_I2CS#tabs-0-product_tabset-2

import com.pi4j.io.i2c.I2CBus;
import com.pi4j.io.i2c.I2CDevice;
import com.pi4j.io.i2c.I2CFactory;
import java.io.IOException;

public class TMD26721
{
	public static void main(String args[]) throws Exception
	{
		// Create I2C bus
		I2CBus bus = I2CFactory.getInstance(I2CBus.BUS_1);
		// Get I2C device, TMD26721 I2C address is 0x39(57)
		I2CDevice device = bus.getDevice(0x39);

		// Select proximity time register OR with command register
		// Ptime = 2.73 ms
		device.write(0x02 | 0x80, (byte)0xFF);
		// Select wait time register OR with command register
		// Wtime = 2.73 ms
		device.write(0x03 | 0x80, (byte)0xFF);
		// Select pulse count register OR with command register
		// Pulse count = 32
		device.write(0x0E | 0x80, (byte)0x20);
		// Select control register OR with command register
		// 100 mA LED strength, proximtiy uses CH1 diode, 1x PGAIN, 1x AGAIN
		device.write(0x0F | 0x80, (byte)0x20);
		// Select enable register OR with command register
		// Set Power ON, proximity and wait enabled
		device.write(0x00 | 0x80, (byte)0x0D);
		Thread.sleep(800);

		// Read 2 bytes of data from address 0x18(24)
		// proximity lsb, proximity msb
		byte[] data = new byte[2];
		device.read(0x18 | 0x80, data, 0, 2);

		// Convert the data
		int proximity = (((data[1] & 0xFF) * 256)+ (data[0] & 0xFF));
		
		// Output data to screen
		System.out.printf("Proximity Of the Device : %d %n", proximity);
	}
}

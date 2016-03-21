# Distributed with a free-will license.
# Use it any way you want, profit or free, provided it fits in the licenses of its associated works.
# TMD26721
# This code is designed to work with the TMD26721_I2CS I2C Mini Module available from ControlEverything.com.
# https://www.controleverything.com/content/Proximity?sku=TMD26721_I2CS#tabs-0-product_tabset-2

import smbus
import time

# Get I2C bus
bus = smbus.SMBus(1)

# TMD26721 address, 0x39(57)
# Select enable register register, 0x00(0), with command register 0x80(128)
#		0x0D(14)	Power on, Wait enabled, Proximity enabled
bus.write_byte_data(0x39, 0x00 | 0x80, 0x0D)
# TMD26721 address, 0x39(57)
# Select proximity time control register, 0x02(2), with command register 0x80(128)
#		0xFF(255)	Time = 2.73 ms
bus.write_byte_data(0x39, 0x02 | 0x80, 0xFF)
# TMD26721 address, 0x39(57)
# Select wait time register 0x03(03), with command register, 0x80(128)
#		0xFF(255)	Time - 2.73ms
bus.write_byte_data(0x39, 0x03 | 0x80, 0xFF)
# TMD26721 address, 0x39(57
# Select pulse count register, 0x0E(14), with command register 0x80(128)
#		0x20(32)	Pulse count = 32
bus.write_byte_data(0x39, 0x0E | 0x80, 0x20)
# TMD26721 address, 0x39(57
# Select control register, 0x0F(15), with command register 0x80(128)
#		0x20(32)	Proximity uses CH1 diode
bus.write_byte_data(0x39, 0x0F | 0x80, 0x20)

time.sleep(0.8)

# TMD26721 address, 0x39(57)
# Read data back from 0x18(57) with command register 0x80(128), 2 bytes
# Proximity lsb, Proximity msb
data = bus.read_i2c_block_data(0x39, 0x18 | 0x80, 2)

# Convert the data
proximity = data[1] * 256 + data[0]

# Output data to screen
print "Proximity of the Device : %d" %proximity

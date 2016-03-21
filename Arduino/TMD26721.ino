// Distributed with a free-will license.
// Use it any way you want, profit or free, provided it fits in the licenses of its associated works.
// TMD26721
// This code is designed to work with the TMD26721_I2CS I2C Mini Module available from ControlEverything.com.
// https://www.controleverything.com/content/Proximity?sku=TMD26721_I2CS#tabs-0-product_tabset-2

#include <Wire.h>

// I2C address of the TMD26721 0x39(57)
#define Addr 0x39

void setup()
{
  // Initialise I2C communication as MASTER
  Wire.begin();
  // Initialise Serial Communication, set baud rate = 9600
  Serial.begin(9600);
  
  // Start I2C Transmission
  Wire.beginTransmission(Addr);
  // Select enable register
  Wire.write(0x00 | 0x80);
  // Set power on, proximity and wait enabled
  Wire.write(0x0D);
  // Stop I2C Transmission
  Wire.endTransmission();
  delay(500);

  // Start I2C Transmission
  Wire.beginTransmission(Addr);
  // Select proximity time control register
  Wire.write(0x02 | 0x80);
  // time = 2.73 ms
  Wire.write(0xFF);
  // Stop I2C Transmission
  Wire.endTransmission();

  // Start I2C Transmission
  Wire.beginTransmission(Addr);
  // Select proximity time register
  Wire.write(0x0F | 0x80);
  // Proximity uses Ch0 diode
  Wire.write(0x20);
  // Stop I2C Transmission
  Wire.endTransmission();
  delay(300);
}

void loop()
{
  unsigned int data[0];
  
  // Start I2C Transmission
  Wire.beginTransmission(Addr);
  // Select data register
  Wire.write(0x18 | 0x80);
  // Stop I2C Transmission
  Wire.endTransmission();
  
  // Request 2 bytes of data
  Wire.requestFrom(Addr, 2);
  
  // Read 2 bytes of data
  // proximity lsb, proximity msb
  if(Wire.available() == 2)
  {
    data[0] = Wire.read();
    data[1] = Wire.read();
  }

  // Convert the data
  int proximity = data[1] * 256 + data[0];

  // Output data to serial monitor
  Serial.print("Proximity of the device : ");
  Serial.println(proximity);
  delay(1000);
}


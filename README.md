# LED Matrix Controller

A C# serial port application and Arduino code to drive a MAX7219 LED Matrix module from an Arduino microcontroller

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

- Visual Studio 2017
- Arduino IDE v1.8.5+
- LedControl library https://playground.arduino.cc/Main/LedControl

## Deployment

- Load the sketch in the LedMatrix.Arduino folder into the Arduino IDE
- Press Ctrl-U to compile and upload the sketch to a Arduino Uno R3 controller board

- Run the LedMatrix.Client C# application in Visual Studio
- Select a COM port, and speed and click 'Open' to send commands to the controller

## License

This project is licensed under the ISC License - see the [LICENSE](LICENSE) file for details


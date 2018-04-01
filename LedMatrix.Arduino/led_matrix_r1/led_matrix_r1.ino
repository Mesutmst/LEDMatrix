#include <LedControl.h>
#include <stdlib.h>

/* LED Matrix Pins */
#define LMX_DIN 12
#define LMX_CLK 10
#define LMX_CS 11
/* LED Matrix Count */
#define LMX_COUNT 3

LedControl ctrl = LedControl(LMX_DIN, LMX_CLK, LMX_CS, LMX_COUNT);

typedef struct {
  byte mx_hdr;
  byte mx_id;
  byte mx_data[8];
} MatrixFrame;

MatrixFrame currentFrame = { 0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00 };
MatrixFrame nextFrame = { 0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00 };

MatrixFrame testFrame1 = { 0x0F,0xF0,0x0F,0xF0,0x0F,0xF0,0x0F,0xF0 };
MatrixFrame testFrame2 = { 0xF0,0x0F,0xF0,0x0F,0xF0,0x0F,0xF0,0x0F };

void mxCopyFrame(MatrixFrame& srcFrame, MatrixFrame& destFrame)
{
  destFrame.mx_hdr = srcFrame.mx_hdr;
  destFrame.mx_id = srcFrame.mx_id;
  for(int i = 0; i < 8; i++) { 
    destFrame.mx_data[i] = srcFrame.mx_data[i];
  }
}

void mxPrintFrame(MatrixFrame& frame)
{
  for(int i = 0; i < 8; i++) {
    ctrl.setRow(frame.mx_id,i,frame.mx_data[i]);
  }
}

bool mxSerialReadFrame(MatrixFrame& frame)
{
  char serialData[20];
  if (Serial.readBytesUntil('\n',serialData,20) >= 20)
  {
    mxSerialParseFrame(frame, serialData);
    return true;
  }
  return false;
}

void mxSerialParseFrame(MatrixFrame& frame, char* data)
{
  frame.mx_hdr = mxSerialParseByte(data);
  frame.mx_id = mxSerialParseByte(data+2);
  for (int i = 0; i < 8; i++) {
    frame.mx_data[i] = mxSerialParseByte(data + 4 + (i*2));
  }
}

byte mxSerialParseByte(char* data)
{
   char sbuffer[3];
   sbuffer[0] = *(data);
   sbuffer[1] = *(data+1);
   sbuffer[2] = '\0';
   return (byte)strtol(sbuffer, NULL, 16);
}

void setup() {
  for (int i = 0; i < LMX_COUNT; i++)
  {
    ctrl.shutdown(i, false);
    ctrl.setIntensity(i, 5);
    ctrl.clearDisplay(i);
  }
  Serial.begin(115200);
  Serial.setTimeout(1000);
  delay(1000);
}

void loop() {
  mxSerialReadFrame(currentFrame);
  mxPrintFrame(currentFrame);
}



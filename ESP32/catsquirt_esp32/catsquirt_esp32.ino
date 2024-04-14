
// using board "ESP32 Dev Module" for ESP32 Pico kit https://www.mouser.com/ProductDetail/Espressif-Systems/ESP32-PICO-KIT?qs=MLItCLRbWsyoLrlknFRqcQ%3D%3D

//#include "BluetoothSerial.h"
#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEServer.h>
//#include <BLE2902.h>
//#include "BluetoothSerial.h"

#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"

//#define servoPWM 12
#define turn0 15
#define turn1 16

/*
double      ledcSetup(uint8_t channel, double freq, uint8_t resolution_bits);
void        ledcWrite(uint8_t channel, uint32_t duty);
double      ledcWriteTone(uint8_t channel, double freq);
double      ledcWriteNote(uint8_t channel, note_t note, uint8_t octave);
uint32_t    ledcRead(uint8_t channel);
double      ledcReadFreq(uint8_t channel);
void        ledcAttachPin(uint8_t pin, uint8_t channel);
void        ledcDetachPin(uint8_t pin);
*/

//servo settings
const int servoPWM = 15;
const int pwmFreq = 500;
const int pwmRes = 8;
const int pwmChan = 0;

int servoAPosition = 50;
int prevServoAPosition = 50;

bool SQUIRT = false;


//BluetoothSerial espBT;
#define SERVICE_UUID           "f4f80098-5ba3-11ec-bf63-0242ac130002"
#define CHARACTERISTIC_UUID_RX "f4f8061a-5ba3-11ec-bf63-0242ac130002"
#define CHARACTERISTIC_UUID_TX "f4f80778-5ba3-11ec-bf63-0242ac130002"

//int recv;
BLECharacteristic *pCharacteristic;
BLECharacteristic *pCharacteristicRX;
bool deviceConnected = false;


class RecvCallbacks: public BLECharacteristicCallbacks {
    void onWrite(BLECharacteristic *pCharacteristicRX) {
      std::string recv = pCharacteristicRX->getValue();

      if (recv.length() > 0) {
        
        //print what recv from mobile app
        Serial.print("Received: ");
        //for (int i = 0; i < recv.length(); i++) {
        //  Serial.print(recv[i]);
        //}
        Serial.println(recv.c_str());
        //Serial.println();

        //check for what inputs we receive
        if (recv.find("A") != -1) {
          
          //int servoTo = atoi(recv.erase(0).c_str());
          //Serial.print("Rotating servo to: ");
          //Serial.println(recv.substr(1, recv.length()).c_str());
          /////servoAPosition = atoi(recv.substr(1, recv.length()).c_str());
        }
        if (recv.find("SQUIRT") != -1) {
          SQUIRT = true;
          Serial.println("we squirt");
        }
      
      }
    }
};

class ServerConnectionCallbacks: public BLEServerCallbacks {
    void onConnect(BLEServer* pServer) {
      deviceConnected = true;
      Serial.println("Device Connected");
    };

    void onDisconnect(BLEServer* pServer) {
      deviceConnected = false;
      Serial.println("Device disconnected");
      pServer->getAdvertising()->start();
    }
};

void setup() {
  //setup pins
  ledcAttachPin(servoPWM, pwmChan);
  ledcSetup(pwmChan, pwmFreq, pwmRes);

  //disable brownout (probably a bad thing . . . )
  WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0);
  

  Serial.begin(9600);  
  //espBT.begin("ESP32");
  BLEDevice::init("ESP32 hh");
  BLEServer *pServer = BLEDevice::createServer();
  pServer->setCallbacks(new ServerConnectionCallbacks());

  BLEService *pService = pServer->createService(SERVICE_UUID);
  pCharacteristic = pService->createCharacteristic(
                                         CHARACTERISTIC_UUID_TX,
                                         BLECharacteristic::PROPERTY_NOTIFY |
                                         //BLECharacteristic::PROPERTY_WRITE |
                                         BLECharacteristic::PROPERTY_READ
                                       );
  //pCharacteristic->addDescriptor(new BLE2902());

  pCharacteristicRX = pService->createCharacteristic(
                                         CHARACTERISTIC_UUID_RX,
                                         BLECharacteristic::PROPERTY_WRITE
                                       );
  pCharacteristicRX->setCallbacks(new RecvCallbacks());


  pService->start();

  pServer->getAdvertising()->start();
  Serial.println("waiting for connection ...");
}

int i = 0;
float txValue = 0;
const int openValue = 120;  //angle need to press is about 30 deg
const int closeValue = 180; //
void loop() {
  if (deviceConnected) {

    // Let's convert the value to a char array:
    //char txString[8]; 
    //dtostrf(txValue, 1, 2, txString); // float_val, min_width, digits_after_decimal, char_buffer
    
//    pCharacteristic->setValue(&txValue, 1); // To send the integer value
//    pCharacteristic->setValue("Hello!"); // Sending a test message
    //pCharacteristic->setValue(txString);
    
    //pCharacteristic->notify();
    //Serial.print("Sent: ");
    //Serial.println(txString);

    if (prevServoAPosition != servoAPosition){
      //ledcWrite(pwmChan, servoAPosition);
    }

    if (SQUIRT){
      SQUIRT = false;
      Serial.println("SQUIRT GO GO GO");
      ledcWrite(pwmChan, closeValue);
      delay(500);
      ledcWrite(pwmChan, openValue);
    }
    
    //delay(15);


  } else {
    Serial.println("Device not connected. . . ");
    delay(4000);
  }

  delay(15);
  //txValue++;


  /*if(turn0 == HIGH){
    ledcWrite(pwmChan, 50);
  } else if(turn1 == HIGH){
    ledcWrite(pwmChan, 150);
  }*/

  /*for(int dutyCycle = 0; dutyCycle <= 255; dutyCycle++){   
    // changing the LED brightness with PWM
    ledcWrite(pwmChan, dutyCycle);
    delay(15);
  }

  // decrease the LED brightness
  for(int dutyCycle = 255; dutyCycle >= 0; dutyCycle--){
    // changing the LED brightness with PWM
    ledcWrite(pwmChan, dutyCycle);   
    delay(15);
  }*/
}

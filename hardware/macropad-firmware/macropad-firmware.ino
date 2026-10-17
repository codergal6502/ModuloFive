#include <inttypes.h>

#define FRAME_BEGIN 0x1D
#define FRAME_END 0xD1
#define FRAME_ESCAPE 0x1B
#define FRAME_ESCAPE_XOR 0x20

// Message Format:
//   0x1D
//   1-byte response or command type code
//   1-bit initiator, 0 for client or 1 for pad
//   31-bit identifier
//   Payload

#define INITIATOR_MASK 0b10000000
#define INITIATOR_PAD 0b10000000
#define INITIATOR_CLIENT 0b00000000
#define IDENTIFIER_FIRST_BYTE_MASK 0b01111111

// Identification Payload Format:
// 16-byte product ID
// 3-byte version number
// 16-byte serial ID
#define TYPE_CODE_IDENTIFICATION 0x99

// ASCII Log Payload Format:
// any number of bytes
#define TYPE_CODE_ASCII_LOG 0xEE

// Acknowledgement (No Payload)
#define TYPE_CODE_ACKNOWLEDGE 0x88

// Switch State Change
// 1-bit switch state
// 7-bit switch number
#define TYPE_CODE_SWITCH_STATE_CHANGE 0x77

// Two 128-bit numbers are used to identify the macro pad.
// The first (4f396736-45a9-44bf-97f3-192b00747d65) indicates that this device is one of my macropads.
// The second (b7f36474-b57b-45d2-bfbe-1adec9194a21) identifies the specific macropad.

// See https://stackoverflow.com/a/10191188 regarding byte order.
const uint8_t MACRO_PAD_PRODUCT[] = {
  0x36, 0x67, 0x39, 0x4f,
  0xa9, 0x45,
  0xbf, 0x44,
  0x97, 0xf3, 0x19, 0x2b, 0x00, 0x74, 0x7d, 0x65
};
const uint8_t VERSION_IDENTIFIER[] = { 0x00, 0x01, 0x1D };
const uint8_t MACRO_PAD_SERIAL[] = {
  0x74, 0x64, 0xf3, 0xb7,
  0x7b, 0xb5,
  0xd2, 0x45,
  0xbf, 0xbe, 0x1a, 0xde, 0xc9, 0x19, 0x4a, 0x21
};

struct DebouncePropertySet {
  int state;
  int prevRead;
  int curRead;
  int pin;
};

// response code, with space for the payload
uint8_t stateChangeToConnected[] = { 0xFE, 0xEF };
uint8_t switchChangedResponse[] = { 0xF0, 0x0F, 0x00 };

const unsigned long DEBOUNCE_DELAY = 50;

const unsigned short BUTTON_COUNT = 2;
const int BASE_PIN = 2;

DebouncePropertySet debouncePropertySet[BUTTON_COUNT];

unsigned long lastDebounceTime = 0;

const int LOOP_STATE_IDENTIFICATION = 1;
const int LOOP_STATE_CONNECTED = 2;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  for (unsigned short i = 0; i < BUTTON_COUNT; i++) {
    pinMode(BASE_PIN + i, INPUT_PULLUP);
    debouncePropertySet[i].state = digitalRead(BASE_PIN + i);
    debouncePropertySet[i].prevRead = debouncePropertySet[i].state;
  }
  Serial.begin(9600);
}

int loopState = LOOP_STATE_IDENTIFICATION;

void loop() {
  switch (loopState) {
    case LOOP_STATE_IDENTIFICATION:
      {
        identificationHandler();
        break;
      }
    case LOOP_STATE_CONNECTED:
      {
        buttonHandler();
        break;
      }
  }
}

// See https://jgtao.me/content/04-02-16/
void writeFrameHeader(uint8_t messageType, uint8_t initiator, unsigned long identifier) {
  Serial.write(FRAME_BEGIN);
  Serial.write(messageType);

  initiator = initiator & INITIATOR_MASK;

  uint8_t firstIdentifierByte = identifier >> 24;
  firstIdentifierByte = firstIdentifierByte & IDENTIFIER_FIRST_BYTE_MASK;
  firstIdentifierByte = initiator | firstIdentifierByte;

  byte identifierBytes[4];

  identifierBytes[0] = (firstIdentifierByte);
  identifierBytes[1] = (identifier >> 16);
  identifierBytes[2] = (identifier >> 8);
  identifierBytes[3] = (identifier);

  writeEscapedBytes(identifierBytes, sizeof(identifierBytes));
}

void writeFrameFooter() {
  Serial.write(FRAME_END);
  Serial.flush();
}

void writeEscapedByte(uint8_t b) {
  switch (b) {
    case FRAME_BEGIN:
    case FRAME_END:
    case FRAME_ESCAPE:
      {
        Serial.write(FRAME_ESCAPE);
        Serial.write(b ^ FRAME_ESCAPE_XOR);
        break;
      }
    default:
      {
        Serial.write(b);
        break;
      }
  }
}

void writeEscapedBytes(uint8_t payload[], unsigned int payloadLength) {
  for (int i = 0; i < payloadLength; i++) {
    uint8_t b = payload[i];
    writeEscapedByte(b);
  }
}

void quickFlash() {
  for (int i = 0; i < 5; i++) {
    digitalWrite(LED_BUILTIN, HIGH);
    delay(125);
    digitalWrite(LED_BUILTIN, LOW);
    delay(125);
  }
  delay(1000);
}

void slowFlash() {
  for (int i = 0; i < 5; i++) {
    digitalWrite(LED_BUILTIN, HIGH);
    delay(1250);
    digitalWrite(LED_BUILTIN, LOW);
    delay(1250);
  }
  delay(1000);
}

void multiFlash(int pulseCount, int iterationCount) {
  for (int j = 0; j < iterationCount; j++) {
    for (int i = 0; i < pulseCount; i++) {
      digitalWrite(LED_BUILTIN, HIGH);
      delay(125);
      digitalWrite(LED_BUILTIN, LOW);
      delay(125);
    }
    delay(1000);
  }
}

void consumeReads() {
  int read;
  do {
    read = Serial.read();
  } while(read > 0);
}

void writeAsciiLog(char message[]) {
  unsigned long messageIdentification = millis();
  writeFrameHeader(TYPE_CODE_ASCII_LOG, INITIATOR_PAD, messageIdentification);
  writeEscapedBytes(message, strlen(message));
  writeFrameFooter();
  Serial.flush();
}

const char buffer[100];

int readEscapedByte() {
  int read = Serial.read();

  if (read < 0) return read;
  if (read == FRAME_ESCAPE) {
    read = Serial.read();
    if (read < 0) return read;
    return read ^ FRAME_ESCAPE_XOR;
  } else {
    return read;
  }
}

void identificationHandler() {
  unsigned long identifierSent = 0x0102031D;
  writeFrameHeader(TYPE_CODE_IDENTIFICATION, INITIATOR_PAD, identifierSent);
  writeEscapedBytes(MACRO_PAD_PRODUCT, sizeof(MACRO_PAD_PRODUCT));
  writeEscapedBytes(VERSION_IDENTIFIER, sizeof(VERSION_IDENTIFIER));
  writeEscapedBytes(MACRO_PAD_SERIAL, sizeof(MACRO_PAD_SERIAL));
  writeFrameFooter();
  delay(125);

  if (expectAck(identifierSent)) {
    loopState = LOOP_STATE_CONNECTED;
  } else {
    writeAsciiLog("Identifier acknowledge did not arrive correctly.");
  }
}

void buttonHandler() {
  for (uint8_t i = 0; i < BUTTON_COUNT; i++) {
    debouncePropertySet[i].curRead = digitalRead(BASE_PIN + i);

    if (debouncePropertySet[i].prevRead != debouncePropertySet[i].curRead) {
      lastDebounceTime = millis();
    }

    if (millis() - lastDebounceTime > DEBOUNCE_DELAY) {
      if (debouncePropertySet[i].state != debouncePropertySet[i].curRead) {
        debouncePropertySet[i].state = debouncePropertySet[i].curRead;
        // sprintf(buffer, "{'status': 'ok', 'buttonNumber': %d, 'buttonState': %d}", i, debouncePropertySet[i].state);
        // writeAsciiLog(buffer);

        uint8_t payload = 0;
        if (debouncePropertySet[i].state == LOW) {
          payload = 0b10000000;
        }

        payload |= i;
        switchChangedResponse[2] = payload;
        unsigned long messageIdentification = millis();
        sprintf(buffer, "Button press message identifier is 0x%08" PRIX32 ".", messageIdentification);
        writeAsciiLog(buffer);
        writeFrameHeader(TYPE_CODE_SWITCH_STATE_CHANGE, INITIATOR_PAD, messageIdentification);
        writeEscapedByte(payload);
        writeFrameFooter();
        delay(25);

        expectAck(messageIdentification);
      }
    }

    debouncePropertySet[i].prevRead = debouncePropertySet[i].curRead;
  }
}



bool expectAck(uint32_t identifierExpected) {
  int read = Serial.read();
  if (read < 0) {
    writeAsciiLog("First ack read failed.");
    return false;
  }
  uint8_t expectedFrameBegin = (uint8_t)read;

  if (expectedFrameBegin != FRAME_BEGIN) {
    sprintf(buffer, "First ack expected frame begin (0x%02X) but got 0x%02X.", FRAME_BEGIN, expectedFrameBegin);
    writeAsciiLog(buffer);
    consumeReads();
    return false;
  }

  read = Serial.read();
  if (read < 0) {
    writeAsciiLog("Second ack read failed.");
    consumeReads();
    return false;
  }

  uint8_t expectedAckCode = read;

  uint8_t identifierBytes[4];
  for (int i = 0; i < 4; i++) {
    read = readEscapedByte();
    if (read < 0) {
      sprintf(buffer, "Identifier byte %d read failed.", i);
      writeAsciiLog(buffer);
      consumeReads();
      return false;
    }
    identifierBytes[i] = read;
  }

  if (read < 0) {
    return false;
  }

  uint32_t identifierEcho = ((uint32_t)identifierBytes[0] & IDENTIFIER_FIRST_BYTE_MASK) << 24;
  identifierEcho |= ((uint32_t)identifierBytes[1]) << 16;
  identifierEcho |= ((uint32_t)identifierBytes[2]) << 8;
  identifierEcho |= ((uint32_t)identifierBytes[3]);

  if (identifierExpected != identifierEcho) {
    // See https://stackoverflow.com/a/43029128.
    sprintf(buffer, "Expected identifier 0x%08" PRIX32 " but got 0x%08" PRIX32 ".", identifierExpected, identifierEcho);
    writeAsciiLog(buffer);
  }

  read = readEscapedByte();
  if (read < 0) {
    writeAsciiLog("First identification frame end read failed.");
    consumeReads();
    return false;
  }

  uint8_t expectedFrameEnd = (uint8_t)read;

  if (expectedFrameEnd != FRAME_END) {
    sprintf(buffer, "First identification ack expected frame end (0x%02X) but got 0x%02X.", FRAME_END, expectedFrameEnd);
    writeAsciiLog(buffer);
    consumeReads();
    return false;
  }

  sprintf(buffer, "Client Acknowledged 0x%08" PRIX32 ".", identifierExpected);
  writeAsciiLog(buffer);
}
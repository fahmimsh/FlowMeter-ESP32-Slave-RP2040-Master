#include "OPC.h"
#include <Arduino.h>
#include <Ethernet.h>

/************************************* OPC */

OPC::OPC() : OPCItemList(NULL), OPCItemsCount(0) {}

OPCItemType OPC::getOPCItem(const char *itemID)
{
	for (int i = 0; i < OPCItemsCount; i++){
		if (!strncmp(itemID, OPCItemList[i].itemID, SERIALCOMMAND_MAXCOMMANDLENGTH)){
			return OPCItemList[i];
		}
	}
	OPCItemType defaultItem;
	defaultItem.itemID = NULL;
	defaultItem.opcAccessRight = opc_undefined;
	defaultItem.itemType = opc_bool;
	defaultItem.ptr_callback = 0;
	return defaultItem;
}

void OPC::addItem(const char *itemID, opcAccessRights opcAccessRight, opctypes opctype, bool(*function)(const char *itemID, const opcOperation opcOP, const bool value))
{
	internaladdItem(itemID, opcAccessRight, opctype, int(function));
}

void OPC::addItem(const char *itemID, opcAccessRights opcAccessRight, opctypes opctype, byte(*function)(const char *itemID, const opcOperation opcOP, const byte value))
{
	internaladdItem(itemID, opcAccessRight, opctype, int(function));
}

void OPC::addItem(const char *itemID, opcAccessRights opcAccessRight, opctypes opctype, int(*function)(const char *itemID, const opcOperation opcOP, const int value))
{
	internaladdItem(itemID, opcAccessRight, opctype, int(function));
}

void OPC::addItem(const char *itemID, opcAccessRights opcAccessRight, opctypes opctype, float(*function)(const char *itemID, const opcOperation opcOP, const float value))
{
	internaladdItem(itemID, opcAccessRight, opctype, int(function));
}

void OPC::internaladdItem(const char *itemID, opcAccessRights opcAccessRight, opctypes opctype, int callback_function)
{
	OPCItemList = (OPCItemType *)realloc(OPCItemList, (OPCItemsCount + 1) * sizeof(OPCItemType));
	if (OPCItemList != NULL) {
		OPCItemList[OPCItemsCount].itemType = opctype;

		OPCItemList[OPCItemsCount].itemID = (char *)malloc(strlen(itemID) + 1);
		strncpy(&OPCItemList[OPCItemsCount].itemID[0], itemID, strlen(itemID) + 1);

		OPCItemList[OPCItemsCount].opcAccessRight = opcAccessRight;
		OPCItemList[OPCItemsCount].ptr_callback = callback_function;
		OPCItemsCount++;
	}
	else {
		Serial.println(F("Not enough memory"));
	}

}

/************************************* OPCEthernet */

OPCEthernet::OPCEthernet() {}

void OPCEthernet::after_setup(uint8_t listen_port)
{
	internal_ethernet_server = new EthernetServer(listen_port);
	internal_ethernet_server->begin();
}
int OPCEthernet::setup(uint8_t listen_port, uint8_t id)
{
  byte mac[][6] = {{0x30, 0xC6, 0xF7, 0x2F, 0x58, 0xB4}, {0xC0, 0x49, 0xEF, 0xF9, 0xAD, 0x10}, {0x58, 0xBF, 0x25, 0x18, 0xBA, 0x88}};
  int con = Ethernet.begin(mac[id]);
  after_setup(listen_port);
  return con;
}

void OPCEthernet::setup(uint8_t listen_port, uint8_t id, IPAddress local_ip)
{
  Ethernet.init(17);
  byte mac[][6] = {{0x30, 0xC6, 0xF7, 0x2F, 0x58, 0xB4}, {0xC0, 0x49, 0xEF, 0xF9, 0xAD, 0x10}, {0x58, 0xBF, 0x25, 0x18, 0xBA, 0x88}};
  Ethernet.begin(mac[id],local_ip);
  after_setup(listen_port);
}

void OPCEthernet::setup(uint8_t listen_port, uint8_t id, IPAddress local_ip, IPAddress dns_server)
{
  Ethernet.init(17);
  byte mac[][6] = {{0x30, 0xC6, 0xF7, 0x2F, 0x58, 0xB4}, {0xC0, 0x49, 0xEF, 0xF9, 0xAD, 0x10}, {0x58, 0xBF, 0x25, 0x18, 0xBA, 0x88}};
  Ethernet.begin(mac[id],local_ip,dns_server);
  after_setup(listen_port);
}

void OPCEthernet::setup(uint8_t listen_port, uint8_t id, IPAddress local_ip, IPAddress dns_server, IPAddress gateway)
{
  Ethernet.init(17);
  byte mac[][6] = {{0x30, 0xC6, 0xF7, 0x2F, 0x58, 0xB4}, {0xC0, 0x49, 0xEF, 0xF9, 0xAD, 0x10}, {0x58, 0xBF, 0x25, 0x18, 0xBA, 0x88}};
  Ethernet.begin(mac[id],local_ip,dns_server,gateway);  
  after_setup(listen_port);
}

void OPCEthernet::setup(uint8_t listen_port, uint8_t id, IPAddress local_ip, IPAddress dns_server, IPAddress gateway, IPAddress subnet)
{
  Ethernet.init(17);
  byte mac[][6] = {{0x30, 0xC6, 0xF7, 0x2F, 0x58, 0xB4}, {0xC0, 0x49, 0xEF, 0xF9, 0xAD, 0x10}, {0x58, 0xBF, 0x25, 0x18, 0xBA, 0x88}};
  Ethernet.begin(mac[id],local_ip,dns_server,gateway,subnet);
  after_setup(listen_port);
}
void OPCEthernet::sendOPCItemsMap()
{
	buffer[0] = '\0';
	strcat(buffer, "{\"M\":[");  // Old ItemsMap

	for (int k = 0; k < OPCItemsCount; k++) {
		if (k) strcat(buffer, ",");

		strcat(buffer, "{\"I\":\""); // old ItemId tag
		strcat(buffer, OPCItemList[k].itemID);
		strcat(buffer, "\",\"R\":\""); // old AccessRight tag

		bufPos = strlen(buffer);
		buffer[bufPos] = 48 + int(OPCItemList[k].opcAccessRight);
		buffer[bufPos + 1] = '\0';

		strcat(buffer, "\",\"T\":\""); // old ItemType tag

		bufPos = strlen(buffer);
		buffer[bufPos] = 48 + int(OPCItemList[k].itemType);
		buffer[bufPos + 1] = '\0';

		strcat(buffer, "\"}");

		if (k == OPCItemsCount - 1) strcat(buffer, "]");

		client.write((unsigned char *)buffer, strlen(buffer));
		buffer[0] = '\0';
	}

	client.write((unsigned char *) "}", 1);
}

void OPCEthernet::processClientCommand()
{
	char *p, *j;
	bool matched = false;
	bool(*bool_callback)(const char *itemID, const opcOperation opcOP, const bool value);
	byte(*byte_callback)(const char *itemID, const opcOperation opcOP, const byte value);
	int(*int_callback)(const char *itemID, const opcOperation opcOP, const int value);
	float(*float_callback)(const char *itemID, const opcOperation opcOP, const float value);

	client.println(F("HTTP/1.1 200 OK\r\nContent-Type: text/json\r\nConnection: close\r\n"));

	if (!strcmp(buffer, "itemsmap")) {
		sendOPCItemsMap();
	}
	else
	{
		p = strtok_r(buffer, "=", &j);
		if (!j[0]) {
			for (int i = 0; i < OPCItemsCount; i++) {
				if (!strcmp(buffer, OPCItemList[i].itemID)) {
					// Execute the stored handler function for the command  
					client.print(F("{\"I\":\"")); // old ItemId tag
					client.print(buffer);
					client.print(F("\",\"V\":\"")); // old ItemValue tag

					switch (OPCItemList[i].itemType) {
					case opc_bool:
						bool_callback = (bool(*)(const char *itemID, const opcOperation opcOP, const bool value))(OPCItemList[i].ptr_callback);
						client.print(bool_callback(OPCItemList[i].itemID, opc_opread, 0));
						break;
					case opc_byte:
						byte_callback = (byte(*)(const char *itemID, const opcOperation opcOP, const byte value))(OPCItemList[i].ptr_callback);
						client.print(byte_callback(OPCItemList[i].itemID, opc_opread, 0));
						break;
					case opc_int:
						int_callback = (int(*)(const char *itemID, const opcOperation opcOP, const int value))(OPCItemList[i].ptr_callback);
						client.print(int_callback(OPCItemList[i].itemID, opc_opread, 0));
						break;
					case opc_float:
						float_callback = (float(*)(const char *itemID, const opcOperation opcOP, const float value))(OPCItemList[i].ptr_callback);
						client.print(float_callback(OPCItemList[i].itemID, opc_opread, 0));
						break;
					} /* end switch */

					client.print(F("\"}"));

					matched = true;
					break;
				} /* end if */
			} /* end for */
		} /* end if */
		else
		{
			for (int i = 0; i < OPCItemsCount; i++) {
				if (!strcmp(buffer, OPCItemList[i].itemID)) {

					// Call the stored handler function for the command                          
					switch (OPCItemList[i].itemType) {
					case opc_bool:
						bool_callback = (bool(*)(const char *itemID, const opcOperation opcOP, const bool value))(OPCItemList[i].ptr_callback);
						bool_callback(OPCItemList[i].itemID, opc_opwrite, atoi(j));
						break;
					case opc_byte:
						byte_callback = (byte(*)(const char *itemID, const opcOperation opcOP, const byte value))(OPCItemList[i].ptr_callback);
						byte_callback(OPCItemList[i].itemID, opc_opwrite, atoi(j));
						break;
					case opc_int:
						int_callback = (int(*)(const char *itemID, const opcOperation opcOP, const int value))(OPCItemList[i].ptr_callback);
						int_callback(OPCItemList[i].itemID, opc_opwrite, atoi(j));
						break;
					case opc_float:
						float_callback = (float(*)(const char *itemID, const opcOperation opcOP, const float))(OPCItemList[i].ptr_callback);
						float_callback(OPCItemList[i].itemID, opc_opwrite, atof(j));
						break;
					} /* end case */

					matched = true;
					break;
				} /* end if */
			} /* end for */
		} /* end else */
	} /* end else */
}

void OPCEthernet::processOPCCommands()
{
	client = internal_ethernet_server->available();

	if (client) {
		boolean currentLineIsBlank = true;

		byte s = 0;
		boolean responsed = false;

		while (!responsed && client.connected()) {
			if (client.available()) {
				char c = client.read();

				if (c == '\n' && currentLineIsBlank) {
					processClientCommand();
					responsed = true;
				}
				else if (c == '\n') {
					currentLineIsBlank = true;
				}
				else if (c != '\r') {
					currentLineIsBlank = false;

					switch (s) {
					case 0: if (c == 'G') s++; break;
					case 1: if (c == 'E') s++; else s = 0; break;
					case 2: if (c == 'T') s++; else s = 0; break;
					case 3: if (c == ' ') s++; else s = 0; break;
					case 4: if (c == '/') { s++; bufPos = 0; }
							else s = 0; break;
					case 5: if (c != ' ') {
						buffer[bufPos++] = c;
						buffer[bufPos] = '\0';
					}
							else s = 0;
					}
				}
			}
		}
		client.stop(); // Cierra la conexión.
	}
}
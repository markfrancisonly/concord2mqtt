using System;
using System.Collections.Generic;

using System.Text;

namespace Automation.Concord
{
    /// <summary>
    /// Maps codes to text items using protocol table
    /// </summary>
    public static class DisplayTextCodeMap
    {
        static Dictionary<int, string> codeTextMap = new Dictionary<int, string>();

        static DisplayTextCodeMap()
        {
            #region Initialization

            codeTextMap.Add(0x00, "0");
            codeTextMap.Add(0x01, "1");
            codeTextMap.Add(0x02, "2");
            codeTextMap.Add(0x03, "3");
            codeTextMap.Add(0x04, "4");
            codeTextMap.Add(0x05, "5");
            codeTextMap.Add(0x06, "6");
            codeTextMap.Add(0x07, "7");
            codeTextMap.Add(0x08, "8");
            codeTextMap.Add(0x09, "9");
            codeTextMap.Add(0x0C, "#");
            codeTextMap.Add(0x0D, ":");
            codeTextMap.Add(0x0E, "/");
            codeTextMap.Add(0x0F, "?");
            codeTextMap.Add(0x10, ".");
            codeTextMap.Add(0x11, "A");
            codeTextMap.Add(0x12, "B");
            codeTextMap.Add(0x13, "C");
            codeTextMap.Add(0x14, "D");
            codeTextMap.Add(0x15, "E");
            codeTextMap.Add(0x16, "F");
            codeTextMap.Add(0x17, "G");
            codeTextMap.Add(0x18, "H");
            codeTextMap.Add(0x19, "I");
            codeTextMap.Add(0x1A, "J");
            codeTextMap.Add(0x1B, "K");
            codeTextMap.Add(0x1C, "L");
            codeTextMap.Add(0x1D, "M");
            codeTextMap.Add(0x1E, "N");
            codeTextMap.Add(0x1F, "O");
            codeTextMap.Add(0x20, "P");
            codeTextMap.Add(0x21, "Q");
            codeTextMap.Add(0x22, "R");
            codeTextMap.Add(0x23, "S");
            codeTextMap.Add(0x24, "T");
            codeTextMap.Add(0x25, "U");
            codeTextMap.Add(0x26, "V");
            codeTextMap.Add(0x27, "W");
            codeTextMap.Add(0x28, "X");
            codeTextMap.Add(0x29, "Y");
            codeTextMap.Add(0x2A, "Z");
            codeTextMap.Add(0x2B, " ");
            codeTextMap.Add(0x2C, "'");
            codeTextMap.Add(0x2D, "-");
            codeTextMap.Add(0x2E, "_");
            codeTextMap.Add(0x2F, "*");
            codeTextMap.Add(0x30, "AC POWER");
            codeTextMap.Add(0x31, "ACCESS");
            codeTextMap.Add(0x32, "ACCOUNT");
            codeTextMap.Add(0x33, "ALARM");
            codeTextMap.Add(0x34, "ALL");
            codeTextMap.Add(0x35, "ARM");
            codeTextMap.Add(0x36, "ARMING");
            codeTextMap.Add(0x37, "AREA");
            codeTextMap.Add(0x38, "ATTIC");
            codeTextMap.Add(0x39, "AUTO");
            codeTextMap.Add(0x3A, "AUXILIARY");
            codeTextMap.Add(0x3B, "AWAY");
            codeTextMap.Add(0x3C, "BACK");
            codeTextMap.Add(0x3D, "BATTERY");
            codeTextMap.Add(0x3E, "BEDROOM");
            codeTextMap.Add(0x3F, "BEEPS");
            codeTextMap.Add(0x40, "BOTTOM");
            codeTextMap.Add(0x41, "BREEZEWAY");
            codeTextMap.Add(0x42, "BASEMENT");
            codeTextMap.Add(0x43, "BATHROOM");
            codeTextMap.Add(0x44, "BUS");
            codeTextMap.Add(0x45, "BYPASS");
            codeTextMap.Add(0x46, "BYPASSED");
            codeTextMap.Add(0x47, "CABINET");
            codeTextMap.Add(0x48, "CANCELED");
            codeTextMap.Add(0x49, "CARPET");
            codeTextMap.Add(0x4A, "CHIME");
            codeTextMap.Add(0x4B, "CLOSET");
            codeTextMap.Add(0x4C, "CLOSING");
            codeTextMap.Add(0x4D, "CODE");
            codeTextMap.Add(0x4E, "CONTROL");
            codeTextMap.Add(0x4F, "CPU");
            codeTextMap.Add(0x50, "DEGREES");
            codeTextMap.Add(0x51, "DEN");
            codeTextMap.Add(0x52, "DESK");
            codeTextMap.Add(0x53, "DELAY");
            codeTextMap.Add(0x54, "DELETE");
            codeTextMap.Add(0x55, "DINING");
            codeTextMap.Add(0x56, "DIRECT");
            codeTextMap.Add(0x57, "DOOR");
            codeTextMap.Add(0x58, "DOWN");
            codeTextMap.Add(0x59, "DOWNLOAD");
            codeTextMap.Add(0x5A, "DOWNSTAIRS");
            codeTextMap.Add(0x5B, "DRAWER");
            codeTextMap.Add(0x5C, "DISPLAY");
            codeTextMap.Add(0x5D, "DURESS");
            codeTextMap.Add(0x5E, "EAST");
            codeTextMap.Add(0x5F, "ENERGY SAVER");
            codeTextMap.Add(0x60, "ENTER");
            codeTextMap.Add(0x61, "ENTRY");
            codeTextMap.Add(0x62, "ERROR");
            codeTextMap.Add(0x63, "EXIT");
            codeTextMap.Add(0x64, "FAIL");
            codeTextMap.Add(0x65, "FAILURE");
            codeTextMap.Add(0x66, "FAMILY");
            codeTextMap.Add(0x67, "FEATURES");
            codeTextMap.Add(0x68, "FIRE");
            codeTextMap.Add(0x69, "FIRST");
            codeTextMap.Add(0x6A, "FLOOR");
            codeTextMap.Add(0x6B, "FORCE");
            codeTextMap.Add(0x6C, "FORMAT");
            codeTextMap.Add(0x6D, "FREEZE");
            codeTextMap.Add(0x6E, "FRONT");
            codeTextMap.Add(0x6F, "FURNACE");
            codeTextMap.Add(0x70, "GARAGE");
            codeTextMap.Add(0x71, "GALLERY");
            codeTextMap.Add(0x72, "GOODBYE");
            codeTextMap.Add(0x73, "GROUP");
            codeTextMap.Add(0x74, "HALL");
            codeTextMap.Add(0x75, "HEAT");
            codeTextMap.Add(0x76, "HELLO");
            codeTextMap.Add(0x77, "HELP");
            codeTextMap.Add(0x78, "HIGH");
            codeTextMap.Add(0x79, "HOURLY");
            codeTextMap.Add(0x7A, "HOUSE");
            codeTextMap.Add(0x7B, "IMMEDIATE");
            codeTextMap.Add(0x7C, "IN SERVICE");
            codeTextMap.Add(0x7D, "INTERIOR");
            codeTextMap.Add(0x7E, "INTRUSION");
            codeTextMap.Add(0x7F, "INVALID");
            codeTextMap.Add(0x80, "IS");
            codeTextMap.Add(0x81, "KEY");
            codeTextMap.Add(0x82, "KITCHEN");
            codeTextMap.Add(0x83, "LAUNDRY");
            codeTextMap.Add(0x84, "LEARN");
            codeTextMap.Add(0x85, "LEFT");
            codeTextMap.Add(0x86, "LIBRARY");
            codeTextMap.Add(0x87, "LEVEL");
            codeTextMap.Add(0x88, "LIGHT");
            codeTextMap.Add(0x89, "LIGHTS");
            codeTextMap.Add(0x8A, "LIVING");
            codeTextMap.Add(0x8B, "LOW");
            codeTextMap.Add(0x8C, "MAIN");
            codeTextMap.Add(0x8D, "MASTER");
            codeTextMap.Add(0x8E, "MEDICAL");
            codeTextMap.Add(0x8F, "MEMORY");
            codeTextMap.Add(0x90, "MIN");
            codeTextMap.Add(0x91, "MODE");
            codeTextMap.Add(0x92, "MOTION");
            codeTextMap.Add(0x93, "NIGHT");
            codeTextMap.Add(0x94, "NORTH");
            codeTextMap.Add(0x95, "NOT");
            codeTextMap.Add(0x96, "NUMBER");
            codeTextMap.Add(0x97, "OFF");
            codeTextMap.Add(0x98, "OFFICE");
            codeTextMap.Add(0x99, "OK");
            codeTextMap.Add(0x9A, "ON");
            codeTextMap.Add(0x9B, "OPEN");
            codeTextMap.Add(0x9C, "OPENING");
            codeTextMap.Add(0x9D, "PANIC");
            codeTextMap.Add(0x9E, "PARTITION");
            codeTextMap.Add(0x9F, "PATIO");
            codeTextMap.Add(0xA0, "PHONE");
            codeTextMap.Add(0xA1, "POLICE");
            codeTextMap.Add(0xA2, "POOL");
            codeTextMap.Add(0xA3, "PORCH");
            codeTextMap.Add(0xA4, "PRESS");
            codeTextMap.Add(0xA5, "QUIET");
            codeTextMap.Add(0xA6, "QUICK");
            codeTextMap.Add(0xA7, "RECEIVER");
            codeTextMap.Add(0xA8, "REAR");
            codeTextMap.Add(0xA9, "REPORT");
            codeTextMap.Add(0xAA, "REMOTE");
            codeTextMap.Add(0xAB, "RESTORE");
            codeTextMap.Add(0xAC, "RIGHT");
            codeTextMap.Add(0xAD, "ROOM");
            codeTextMap.Add(0xAE, "SCHEDULE");
            codeTextMap.Add(0xAF, "SCRIPT");
            codeTextMap.Add(0xB0, "SEC");
            codeTextMap.Add(0xB1, "SECOND");
            codeTextMap.Add(0xB2, "SET");
            codeTextMap.Add(0xB3, "SENSOR");
            codeTextMap.Add(0xB4, "SHOCK");
            codeTextMap.Add(0xB5, "SIDE");
            codeTextMap.Add(0xB6, "SIREN");
            codeTextMap.Add(0xB7, "SLIDING");
            codeTextMap.Add(0xB8, "SMOKE");
            codeTextMap.Add(0xB9, "Sn");
            codeTextMap.Add(0xBA, "SOUND");
            codeTextMap.Add(0xBB, "SOUTH");
            codeTextMap.Add(0xBC, "SPECIAL");
            codeTextMap.Add(0xBD, "STAIRS");
            codeTextMap.Add(0xBE, "START");
            codeTextMap.Add(0xBF, "STATUS");
            codeTextMap.Add(0xC0, "STAY");
            codeTextMap.Add(0xC1, "STOP");
            codeTextMap.Add(0xC2, "SUPERVISORY");
            codeTextMap.Add(0xC3, "SYSTEM");
            codeTextMap.Add(0xC4, "TAMPER");
            codeTextMap.Add(0xC5, "TEMPERATURE");
            codeTextMap.Add(0xC6, "TEMPORARY");
            codeTextMap.Add(0xC7, "TEST");
            codeTextMap.Add(0xC8, "TIME");
            codeTextMap.Add(0xC9, "TIMEOUT");
            codeTextMap.Add(0xCA, "TOUCHPAD");
            codeTextMap.Add(0xCB, "TRIP");
            codeTextMap.Add(0xCC, "TROUBLE");
            codeTextMap.Add(0xCD, "UNBYPASS");
            codeTextMap.Add(0xCE, "UNIT");
            codeTextMap.Add(0xCF, "UP");
            codeTextMap.Add(0xD0, "VERIFY");
            codeTextMap.Add(0xD1, "VIOLATION");
            codeTextMap.Add(0xD2, "WARNING");
            codeTextMap.Add(0xD3, "WEST");
            codeTextMap.Add(0xD4, "WINDOW");
            codeTextMap.Add(0xD5, "MENU");
            codeTextMap.Add(0xD6, "RETURN");
            codeTextMap.Add(0xD7, "POUND");
            codeTextMap.Add(0xD8, "HOME");
            codeTextMap.Add(0xF9, string.Empty); //"carriage return");
            codeTextMap.Add(0xFA, string.Empty); //"pseudo space");
            codeTextMap.Add(0xFB, "\r"); //"carriage return");
            codeTextMap.Add(0xFD, string.Empty); //"backspace");
            codeTextMap.Add(0xFE, string.Empty); //"blink next token");

            #endregion
        }


        /// <summary>
        /// Returns ASCII encoded hex value string representing requested text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetAsciiHexString(string text)
        {
            StringBuilder builder = new StringBuilder();

            string[] words = text.ToUpperInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            bool insertspace = false;
            foreach (string word in words)
            {
                bool matched = false;
                foreach (KeyValuePair<int, string> entry in codeTextMap)
                {
                    if (entry.Value == word)
                    {
                        builder.Append(Message.ToAsciiHex(entry.Key));
                        matched = true;
                        insertspace = false;
                        break;
                    }
                }

                if (matched) continue;


                char[] array;

                // each word is followed by a space
                if (insertspace)
                {
                    array = (" " + text.ToUpper()).ToCharArray();
                    insertspace = false;
                }
                else
                {
                    array = text.ToUpper().ToCharArray();
                }

                foreach (char c in array)
                {
                    int asciicode = c - 0x30;

                    if (asciicode >= (int)DisplayTextCharacter.A && asciicode - 0x30 <= (int)DisplayTextCharacter.Z)
                        builder.Append(Message.ToAsciiHex(asciicode));
                    else if (asciicode >= (int)DisplayTextCharacter.Zero && asciicode <= (int)DisplayTextCharacter.Nine)
                        builder.Append(Message.ToAsciiHex(asciicode));
                    else if (c == 0x20)
                        builder.Append(Message.ToAsciiHex((int)DisplayTextCharacter.Space));
                    else
                        throw new ArgumentException("Unsupport character encountered");
                }
            }

            string result = builder.ToString();
            return result;
        }

        /// <summary>
        /// Returns english word or character value for ascii hex code. Special text items return an empty string.
        /// </summary>
        /// <param name="asciiCodePair"></param>
        /// <returns></returns>
        public static string GetText(int code)
        {
            if (codeTextMap.ContainsKey(code))
            {
                return codeTextMap[code];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns english string for ascii hex code string. Special text items are ignored.
        /// </summary>
        /// <param name="asciiHexString"></param>
        /// <returns></returns>
        public static string GetText(string asciiHexString)
        {
            if ((asciiHexString.Length % 2) != 0) throw new ArgumentException("Invalid ascii hex string length");

            //int lastTokenValue = -1;
            string text = "";

            int tokenCount = asciiHexString.Length / 2;
            int i = 0;
            while (true)
            {
                string token = asciiHexString.Substring(2 * i, 2);
                int tokenValue = Message.ToInt(token);

                string tokenText = GetText(tokenValue);

                text += tokenText;

                //add space after words
                if (tokenValue >= 0x30 && tokenValue <= 0xD8)
                {
                    text += " ";
                }
                else if (tokenValue == (int)DisplayTextSpecial.Backspace)
                {
                    text = text.Substring(0, text.Length - 1);
                }



                if (++i >= tokenCount) break;
            }

            return text.TrimEnd();
        }

        //public static string GetAsciiHexString(string text)
        //{}

    }





}

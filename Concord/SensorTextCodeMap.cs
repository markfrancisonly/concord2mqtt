using System;
using System.Collections.Generic;

namespace Automation.Concord
{
    /// <summary>
    /// Maps codes to text items using protocol table
    /// </summary>
    public static class SensorTextCodeMap
    {
        static Dictionary<string, string> userTextMap = new Dictionary<string, string>();

        static SensorTextCodeMap()
        {
            #region Initialization
            lock (userTextMap)
            {

                userTextMap.Add("ABORTED", "001");
                userTextMap.Add("AC", "002");
                userTextMap.Add("ACCESS", "003");
                userTextMap.Add("ACTIVE", "004");
                userTextMap.Add("ACTIVITY", "005");
                userTextMap.Add("ALARM", "006");
                userTextMap.Add("ALL", "007");
                userTextMap.Add("AM", "008");
                userTextMap.Add("AREA", "009");
                userTextMap.Add("ARM", "010");
                userTextMap.Add("ARMED", "011");
                userTextMap.Add("ARMING", "012");
                userTextMap.Add("ATTIC", "013");
                userTextMap.Add("AUXILLIARY", "014");
                userTextMap.Add("AWAY", "015");
                userTextMap.Add("BABY", "016");
                userTextMap.Add("BACK", "017");
                userTextMap.Add("BAR", "018");
                userTextMap.Add("BASEMENT", "019");
                userTextMap.Add("BATHROOM", "020");
                userTextMap.Add("BATTERY", "021");
                userTextMap.Add("BEDROOM", "022");
                userTextMap.Add("BOTTOM", "023");
                userTextMap.Add("BREEZEWAY", "024");
                userTextMap.Add("BUILDING", "025");
                userTextMap.Add("BUS", "026");
                userTextMap.Add("BYPASS", "027");
                userTextMap.Add("BYPASSED", "028");
                userTextMap.Add("CABINET", "029");
                userTextMap.Add("CANCELED", "030");
                userTextMap.Add("CAR", "031");
                userTextMap.Add("CARBON MONOXIDE", "032");
                userTextMap.Add("CENTRAL", "033");
                userTextMap.Add("CHIME", "034");
                userTextMap.Add("CLOSED", "035");
                userTextMap.Add("CLOSET", "036");
                userTextMap.Add("CLOSING", "037");
                userTextMap.Add("CODE", "038");
                userTextMap.Add("COMPUTER", "039");
                userTextMap.Add("CONTROL", "040");
                userTextMap.Add("DATE", "041");
                userTextMap.Add("DAUGHTERS", "042");
                userTextMap.Add("DEGREES", "043");
                userTextMap.Add("DELAY", "044");
                userTextMap.Add("DEN", "045");
                userTextMap.Add("DESK", "046");
                userTextMap.Add("DETECTOR", "047");
                userTextMap.Add("DINING", "048");
                userTextMap.Add("DISARMED", "049");
                userTextMap.Add("DOOR", "050");
                userTextMap.Add("DOWN", "051");
                userTextMap.Add("DOWNLOAD", "052");
                userTextMap.Add("DOWNSTAIRS", "053");
                userTextMap.Add("DRAWER", "054");
                userTextMap.Add("DRIVEWAY", "055");
                userTextMap.Add("DUCT", "056");
                userTextMap.Add("DURESS", "057");
                userTextMap.Add("EAST", "058");
                userTextMap.Add("ENERGY SAVER", "059");
                userTextMap.Add("ENTER", "060");
                userTextMap.Add("ENTRY", "061");
                userTextMap.Add("ERROR", "062");
                userTextMap.Add("EXIT", "063");
                userTextMap.Add("EXTERIOR", "064");
                userTextMap.Add("FACTORY", "065");
                userTextMap.Add("FAILURE", "066");
                userTextMap.Add("FAMILY", "067");
                userTextMap.Add("FATHER’S", "068");
                userTextMap.Add("FEATURE", "069");
                userTextMap.Add("FENCE", "070");
                userTextMap.Add("FIRE", "071");
                userTextMap.Add("FIRST", "072");
                userTextMap.Add("FLOOR", "073");
                userTextMap.Add("FORCE", "074");
                userTextMap.Add("FOYER", "075");
                userTextMap.Add("FREEZE", "076");
                userTextMap.Add("FRONT", "077");
                userTextMap.Add("FURNACE", "078");
                userTextMap.Add("GALLERY", "079");
                userTextMap.Add("GARAGE", "080");
                userTextMap.Add("GAS", "081");
                userTextMap.Add("GLASS", "082");
                userTextMap.Add("GOODBYE", "083");
                userTextMap.Add("HALLWAY", "084");
                userTextMap.Add("HEAT", "085");
                userTextMap.Add("HELLO", "086");
                userTextMap.Add("HELP", "087");
                userTextMap.Add("HIGH", "088");
                userTextMap.Add("HOME", "089");
                userTextMap.Add("HOUSE", "090");
                userTextMap.Add("IN", "091");
                userTextMap.Add("INSTALL", "092");
                userTextMap.Add("INTERIOR", "093");
                userTextMap.Add("INTRUSION", "094");
                userTextMap.Add("INVALID", "095");
                userTextMap.Add("IS", "096");
                userTextMap.Add("KEY", "097");
                userTextMap.Add("KIDS", "098");
                userTextMap.Add("KITCHEN", "099");
                userTextMap.Add("LATCHKEY", "100");
                userTextMap.Add("LAUNDRY", "101");
                userTextMap.Add("LEFT", "102");
                userTextMap.Add("LEVEL", "103");
                userTextMap.Add("LIBRARY", "104");
                userTextMap.Add("LIGHT", "105");
                userTextMap.Add("LIGHTS", "106");
                userTextMap.Add("LIVING", "107");
                userTextMap.Add("LOAD", "108");
                userTextMap.Add("LOADING", "109");
                userTextMap.Add("LOW", "110");
                userTextMap.Add("LOWER", "111");
                userTextMap.Add("MAIN", "112");
                userTextMap.Add("MASTER", "113");
                userTextMap.Add("MAT", "114");
                userTextMap.Add("MEDICAL", "115");
                userTextMap.Add("MEMORY", "116");
                userTextMap.Add("MENU", "117");
                userTextMap.Add("MOTHER’S", "118");
                userTextMap.Add("MOTION", "119");
                userTextMap.Add("NO", "120");
                userTextMap.Add("NORTH", "121");
                userTextMap.Add("NOT", "122");
                userTextMap.Add("NOW", "123");
                userTextMap.Add("NUMBER", "124");
                userTextMap.Add("OFF", "125");
                userTextMap.Add("OFFICE", "126");
                userTextMap.Add("OK", "127");
                userTextMap.Add("ON", "128");
                userTextMap.Add("OPEN", "129");
                userTextMap.Add("OPENING", "130");
                userTextMap.Add("PANIC", "131");
                userTextMap.Add("PARTITION", "132");
                userTextMap.Add("PATIO", "133");
                userTextMap.Add("PET", "134");
                userTextMap.Add("PHONE", "135");
                userTextMap.Add("PLEASE", "136");
                userTextMap.Add("PM", "137");
                userTextMap.Add("POLICE", "138");
                userTextMap.Add("POOL", "139");
                userTextMap.Add("PORCH", "140");
                userTextMap.Add("POWER", "141");
                userTextMap.Add("PRESS", "142");
                userTextMap.Add("PROGRAM", "143");
                userTextMap.Add("PROGRESS", "144");
                userTextMap.Add("QUIET", "145");
                userTextMap.Add("REAR", "146");
                userTextMap.Add("RECEIVER", "147");
                userTextMap.Add("REPORT", "148");
                userTextMap.Add("RF", "149");
                userTextMap.Add("RIGHT", "150");
                userTextMap.Add("ROOM", "151");
                userTextMap.Add("SAFE", "152");
                userTextMap.Add("SCHEDULE", "153");
                userTextMap.Add("SCREEN", "154");
                userTextMap.Add("SECOND", "155");
                userTextMap.Add("SENSOR", "156");
                userTextMap.Add("SERVICE", "157");
                userTextMap.Add("SHED", "158");
                userTextMap.Add("SHOCK", "159");
                userTextMap.Add("SIDE", "160");
                userTextMap.Add("SIREN", "161");
                userTextMap.Add("SLIDING", "162");
                userTextMap.Add("SMOKE", "163");
                userTextMap.Add("SON'S", "164");
                userTextMap.Add("SOUND", "165");
                userTextMap.Add("SOUTH", "166");
                userTextMap.Add("SPECIAL", "167");
                userTextMap.Add("STAIRS", "168");
                userTextMap.Add("STAY", "169");
                userTextMap.Add("SUPERVISORY", "170");
                userTextMap.Add("SYSTEM", "171");
                userTextMap.Add("TAMPER", "172");
                userTextMap.Add("TEMPERATURE", "173");
                userTextMap.Add("TEST", "174");
                userTextMap.Add("TIME", "175");
                userTextMap.Add("TO", "176");
                userTextMap.Add("TOUCHPAD", "177");
                userTextMap.Add("TROUBLE", "178");
                userTextMap.Add("UNBYPASS", "179");
                userTextMap.Add("UNIT", "180");
                userTextMap.Add("UP", "181");
                userTextMap.Add("WEST", "182");
                userTextMap.Add("WINDOW", "183");
                userTextMap.Add("ZONE", "184");
                userTextMap.Add("0", "185");
                userTextMap.Add("1", "186");
                userTextMap.Add("2", "187");
                userTextMap.Add("3", "188");
                userTextMap.Add("4", "189");
                userTextMap.Add("5", "190");
                userTextMap.Add("6", "191");
                userTextMap.Add("7", "192");
                userTextMap.Add("8", "193");
                userTextMap.Add("9", "194");
                userTextMap.Add("A", "195");
                userTextMap.Add("B", "196");
                userTextMap.Add("C", "197");
                userTextMap.Add("D", "198");
                userTextMap.Add("E", "199");
                userTextMap.Add("F", "200");
                userTextMap.Add("G", "201");
                userTextMap.Add("H", "202");
                userTextMap.Add("I", "203");
                userTextMap.Add("J", "204");
                userTextMap.Add("K", "205");
                userTextMap.Add("L", "206");
                userTextMap.Add("M", "207");
                userTextMap.Add("N", "208");
                userTextMap.Add("O", "209");
                userTextMap.Add("P", "210");
                userTextMap.Add("Q", "211");
                userTextMap.Add("R", "212");
                userTextMap.Add("S", "213");
                userTextMap.Add("T", "214");
                userTextMap.Add("U", "215");
                userTextMap.Add("V", "216");
                userTextMap.Add("W", "217");
                userTextMap.Add("X", "218");
                userTextMap.Add("Y", "219");
                userTextMap.Add("Z", "220");
                userTextMap.Add(" ", "221");
                userTextMap.Add("'", "222");
                userTextMap.Add("-", "223");
                userTextMap.Add("_", "224");
                userTextMap.Add("*", "225");
                userTextMap.Add("#", "226");
                userTextMap.Add(":", "227");
                userTextMap.Add("/", "228");
                userTextMap.Add("?", "229");

            }
            #endregion
        }


        /// <summary>
        /// Returns ASCII encoded hex value string representing requested text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string[] GetEncoding(string text)
        {
            List<string> codes = new List<string>();

            string[] words = text.ToUpperInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];

                if (word.Length > 1 && userTextMap.ContainsKey(word))
                {
                    codes.Add(userTextMap[word]);
                    continue;
                }
                else
                {

                    char[] array;

                    // each word is followed by a space
                    array = (word.ToUpper() + " ").ToCharArray();

                    foreach (char c in array)
                    {
                        string str = c.ToString();
                        if (!userTextMap.ContainsKey(str))
                            throw new ArgumentException("Unsupport character encountered");
                        else
                            codes.Add(userTextMap[str]);
                    }
                }
            }

            return codes.ToArray();
        }
    }
}




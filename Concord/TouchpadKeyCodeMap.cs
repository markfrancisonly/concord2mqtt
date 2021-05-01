using System;

using System.Text;

namespace Automation.Concord
{

    /// <summary>
    /// Touchpad key press coding helper
    /// </summary>
    public static class TouchpadKeyCodeMap
    {


        /// <summary>
        /// Returns touchpad key button press array for specified string 0-9, A-F, #, and *.
        /// </summary>
        /// <param name="numbersPoundStar">Allowed characters: 0-9, #, and *</param>
        /// <returns></returns>
        public static TouchpadKey[] GetKeypress(string numbersPoundStar)
        {
            TouchpadKey[] keys = new TouchpadKey[numbersPoundStar.Length];

            char[] array = numbersPoundStar.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                char c = array[i];
                if (Char.IsDigit(c))
                {
                    keys[i] = (TouchpadKey)(c - 48);
                }
                else if (c == 'A')
                {
                    keys[i] = TouchpadKey.KeyA;
                }
                else if (c == 'B')
                {
                    keys[i] = TouchpadKey.KeyB;
                }
                else if (c == 'C')
                {
                    keys[i] = TouchpadKey.KeyC;
                }
                else if (c == 'D')
                {
                    keys[i] = TouchpadKey.KeyD;
                }
                else if (c == 'E')
                {
                    keys[i] = TouchpadKey.KeyE;
                }
                else if (c == 'F')
                {
                    keys[i] = TouchpadKey.KeyF;
                }
                else if (c == '#')
                {
                    keys[i] = TouchpadKey.Pound;
                }
                else if (c == '*')
                {
                    keys[i] = TouchpadKey.Star;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Can only accept 0-9, A-F, #, and *");
                }

            }
            return keys;
        }

        /// <summary>
        /// Returns string for specified key button press array
        /// </summary>
        /// <param name="keys">Allowed key presses: 0-9, #, and *</param>
        /// <returns></returns>
        public static string GetString(params TouchpadKey[] keys)
        {
            StringBuilder builder = new StringBuilder();

            foreach (TouchpadKey key in keys)
            {
                int keycode = (int)key;
                if (keycode >= 0 && keycode <= 9)
                {
                    builder.Append((char)(keycode + 0x30));
                }
                else if (key == TouchpadKey.KeyA)
                {
                    builder.Append("A");
                }
                else if (key == TouchpadKey.KeyB)
                {
                    builder.Append("B");
                }
                else if (key == TouchpadKey.KeyC)
                {
                    builder.Append("C");
                }
                else if (key == TouchpadKey.KeyD)
                {
                    builder.Append("D");
                }
                else if (key == TouchpadKey.KeyE)
                {
                    builder.Append("E");
                }
                else if (key == TouchpadKey.KeyF)
                {
                    builder.Append("F");
                }
                else if (key == TouchpadKey.Star)
                {
                    builder.Append("*");
                }
                else if (key == TouchpadKey.Pound)
                {
                    builder.Append("#");
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Can only accept 0-9, A-F, #, and *.");
                }

            }
            return builder.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    static class Mathf
    {
        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
        public static int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
        public static byte Clamp(byte value, byte min, byte max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }

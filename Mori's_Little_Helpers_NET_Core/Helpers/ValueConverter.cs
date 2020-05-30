using System;
using System.IO;

namespace Schweigm_NETCore_Helpers
{
    public static class ValueConverter
    {

        #region Conversion-Functions

        public static bool ConvertBool(byte[] input)
        {
            return input[0] > 0;
        }

        public static string Convert2Bit(byte[] input)
        {
            return ((byte)(input[0] & 0b00000011)).ToString();
        }

        public static string Convert4Bit(byte[] input)
        {
            return ((byte)(input[0] & 0x0F)).ToString();
        }

        public static byte Convert8Bit(byte[] input)
        {
            return input[0];
        }

        public static ushort Convert16Bit(byte[] input)
        {
            return BitConverter.ToUInt16(input);
        }

        public static uint Convert24Bit(byte[] input)
        {
            var inputPadded = new byte[input.Length + 1];
            input.CopyTo(inputPadded, 0);
            inputPadded[3] = 0x00;
            return BitConverter.ToUInt32(inputPadded);
        }

        public static uint Convert32Bit(byte[] input)
        {
            return BitConverter.ToUInt32(input);
        }

        public static byte ConvertUint8(byte[] input)
        {
            return input[0];
        }

        public static ushort ConvertUint16(byte[] input)
        {
            return BitConverter.ToUInt16(input);
        }

        public static uint ConvertUint24(byte[] input)
        {
            var inputPadded = new byte[input.Length + 1];
            input.CopyTo(inputPadded, 0);
            inputPadded[3] = 0x00;
            var conv = BitConverter.ToUInt32(inputPadded);
            return conv;
        }

        public static uint ConvertUint32(byte[] input)
        {
            var conv = BitConverter.ToUInt32(input);
            return conv;
        }

        public static ulong ConvertUint40(byte[] input)
        {
            var inputPadded = new byte[input.Length + 3];
            input.CopyTo(inputPadded, 0);
            inputPadded[5] = 0x00;
            inputPadded[6] = 0x00;
            inputPadded[7] = 0x00;
            var conv = BitConverter.ToUInt64(inputPadded);
            return conv;
        }

        public static ulong ConvertUint48(byte[] input)
        {
            var conv = BitConverter.ToUInt64(input);
            return conv;
        }

        public static ulong ConvertUint56(byte[] input)
        {
            var inputPadded = new byte[input.Length + 1];
            input.CopyTo(inputPadded,0 );
            inputPadded[7] = new byte();

            var conv = BitConverter.ToUInt64(inputPadded);
            return conv;
        }

        public static ulong ConvertUint64(byte[] input)
        {
            var conv = BitConverter.ToUInt64(input);
            return conv;
        }

        public static string ConvertUint128(byte[] input)
        {
            return "Not Supported";
        }

        public static sbyte ConvertSint8(byte[] input)
        {
            var conv = (sbyte) input[0];
            return conv;
        }

        public static short ConvertSint16(byte[] input)
        {
            var conv = BitConverter.ToInt16(input);
            return conv;
        }

        public static int ConvertSint32(byte[] input)
        {
            var conv = BitConverter.ToInt32(input);
            return conv;
        }

        public static long ConvertSint64(byte[] input)
        {
            var conv = BitConverter.ToInt64(input);
            return conv;
        }

        public static float ConvertFloat32(byte[] input)
        {
            var conv = BitConverter.ToSingle(input, 0);
            return conv;
        }

        public static double ConvertFloat64(byte[] input)
        {
            var conv = BitConverter.ToDouble(input, 0);
            return conv;
        }

#endregion Conversion-Functions

        /// <summary>
        /// Tries to Parse the input HEX String into a Byte Array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string hex)
        {
            var numberChars = hex.Length / 2;
            var bytes = new byte[numberChars];
            using var sr = new StringReader(hex);
            for (var i = 0; i < numberChars; i++)
                bytes[i] =
                    Convert.ToByte(new string(new[] { (char)sr.Read(), (char)sr.Read() }), 16);
            return bytes;
        }

        /// <summary>
        /// Takes a int and checks if the bit at the given position is set
        /// </summary>
        /// <param name="byte"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool IsBitSet(int @byte, int position)
        {
            return (@byte & (1 << position)) != 0;
        }
    }
}

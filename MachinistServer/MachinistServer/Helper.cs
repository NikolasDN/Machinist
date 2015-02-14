using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace MachinistServer
{
    public static class Helper
    {

        public static List<T> EnumToList<T>()
        {
            Type enumType = typeof(T);

            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            Array enumValArray = Enum.GetValues(enumType);

            List<T> enumValList = new List<T>(enumValArray.Length);

            foreach (int val in enumValArray)
            {
                enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
            }

            return enumValList;
        }

        public static byte StringToByte(string s)
        {
            int i = Convert.ToInt32(s);
            char c = (char)i;
            byte b = (byte)c;
            return b;
        }

        public static byte[] BmpToBytes(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            // Save to memory using the Jpeg format
            bmp.Save(ms, ImageFormat.Jpeg);

            // read to end
            byte[] bmpBytes = ms.GetBuffer();
            bmp.Dispose();
            ms.Close();

            return bmpBytes;
        }

        public static Bitmap BytesToBmp(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            Bitmap result = new Bitmap(ms);
            ms.Close();
            return result;
        }

        public static string Int32ToHexString(Int32 i)
        {
            byte[] int32Bytes;
            int32Bytes = BitConverter.GetBytes(i);
            return PadString(int32Bytes[0].ToString("X"));
            //return String.Format("{0}{1}{2}{3}",
            //    PadString(int32Bytes[0].ToString("X")),
            //    PadString(int32Bytes[1].ToString("X")),
            //    PadString(int32Bytes[2].ToString("X")),
            //    PadString(int32Bytes[3].ToString("X")));
        }

        public static string Int32ToVisualByte(Int32 t)
        {
            byte val = BitConverter.GetBytes(t)[0];
            string result = "";

            for (t = 128; t > 0; t = t / 2)
            {
                if (((val + 1) & t) != 0) result += "1 ";
                if (((val + 1) & t) == 0) result += "0 ";
            }

            return result;
        }

        private static string PadString(string s)
        {
            while (s.Length < 2) s = "0" + s;
            while (s.Length < 3) s = " " + s;
            return s;
        }
    }
}

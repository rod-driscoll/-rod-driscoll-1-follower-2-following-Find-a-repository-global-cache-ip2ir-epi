using System;
using System.Text;
using System.Text.RegularExpressions;

namespace global_cache_ip2ir_epi
{
    public static class StringExtensions
    {
        public static byte[] GetBytes(string str) // because no other encodings work the way we need
        {
            if (str == null)
                return new byte[0];
            else
                return Encoding.GetEncoding("ISO-8859-1").GetBytes(str);
        }
        public static string HexPrintable(byte[] bArgs)
        {
            string strOut_ = "";
            foreach (byte bIndex_ in bArgs)
                strOut_ += String.Format("\\x{0:X2}", bIndex_);
            return strOut_;
        }
        public static string GetString(byte[] bytes)
        {
            if (bytes == null)
                return String.Empty;
            else
                return Encoding.GetEncoding("ISO-8859-1").GetString(bytes, 0, bytes.Length);
        }
        public static string Printable(byte[] bArgs, bool debugAsHex)
        {
            if (debugAsHex)
                return HexPrintable(bArgs);
            else
            {
                string strOut_ = "";
                foreach (byte bIndex_ in bArgs)
                {
                    string sOutput_ = String.Empty;
                    if (bIndex_ < 0x20 || bIndex_ > 0x7F)
                        strOut_ += String.Format("\\x{0:X2}", bIndex_);
                    else
                        strOut_ += GetString(new byte[] { bIndex_ });
                }
                return strOut_;
            }
        }
        public static int Atoi(string strArg) // "hello 123 there" returns 123, because ToInt throws exceptions when non numbers are inserted
        {
            if (strArg == null)
                return 0;
            else
            {
                string m = Regex.Match(strArg, @"[-]*\d+").Value;
                return (m.Length == 0 ? 0 : Convert.ToInt32(m));
            }
        }

        public static string Remove(this string str, int startIndex)
        {
            return str.Remove(startIndex, str.Length - startIndex);
        }
        public static string CapitaliseFirstDigit(this string s)
        {
            return s.Remove(1).ToUpper() + s.Substring(1).ToLower();
        }
        public static string BytesFromHex(this string str)
        {
            if (str == null)
                return String.Empty;
            string p1 = @"(\\[xX][0-9a-fA-F]{2}|.)";
            var r1 = new Regex(p1);
            MatchCollection m = r1.Matches(str);
            string s1 = "";
            foreach (Match m1 in m)
            {
                string s2 = m1.Value;
                if (m1.Value.IndexOf("\\x") > -1)
                {
                    string s3 = m1.Value.Remove(0, 2);
                    byte b2 = Byte.Parse(s3, System.Globalization.NumberStyles.HexNumber);
                    s1 = s1 + GetString(new byte[] { b2 });
                }
                else
                {
                    byte[] b1 = GetBytes(m1.Value);
                    s1 = s1 + GetString(b1);
                }
            }
            byte[] b = GetBytes(s1);
            return s1;
        }
        public static string HexPrintable(this string str)
        {
            byte[] b = GetBytes(str);
            return HexPrintable(b);
        }
        public static string Printable(this string str, bool debugAsHex)
        {
            if (debugAsHex)
                return str.HexPrintable();
            else
            {
                byte[] b = GetBytes(str); // UTF8 creates 2 bytes when over \x80
                return Printable(b, debugAsHex);
            }
        }
    }
}

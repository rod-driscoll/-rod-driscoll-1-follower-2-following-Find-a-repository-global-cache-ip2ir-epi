using Crestron.SimplSharp.CrestronIO;
using PepperDash.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace global_cache_ip2ir_epi
{
    public static class ip2irOperations
    {
        public static Dictionary<string, string> ReadIrFile(string path)
        {
            Debug.Console(1, "ip2ir ReadIrFile, searching for: {0}", path);
            var data = new Dictionary<string, string>();
            try
            {
                if (File.Exists(path))
                {
                    Debug.Console(1, "ip2ir Reading IR file: {0}", path);
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        StreamReader sr = new StreamReader(fs);
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        { // [POWER]	0000 0073 0000 0036 0010 000A 0005 000A 0005 0016 0005 000A 0005 0010 0005 0016 0005 0016 0005 000A 0005 000A 0005 000A 0005 0016 0005 0010 0005 0016 0005 000A 0005 000A 0005 001D 0005 000A 0005 0C9E 0010 000A 0005 000A 0005 0016 0005 000A 0005 0010 0005 0016 0005 0016 0005 000A 0005 000A 0005 000A 0005 0016 0005 0010 0005 0016 0005 000A 0005 000A 0005 001D 0005 000A 0005 04E1 0010 000A 0005 000A 0005 0016 0005 000A 0005 0010 0005 0016 0005 0016 0005 000A 0005 000A 0005 0016 0005 0016 0005 0010 0005 0016 0005 000A 0005 000A 0005 001D 0005 000A 0005 04E1
                            Match m = Regex.Match(line, @"^\[(.*)\]\s*(.*)$"); //[NAME] 0000 0000"
                            if (m.Success)
                                data.Add(m.Groups[1].Value.ToUpper(), m.Groups[2].Value);
                        }
                    }
                }
                else
                {
                    Debug.Console(1, "ip2ir No files found at {0}", path);
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, "ip2ir RecallConfig ERROR: {0}", e);
            }
            return data;
        }
        public static string GetGlobalCaheIrString(int port, string str, int repeat)
        {
            // str = 0000 0073 0000 0036 0010 000A 0005 000A 0005 0016 0005 000A 0005 0010 0005 0016 0005 0016 0005 000A 0005 000A 0005 000A 0005 0016 0005 0010 0005 0016 0005 000A 0005 000A 0005 001D 0005 000A 0005 0C9E 0010 000A 0005 000A 0005 0016 0005 000A 0005 0010 0005 0016 0005 0016 0005 000A 0005 000A 0005 000A 0005 0016 0005 0010 0005 0016 0005 000A 0005 000A 0005 001D 0005 000A 0005 04E1 0010 000A 0005 000A 0005 0016 0005 000A 0005 0010 0005 0016 0005 0016 0005 000A 0005 000A 0005 0016 0005 0016 0005 0010 0005 0016 0005 000A 0005 000A 0005 001D 0005 000A 0005 04E1
            // return = sendir,1:1,1,38000,1,1,343,169,22,63,22,19,22,19,22,19,22,19,22,19,22,19,22,63,22,19,22,19,22,63,22,63,22,19,22,63,22,19,22,19,22,19,22,63,22,19,22,19,22,63,22,19,22,19,22,19,22,63,22,19,22,63,22,63,22,19,22,63,22,63,22,63,22,3800
            StringBuilder sb = new StringBuilder();
            MatchCollection m1 = Regex.Matches(str, @"(\S+)");
            for (byte b = 4; b < m1.Count; b++)
            {
                UInt16 i = System.UInt16.Parse(m1[b].Value, System.Globalization.NumberStyles.HexNumber);
                sb.Append(String.Format(",{0}", i));
            }
            UInt16 freq = System.UInt16.Parse(m1[1].Value, System.Globalization.NumberStyles.HexNumber);
            freq = (UInt16)(1000000 / (freq * 0.241246));
            freq = (UInt16)(Math.Round((decimal)freq / 1000, 0) * 1000); // round it
            UInt16 preamble = System.UInt16.Parse(m1[2].Value, System.Globalization.NumberStyles.HexNumber);
            preamble = (UInt16)(preamble * 2 + 1);
            Debug.Console(1, "ip2ir sendir,1:{0},{1},{2},{3},1", port, repeat, freq, preamble);
            return String.Format(  "sendir,1:{0},{1},{2},{3},1{4}\x0D", port, repeat, freq, preamble, sb);
        }
        public static string GetDataFromDict(Dictionary<string, string> data, string functionName) // Todo: make this a lot better
        {
            string key = GetValidKeyFromDict(data, functionName);
            if (!String.IsNullOrEmpty(key))
                return data[key];
            return String.Empty;
        }
        public static string GetValidKeyFromDict(Dictionary<string, string> data, string functionName) // Todo: make this a lot better
        {
            try
            {
                if (data.ContainsKey(functionName))
                    return functionName;
                string key = functionName.ToUpper();
                if (data.ContainsKey(key))
                    return key;
                if (key == "ENTER" || key == "OK" || key == "SELECT")
                {
                    List<string> keys_ = new List<string>{ "OK", "SELECT", "ENTER", "SEL" };
                    string result_ = keys_.Find(x => data.ContainsKey(x));
                    if (!String.IsNullOrEmpty(result_))
                        return result_;
                }
                if (key.Contains("VOL"))
                {
                    if (key.Contains("+") || key.Contains("UP"))
                    {
                        List<string> keys_ = new List<string> {
                            "VOL+", "VOL +", 
                            "VOL UP", "VOL_UP", 
                            "VOLUME+", "VOLUME +",
                            "VOLUME UP", "VOLUME_UP" };
                        string result_ = keys_.Find(x => data.ContainsKey(x));
                        if (!String.IsNullOrEmpty(result_))
                            return result_;
                    }
                    if (key.Contains("-") || key.Contains("DOWN") || key.Contains("DN"))
                    {
                        List<string> keys_ = new List<string> {
                            "VOL-", "VOL -",
                            "VOL DN", "VOL_DN",
                            "VOL DOWN", "VOL_DOWN",
                            "VOLUME-", "VOLUME -",
                            "VOLUME DN", "VOLUME_DN",
                            "VOLUME DOWN", "VOLUME_DOWN" };
                        string result_ = keys_.Find(x => data.ContainsKey(x));
                        if (!String.IsNullOrEmpty(result_))
                            return result_;
                    }
                }
                if (key.Contains("CHAN")) // "CH" not tested
                {
                    if (key.Contains("+") || key.Contains("UP"))
                    {
                        List<string> keys_ = new List<string> {
                            "CH+", "CH +",
                            "CH UP", "CH_UP",
                            "CHAN+", "CHAN +",
                            "CHAN UP", "CHAN_UP",
                            "CHANNEL+", "CHANNEL +",
                            "CHANNEL UP", "CHANNEL_UP" };
                         string result_ = keys_.Find(x => data.ContainsKey(x));
                        if (!String.IsNullOrEmpty(result_))
                            return result_;
                    }
                    if (key.Contains("-") || key.Contains("DOWN") || key.Contains("DN"))
                    {
                        List<string> keys_ = new List<string> {
                            "CH-", "CH -",
                            "CH DN", "CH_DN",
                            "CH DOWN", "CH_DOWN",
                            "CHAN-", "CHAN -",
                            "CHAN DN", "CHAN_DN",
                            "CHAN DOWN", "CHAN_DOWN",
                            "CHANNEL-", "CHANNEL -",
                            "CHANNEL DN", "CHANNEL_DN",
                            "CHANNEL DOWN", "CHANNEL_DOWN" };
                        string result_ = keys_.Find(x => data.ContainsKey(x));
                        if (!String.IsNullOrEmpty(result_))
                            return result_;
                    }
                }
                return String.Empty;
            }
            catch (Exception e)
            {
                Debug.Console(1, "ip2ir Error getting IR command [{0}], for {1} \n{2}", functionName, e.Message);
                return String.Empty;
            }
        }
        public static string CreatePrintableString(string str, bool debugAsHex)
        {
            if (debugAsHex)
                return str.HexPrintable();
            else
            {
                byte[] b = StringExtensions.GetBytes(str); // UTF8 creates 2 bytes when over \x80
                return StringExtensions.Printable(b, debugAsHex);
            }
        }
    }
}

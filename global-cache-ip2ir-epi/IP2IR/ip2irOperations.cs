using Crestron.SimplSharp.CrestronIO;
using Independentsoft.Exchange;
using PepperDash.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace global_cache_ip2ir_epi
{
    public static class IP2IROperations
    {
        public static Dictionary<string, IP2IRCommand> ReadIrFile(string path)
        {
            Debug.Console(1, "ip2ir ReadIrFile, searching for: {0}", path);
            var data = new Dictionary<string, IP2IRCommand>();
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
                            {
                                var cmd_ = new IP2IRCommand { Name = m.Groups[1].Value.ToUpper(), CCFString = m.Groups[2].Value };
                                data.Add(cmd_.Name, cmd_);
                            }
                            else
                            { // "AUDIO","sendir,1:1,1,38000,1,1,342,171,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,64,21,64,21,64,21,64,21,64,21,64,21,64,21,64,21,64,21,21,21,64,21,64,21,64,21,21,21,64,21,21,21,21,21,64,21,21,21,21,21,21,21,64,21,21,21,64,21,1534,342,85,21,1241","0000 006D 0000 0024 0156 00AB 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0015 0015 0040 0015 0040 0015 0040 0015 0015 0015 0040 0015 0015 0015 0015 0015 0040 0015 0015 0015 0015 0015 0015 0015 0040 0015 0015 0015 0040 0015 05FE 0156 0055 0015 04D9",,
                                string pattern = "\"([^\"]*)\"";
                                MatchCollection matches = Regex.Matches(line, pattern);
                                if (matches.Count > 0)
                                {
                                    var cmd_ = new IP2IRCommand { 
                                        Name = matches[0].Groups[1].Value.ToUpper(),
                                        IP2IRString = matches[1].Groups[1].Value,
                                        CCFString = matches[2].Groups[1].Value,
                                    };
                                    data.Add(cmd_.Name, cmd_);
                                }
                            }
                        }
                    }
                    Debug.Console(1, "ip2ir IR file contains {0} entries", data.Count);
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
        public static string GetIP2IRString(IP2IRCommand cmd, int port, int repeat)
        {
            if (!String.IsNullOrEmpty(cmd.IP2IRString))
            {
                //Debug.Console(2, "ip2ir GetIP2IRString({0}:{1})", port, cmd.IP2IRString);
                if(repeat == 0)
                    return cmd.IP2IRString.Replace("sendir,1:1,", String.Format("sendir,1:{0},", port));
                else
                    return cmd.IP2IRString.Replace("sendir,1:1,1", String.Format("sendir,1:{0},{1}", port, repeat));
            }
            if (!String.IsNullOrEmpty(cmd.CCFString))
                return ConvertCCFToIP2IR(port, cmd.CCFString, repeat);

            return String.Empty;
        }
        public static string ConvertCCFToIP2IR(int port, string str, int repeat)
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
            //Debug.Console(2, "ip2ir sendir,1:{0},{1},{2},{3},1", port, repeat, freq, preamble);
            return String.Format("sendir,1:{0},{1},{2},{3},1{4}\x0D", port, repeat, freq, preamble, sb);
        }
        public static IP2IRCommand GetDataFromDict(Dictionary<string, IP2IRCommand> data, string functionName) // Todo: make this a lot better
        {
            string key = GetValidKeyFromDict(data, functionName);
            if (!String.IsNullOrEmpty(key))
                return data[key];
            return new IP2IRCommand { Name=String.Empty };
        }
        public static string GetValidKeyFromDict(Dictionary<string, IP2IRCommand> data, string functionName) // Todo: make this a lot better
        {
            try
            {
                //Debug.Console(2, "ip2ir GetValidKeyFromDict({0})", functionName);
                if (data.ContainsKey(functionName))
                {
                    Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", functionName);
                    return functionName;
                }
                string key = functionName.ToUpper();
                if (data.ContainsKey(key))
                {
                    Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", key);
                    return key;
                }
                if (key == "ENTER" || key == "OK" || key == "SELECT")
                {
                    List<string> keys_ = new List<string>{ "OK", "SELECT", "ENTER", "SEL" };
                    string result_ = keys_.Find(x => data.ContainsKey(x));
                    if (!String.IsNullOrEmpty(result_))
                    {
                        Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", result_);
                        return result_;
                    }
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
                        {
                            Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", result_);
                            return result_;
                        }
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
                        {
                            Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", result_);
                            return result_;
                        }
                    }
                }
                if (key.Contains("CHAN") || key.Equals("CH+") || key.Equals("CH-"))
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
                        {
                            Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", result_);
                            return result_;
                        }
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
                        {
                            Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", result_);
                            return result_;
                        }
                    }
                }
                if (key.Contains("ARROW") || key.Equals("CURSOR") || key.Equals("KEY"))
                {
                    if (key.Contains("UP"))
                    {
                        List<string> keys_ = new List<string> {
                            "ARROW UP", "ARROW_UP",
                            "CURSOR UP", "CURSOR_UP",
                            "KEY UP", "KEY_UP" };
                        string result_ = keys_.Find(x => data.ContainsKey(x));
                        if (!String.IsNullOrEmpty(result_))
                        {
                            Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", result_);
                            return result_;
                        }
                    }
                    if (key.Contains("DOWN") || key.Contains("DN"))
                    {
                        List<string> keys_ = new List<string> {
                            "ARROW DN", "ARROW_DN",
                            "ARROW DOWN", "ARROW_DOWN",
                            "CURSOR DN", "CURSOR_DN",
                            "CURSOR DOWN", "CURSOR_DOWN",
                            "KEY DN", "KEY_DN",
                            "KEY DOWN", "KEY_DOWN" };
                        string result_ = keys_.Find(x => data.ContainsKey(x));
                        if (!String.IsNullOrEmpty(result_))
                        {
                            Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", result_);
                            return result_;
                        }
                    }
                    if (key.Contains("LE"))
                    {
                        List<string> keys_ = new List<string> {
                            "ARROW LE", "ARROW_LE",
                            "ARROW LEFT", "ARROW_LEFT",
                            "CURSOR LE", "CURSOR_LE",
                            "CURSOR LEFT", "CURSOR_LEFT",
                            "KEY LE", "KEY_LE",
                            "KEY LEFT", "KEY_LEFT" };
                        string result_ = keys_.Find(x => data.ContainsKey(x));
                        if (!String.IsNullOrEmpty(result_))
                        {
                            Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", result_);
                            return result_;
                        }
                    }
                    if (key.Contains("RI"))
                    {
                        List<string> keys_ = new List<string> {
                            "ARROW RI", "ARROW_RI",
                            "ARROW RIGHT", "ARROW_RIGHT",
                            "CURSOR RI", "CURSOR_RI",
                            "CURSOR RIGHT", "CURSOR_RIGHT",
                            "KEY RI", "KEY_RI",
                            "KEY RIGHT", "KEY_RIGHT" };
                        string result_ = keys_.Find(x => data.ContainsKey(x));
                        if (!String.IsNullOrEmpty(result_))
                        {
                            Debug.Console(2, "ip2ir GetValidKeyFromDict return: {0}", result_);
                            return result_;
                        }
                    }
                }
                //Debug.Console(1, "ip2ir GetValidKeyFromDict searching for '{0}'", key);
                var m = Regex.Match(key, @"\d"); // "1" in "channel 1"
                if (m.Success && m.Value.Length == 1) // source contains a single digit number
                {
                    var val_ = m.Value;
                    //Debug.Console(1, "ip2ir GetValidKeyFromDict found '{0}' in '{1}'", val_, key);
                    foreach (var item in data)
                    {
                        if (item.Key.Contains(val_))
                        {
                            //Debug.Console(1, "ip2ir GetValidKeyFromDict match: {0}", item.Key);
                            var m1 = Regex.Match(item.Key, @"\d");
                            if (m1.Success && m1.Value.Length == 1) // line contains a single digit number
                            {
                                //Debug.Console(1, "ip2ir GetValidKeyFromDict return: {0}", item.Key);
                                return item.Key;
                            }
                        }
                    }
                }
                Debug.Console(1, "ip2ir GetValidKeyFromDict, no valid key found");
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

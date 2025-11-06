using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml;
using HtmlAgilityPack;

namespace OCS_Parser_Minoss
{
    class Parser_xshell_lib
    {
        private string comm_parse_hp_status_value(String line)
        {
            int start_idx, end_idx;
            start_idx = line.IndexOf("\"") + 1;
            end_idx = (line.LastIndexOf("\"") - start_idx) - 1;
            return line.Substring(start_idx, end_idx);
        }

        public int comm_parse_key_value(String line)
        {
            string value = String.Empty;
            string[] itemArr;
            itemArr = line.Split('=');
            if (itemArr.Length > 1)
                value = itemArr[1];
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// This function is common function, it will provide parse 'syscheck' script result, it include cpu usage and memory usage
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public float comm_parse_syschk_cmd(String line)
        {
            float value = 0;
            string[] arr1 = line.Split(' ');
            string[] arr2 = arr1[2].Split('%');
            //Debug.WriteLine(arr2[0]);
            if (arr2.Length > 1)
                float.TryParse(arr2[0], out value);
            //Debug.WriteLine("line: " + line);
            if (line.IndexOf("Mem", StringComparison.OrdinalIgnoreCase) > 0)
                return (100 - value);
            else
                return Convert.ToSingle(Math.Round(value,0));
        }
        
        /// <summary>
        /// This function is common function, it will provide parse 'df' command
        /// </summary>
        /// <param name="line"></param>
        /// <param name="os_disk"></param>
        /// <returns></returns>
        public int comm_parse_df_cmd(StreamReader str)
        {
            String line;
            int max = 0;
            int value = 0;

            line = str.ReadLine();
            while (line.IndexOf("-----------------------------", StringComparison.OrdinalIgnoreCase) < 0)
            {
                string[] arr = line.Split(' ');
                foreach (var ar1 in arr)
                {
                    string[] itemArr = ar1.Split('%');
                    if (itemArr.Length > 1 & int.TryParse(itemArr[0], out value))
                        if (value > max)
                            max = value;
                }
                //Debug.WriteLine(line);
                line = str.ReadLine();
            }
            return max;
        }

        /// <summary>
        /// This function is common function, it will provide parse system log
        /// </summary>
        /// <param name="str"></param>
        /// <param name="syslog_msg"></param>
        /// <returns></returns>
        public bool comm_parse_syslog_cmd(StreamReader str, List<string> syslog_msg)
        {
            String line;
            bool err = false;
            line = str.ReadLine();
            //Debug.WriteLine("$1: " + line);
            if (line.IndexOf("yesterday", StringComparison.OrdinalIgnoreCase) > 0)
                line = str.ReadLine();
            while (line.IndexOf("-----------------------------", StringComparison.OrdinalIgnoreCase) < 0)
            {
                //Debug.WriteLine("$2: " + line);
                if( !err ) err = true;
                syslog_msg.Add(line);
                line = str.ReadLine();
            }
            if (err)
                return false;
            else
                return true;
        }

        /// <summary>
        /// This function is common function, it will provide parse 'df' command
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool comm_parse_bond_status(StreamReader str)
        {
            String line;
            line = str.ReadLine();
            while (line.IndexOf("-----------------------------", StringComparison.OrdinalIgnoreCase) < 0)
            {
                if(line.IndexOf("down", StringComparison.OrdinalIgnoreCase) > 0)
                    return false;
                line = str.ReadLine();
            }
            return true;
        }

        /// <summary>
        /// This function is common function, it will provide parse "mii-tool" command
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool comm_parse_mii_status(StreamReader str)
        {
            String line;
            line = str.ReadLine();
            while (line.IndexOf("-----------------------------", StringComparison.OrdinalIgnoreCase) < 0)
            {
                if (line.IndexOf("link ok", StringComparison.OrdinalIgnoreCase) > 0)
                    return true;
                line = str.ReadLine();
            }
            return false;
        }

        /// <summary>
        /// This function is special function, it will provide parse TSP 'CIPIPLinkStatus' node
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool comm_parse_DIA_status(StreamReader str)
        {
            String line;
            line = str.ReadLine();
            while (line.IndexOf("--", StringComparison.OrdinalIgnoreCase) < 0)
            {
                //Debug.WriteLine(line);
                if (line.IndexOf("Up", StringComparison.OrdinalIgnoreCase) > 0)
                    return true;
                line = str.ReadLine();
            }
            return false;
        }

        /// <summary>
        /// This function is special function, it will provide parse TSP 'signmcli' command
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool comm_parse_SIGTRAN_status(String line)
        {
            //Debug.WriteLine(line);
            if (line.IndexOf("Active", StringComparison.OrdinalIgnoreCase) > 0)
                return true;
            return false;
        }

        /// <summary>
        /// This function is common function, it will provide parse 'ntptime' command
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool comm_parse_ntp_status(StreamReader str)
        {
            String line;
            line = str.ReadLine();
            while (line.IndexOf("-----------------------------", StringComparison.OrdinalIgnoreCase) < 0)
            {
                if (line.IndexOf("ntp_gettime", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    //Debug.WriteLine(line);
                    if (line.IndexOf("OK", StringComparison.OrdinalIgnoreCase) < 0)
                        return false;
                }
                if (line.IndexOf("ntp_adjtime", StringComparison.OrdinalIgnoreCase) > 0)
                {
                   // Debug.WriteLine(line);
                    if (line.IndexOf("OK", StringComparison.OrdinalIgnoreCase) < 0)
                        return false;
                }
                line = str.ReadLine();
            }
            return true;
        }

        /// <summary>
        /// Below function will parse all devices of OCS system
        /// </summary>
        /// <param name="log"></param>
        /// <param name="data"></param>
        public void parser_7L2AIR(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond0_stat = true;
            bool bond3_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/bond0", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond0_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond3", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond3_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("bond0 status: " + bond0_stat);
            Debug.WriteLine("bond3 status: " + bond3_stat);
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond0_stat & bond3_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2SDP(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond0_stat = true;
            bool bond3_stat = true;
            bool eth2_stat = true;
            bool eth6_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/bond0", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond0_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond3", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond3_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("sudo mii-tool eth2", StringComparison.OrdinalIgnoreCase) >= 0)
                        eth2_stat = comm_parse_mii_status(str);

                    if (line.IndexOf("sudo mii-tool eth6", StringComparison.OrdinalIgnoreCase) >= 0)
                        eth6_stat = comm_parse_mii_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("bond0 status: " + bond0_stat);
            Debug.WriteLine("bond3 status: " + bond3_stat);
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond0_stat & bond3_stat & eth2_stat & eth6_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2CCN(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 28;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool SIGTRAN_SRR23_stat = true;
            bool SIGTRAN_SRR31_stat = true;
            bool SIGTRAN_SRR61_stat = true;
            bool SIGTRAN_SRR71_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("getSyslog.bash", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    if (line.IndexOf("M3IETF-Local AS Status of [SRR23 LocalAS", StringComparison.OrdinalIgnoreCase) >= 0)
                        SIGTRAN_SRR23_stat = comm_parse_SIGTRAN_status(line);

                    if (line.IndexOf("M3IETF-Local AS Status of [SRR31 LocalAS", StringComparison.OrdinalIgnoreCase) >= 0)
                        SIGTRAN_SRR31_stat = comm_parse_SIGTRAN_status(line);

                    if (line.IndexOf("M3IETF-Local AS Status of [SRR61 LocalAS", StringComparison.OrdinalIgnoreCase) >= 0)
                        SIGTRAN_SRR61_stat = comm_parse_SIGTRAN_status(line);

                    if (line.IndexOf("M3IETF-Local AS Status of [SRR71 LocalAS", StringComparison.OrdinalIgnoreCase) >= 0)
                        SIGTRAN_SRR71_stat = comm_parse_SIGTRAN_status(line);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("SIGTRAN_SRR23_stat: " + SIGTRAN_SRR23_stat);
            Debug.WriteLine("SIGTRAN_SRR31_stat: " + SIGTRAN_SRR31_stat);
            Debug.WriteLine("SIGTRAN_SRR61_stat: " + SIGTRAN_SRR61_stat);
            Debug.WriteLine("SIGTRAN_SRR71_stat: " + SIGTRAN_SRR71_stat);
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = SIGTRAN_SRR23_stat & SIGTRAN_SRR31_stat & SIGTRAN_SRR61_stat & SIGTRAN_SRR71_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2TMSAP(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bondcdr_stat = true;
            bool bondom_stat = true;
            bool bondha_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/bond_cdr", StringComparison.OrdinalIgnoreCase) >= 0)
                        bondcdr_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_om", StringComparison.OrdinalIgnoreCase) >= 0)
                        bondom_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_ha", StringComparison.OrdinalIgnoreCase) >= 0)
                        bondha_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bondcdr_stat & bondom_stat & bondha_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2TMSDB(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bondom_stat = true;
            bool bondha_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/bond_om", StringComparison.OrdinalIgnoreCase) >= 0)
                        bondom_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_ha", StringComparison.OrdinalIgnoreCase) >= 0)
                        bondha_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bondom_stat & bondha_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2TMSREP(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool ombond3_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/ombond3", StringComparison.OrdinalIgnoreCase) >= 0)
                        ombond3_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = ombond3_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2NDDP(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond_ch_stat = true;
            bool bond_occsig_stat = true;
            bool bond_om_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/bond_ch", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_ch_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_occsig", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_occsig_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_om", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_om_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond_ch_stat & bond_occsig_stat & bond_om_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2IVRC(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond0_stat = true;
            bool bond1_stat = true;
            bool bond3_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/bond0", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond0_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond1", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond1_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond3", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond3_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("bond0 status: " + bond0_stat);
            Debug.WriteLine("bond3 status: " + bond3_stat);
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond0_stat & bond1_stat & bond3_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2USSDGW(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond0_stat = true;
            bool bond1_stat = true;
            bool bond3_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/bond0", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond0_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond1", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond1_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond3", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond3_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("bond0 status: " + bond0_stat);
            Debug.WriteLine("bond3 status: " + bond3_stat);
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond0_stat & bond1_stat & bond3_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2OCSGAP(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond_ch_stat = true;
            bool bond_om_stat = true;
            bool bond_ha_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/bond_ch", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_ch_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_om", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_om_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_ha", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_ha_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond_ch_stat & bond_om_stat & bond_ha_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2OCSGDB(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond_ch_stat = true;
            bool bond_om_stat = true;
            bool bond_ha_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/bond_ch", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_ch_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_om", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_om_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_ha", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_ha_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("bond_ch status: " + bond_ch_stat);
            Debug.WriteLine("bond_om status: " + bond_om_stat);
            Debug.WriteLine("bond_ha status: " + bond_ha_stat);
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond_ch_stat & bond_om_stat & bond_ha_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2PCRFDBSYNAP(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond_om_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_om", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_om_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("bond_om status: " + bond_om_stat);
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond_om_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2PCRFDBSYNDB(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond_om_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_om", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_om_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("bond_om status: " + bond_om_stat);
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond_om_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2PCRF(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 28;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("getSyslog.bash", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = true;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2OAM(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond_om_stat = true;
            bool bond_ha_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse HP ILOM "show /system1/sensor1" command result
                    if (line.IndexOf("CurrentReading=", StringComparison.OrdinalIgnoreCase) >= 0)
                        temp = comm_parse_key_value(line);

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("syscheck_linux.sh daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("$ sudo cat /var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse interface status
                    if (line.IndexOf("$ cat /proc/net/bonding/bond_om", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_om_stat = comm_parse_bond_status(str);

                    if (line.IndexOf("$ cat /proc/net/bonding/bond_ha", StringComparison.OrdinalIgnoreCase) >= 0)
                        bond_ha_stat = comm_parse_bond_status(str);

                    /// Parse ntp status
                    if (line.IndexOf("$ ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("bond_om status: " + bond_om_stat);
            Debug.WriteLine("bond_ha status: " + bond_ha_stat);
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond_om_stat & bond_ha_stat;
            result.ntp_stat = ntp_stat;
        }
        public void parser_7L2NEWOCSG(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            int temp = 0;
            float cpu = 0;
            float mem = 0;
            int disk = 0;
            bool log_stat = false;
            bool bond_ch_stat = true;
            bool bond_om_stat = true;
            bool bond_ha_stat = true;
            bool ntp_stat = true;
            List<string> syslog_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// temperature - parse Dell "/opt/dell/srvadmin/sbin/racadm getsensorinfo" command result
                    if (line.IndexOf("chtop-Temp=", StringComparison.OrdinalIgnoreCase) >= 0)
                    { temp = comm_parse_key_value(line); }

                    /// Parse CPU usage
                    if (line.IndexOf("CPU Busy: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        cpu = comm_parse_syschk_cmd(line);

                    /// Parse memory usage
                    if (line.IndexOf("Free Mem: ", StringComparison.OrdinalIgnoreCase) >= 0)
                        mem = comm_parse_syschk_cmd(line);

                    /// Parse disk usage
                    if (line.IndexOf("daily", StringComparison.OrdinalIgnoreCase) >= 0)
                        disk = comm_parse_df_cmd(str);

                    /// Parse syslog yesterday
                    if (line.IndexOf("/var/log/message", StringComparison.OrdinalIgnoreCase) >= 0)
                        log_stat = comm_parse_syslog_cmd(str, syslog_msg);

                    /// Parse ntp status
                    if (line.IndexOf("ntptime", StringComparison.OrdinalIgnoreCase) >= 0)
                        ntp_stat = comm_parse_ntp_status(str);
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("temperature: " + temp);
            Debug.WriteLine("OS CPU: " + cpu);
            Debug.WriteLine("OS Memory: " + mem);
            Debug.WriteLine("OS Disk: " + disk);
            Debug.WriteLine("OS syslog status: " + log_stat);
            if (log_stat)
                syslog_msg.ForEach(i => Debug.WriteLine("{0}\t", i));
            Debug.WriteLine("NTP status: " + ntp_stat);

            /* Save all parse result to structure*/
            result.temp = temp;
            result.cpu = cpu;
            result.mem = mem;
            result.disk = disk;
            result.log_stat = log_stat;
            result.syslog_msg = syslog_msg;
            result.intf_stat = bond_ch_stat & bond_om_stat & bond_ha_stat;
            result.ntp_stat = ntp_stat;
        }

        /// <summary>
        /// 檢查OAM備份各設備的資料(OS,DB,CM,AP,PM)是否有遺失或錯誤
        /// 請參考MIOSS腳本 "backup_verify.xsh"
        /// </summary>
        /// <param name="log">backup_verify.log</param>
        /// <param name="result"></param>
        public void parser_backup_verify(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            bool log_stat = true;
            string strBreakLine = "==================================================================";
            string strPointName = string.Empty; //定位點，表示目前掃描到哪一台設備
            List<string> log_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// Parse syslog yesterday
                    if (line.IndexOf("BACKUP ERROR", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        while (line.IndexOf(strBreakLine, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            if (log_stat) log_stat = false;
                            log_msg.Add(line);
                            line = str.ReadLine();
                        }
                    }
                    if (line.IndexOf("[7L2", StringComparison.OrdinalIgnoreCase) >= 0)
                        strPointName = string.Copy(line);

                    if (line.IndexOf("missing", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (log_stat) log_stat = false;
                        log_msg.Add(strPointName + " backup " + line);
                    }
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("backup status: " + log_stat);
            if (log_msg != null)
                log_msg.ForEach(i => Debug.WriteLine("{0}\t", i));

            /* Save all parse result to structure*/
            result.temp = 0;
            result.cpu = 0;
            result.mem = 0;
            result.disk = 0;
            result.log_stat = log_stat;
            result.syslog_msg = log_msg;
            result.intf_stat = false;
            result.ntp_stat = false;
        }

        /// <summary>
        /// 檢查TMS設定產出的CDR log是否有Error或者Exception
        /// </summary>
        /// <param name="log">cdr_check.log</param>
        /// <param name="result"></param>
        public void parser_cdr_check(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            bool log_stat = true;
            List<string> log_msg = new List<string>();
            StringComparison ordCmp = StringComparison.OrdinalIgnoreCase;

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                line = str.ReadLine();
                while ( line.IndexOf("-----------------------------", StringComparison.OrdinalIgnoreCase) < 0 )
                {
                    line = str.ReadLine();
                    if (line.IndexOf("check_cdr_exception.sh", ordCmp) > 0)
                        continue;

                    if (line.IndexOf("error", ordCmp) > 0 || line.IndexOf("WARN", ordCmp) > 0)
                    {
                        Debug.WriteLine(line);
                        if (log_stat) log_stat = false;
                        log_msg.Add(line);
                    }   
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("backup status: " + log_stat);
            if (log_msg != null)
                log_msg.ForEach(i => Debug.WriteLine("{0}\t", i));

            /* Save all parse result to structure*/
            result.temp = 0;
            result.cpu = 0;
            result.mem = 0;
            result.disk = 0;
            result.log_stat = log_stat;
            result.syslog_msg = log_msg;
            result.intf_stat = false;
            result.ntp_stat = false;
        }

        /// <summary>
        /// 檢查目前Centreon系統有哪些告警
        /// </summary>
        /// <param name="log">sys_check_7L2OAM1.log</param>
        /// <param name="result"></param>
        public void parser_alarms_stat(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            bool log_stat = false;
            string msg = null;
            List<string> log_msg = new List<string>();

            //Read raw file, and process parser content
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    /// Parse syslog yesterday
                    if (line.IndexOf("sudo /usr/local/nagios/bin/nagiostats", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        if(line.Length == 0)
                            line = str.ReadLine();
                        string[] arr = line.Split('/');
                        try
                        {
                            Debug.WriteLine("Warn: " + arr[4] + "Unk: " + arr[5] + "Crit: " + arr[6]);
                            msg = arr[6] + " 個Ciritical alarm ；" + arr[4] + "個waning alarm ；" + arr[5] + "個unknow alarm .";
                            log_msg.Add(msg);
                            log_stat = true;
                        }
                        catch { Debug.WriteLine("Parse Error"); }
                    }
                }
                str.Close();
            }

            /* Save all parse result to structure*/
            result.temp = 0;
            result.cpu = 0;
            result.mem = 0;
            result.disk = 0;
            result.log_stat = log_stat;
            result.syslog_msg = log_msg;
            result.intf_stat = false;
            result.ntp_stat = false;
        }

        /// <summary>
        /// 此功能將確認是否有人用非為運帳號切換成高權限帳號, 並進行設定
        /// </summary>
        /// <param name="log">1)sys_check_7L2OAM1.log 2)sys_check_7L2OAM2.log</param>
        /// <param name="result">Parse result</param>
        public void parser_login_log(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            bool log_stat = true; //True is root NOT login system
            List<string> log_msg = new List<string>();

            //Read raw file, and process parser content
            Debug.WriteLine("@log name: " + log);
            #region Parse login records yesterday
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;

                while ((line = str.ReadLine()) != null)
                {
                    if (line.IndexOf("uniq", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        bool err = false; // if err is true, Indicate root has login
                        line = str.ReadLine();

                        /// Avoid cmd line last space is empty
                        if(line.Length == 0)
                            line = str.ReadLine();
                        while (line.IndexOf("-----------------------------", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            Debug.WriteLine("line:" + line);
                            if (!err)
                            {
                                log_msg.Add(log.Replace("sys_check_","").Replace(".log","") + "有登入高權限者："); //印主機名稱的log_msg
                                err = true;
                            }
                            log_msg.Add(line+", ");
                            line = str.ReadLine();
                        }
                        if (err) log_stat = false;
                    }
                }
                str.Close();
            }
            #endregion

            /* Debug - Verify all data is correct */
            Debug.WriteLine("System root has login: " + !log_stat);
            if (!log_stat) log_msg.ForEach(i => Debug.WriteLine("{0}\t", i));

            /* Save all parse result to structure*/
            result.temp = 0;
            result.cpu = 0;
            result.mem = 0;
            result.disk = 0;
            result.log_stat = log_stat;
            result.syslog_msg = log_msg;
            result.intf_stat = false;
            result.ntp_stat = false;
        }

        /// <summary>
        /// 確認所有設備的應用程式紀錄, 並包含應用程式錯誤及登出入紀錄
        /// The Function can parse applogin_check log, and it will extract all super user login records per host.
        /// </summary>
        /// <param name="log">applogin_check.log</param>
        /// <param name="result"></param>
        public void parser_app_log(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            bool log_stat = true;
            List<string> log_msg = new List<string>();

            //Read raw file, and process parser content
            Debug.WriteLine("@log name: " + log);
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    Debug.WriteLine(line);
                    if (line.IndexOf("## check session", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string title_str = string.Copy(line).Replace("check session on ","");
                        int logging_line = 1;
                        line = str.ReadLine();
                        while (line.IndexOf("-----------------------------", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            if (log_stat) log_stat = false;
                            if (logging_line == 1)
                                log_msg.Add(title_str + " 主機特殊帳號登入紀錄\r\n");
                            if (line.IndexOf("/su/-/", StringComparison.OrdinalIgnoreCase) > 0)
                                line = line.Replace("/su/-/","有切換到");
                            if (line.IndexOf("/su/-", StringComparison.OrdinalIgnoreCase) > 0)
                                line = line.Replace("/su/-", "有切換到root");
                            log_msg.Add(line+"\r\n");
                            line = str.ReadLine();
                            logging_line++;
                        }
                    }
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("System powerfull account has login: " + !log_stat);
            if (!log_stat) log_msg.ForEach(i => Debug.WriteLine("{0}\t", i));

            /* Save all parse result to structure*/
            result.temp = 0;
            result.cpu = 0;
            result.mem = 0;
            result.disk = 0;
            result.log_stat = log_stat;
            result.syslog_msg = log_msg;
            result.intf_stat = false;
            result.ntp_stat = false;
        }

        /// <summary>
        /// 取得SDP授權數及實際供裝數
        /// The Function can parse 7L2SDP_License_view log, and it will extract three value,
        /// 1. SDP global license max number
        /// 2. Postpaid provision number
        /// 3. Prepaid provision number
        /// </summary>
        /// <param name="log">7L2SDP_License_view.log</param>
        /// <param name="result"></param>
        public void parser_SDPLicense_log(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            bool log_stat = false;
            uint iParseNum = 0; //string Covert integer variable
            uint iSDPIndex = 0; //0~2 equal SDP 1~3
            uint iPostPaidTotalNum = 0, iPrePaidTotalNum = 0;
            uint[] iSDPTotalNum = new uint[3];
            List<string> log_msg = new List<string>();

            //Read raw file, and process parser content
            Debug.WriteLine("@log name: " + log);
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    Debug.WriteLine(line);
                    if (line.IndexOf("SDPlicense", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        log_msg.Add("SDP License 數量為" + line + "000" + "\r\n");
                        line = str.ReadLine();
                        line = str.ReadLine();
                        //log_msg.Add("SDP License 已用數量為" + line + "000" + "\r\n");
                    }
                    // 取得7L2SDP1 postpaid的供裝數
                    if (line.IndexOf("echo \"7L2SDP1\"", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        if (uint.TryParse(line, out iParseNum) != false)
                        {
                            //log_msg.Add("7L2SDP1 postpaid 供裝數量為" + line + "\r\n");
                            iPostPaidTotalNum += uint.Parse(line);
                            iSDPTotalNum[iSDPIndex] += uint.Parse(line);
                        }                        
                    }
                    // 取得7L2SDP2 postpaid的供裝數
                    if (line.IndexOf("echo \"7L2SDP2\"", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        iSDPIndex = 1;
                        line = str.ReadLine();
                        if (uint.TryParse(line, out iParseNum) != false)
                        {
                            //log_msg.Add("7L2SDP2 postpaid 供裝數量為" + line + "\r\n");
                            iPostPaidTotalNum += uint.Parse(line);
                            iSDPTotalNum[iSDPIndex] += uint.Parse(line);
                        }

                    }

                    // 取得7L2SDP3 postpaid的供裝數
                    if (line.IndexOf("echo \"7L2SDP3\"", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        iSDPIndex = 2;
                        line = str.ReadLine();
                        if (uint.TryParse(line, out iParseNum) != false)
                        {
                            //log_msg.Add("7L2SDP3 postpaid 供裝數量為" + line + "\r\n");
                            iPostPaidTotalNum += uint.Parse(line);
                            iSDPTotalNum[iSDPIndex] += uint.Parse(line);
                        }

                    }

                    // 取得prepaid 2G 一般卡的供裝數
                    if (line.IndexOf("2G provision", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        if (uint.TryParse(line, out iParseNum) != false)
                        {
                            //log_msg.Add("7L2SDP Prepaid 2G 一般卡供裝數量為" + line + "\r\n");
                            iPrePaidTotalNum += uint.Parse(line);
                            iSDPTotalNum[iSDPIndex] += uint.Parse(line);
                        }
                    }
                    // 取得prepaid 3G 一般卡的供裝數
                    if (line.IndexOf("3G provision", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        if (uint.TryParse(line, out iParseNum) != false)
                        {
                            //log_msg.Add("7L2SDP Prepaid 3G 一般卡供裝數量為" + line + "\r\n");
                            iPrePaidTotalNum += uint.Parse(line);
                            iSDPTotalNum[iSDPIndex] += uint.Parse(line);
                        }
                    }
                    // 取得prepaid 3G 門號卡的供裝數
                    if (line.IndexOf("3G Fix provision", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        if (uint.TryParse(line, out iParseNum) != false)
                        {
                            //log_msg.Add("7L2SDP Prepaid 3G 門號卡供裝數量為" + line + "\r\n");
                            iPrePaidTotalNum += uint.Parse(line);
                            iSDPTotalNum[iSDPIndex] += uint.Parse(line);
                        }
                    }
                    // 取得prepaid 4G 一般卡的供裝數
                    if (line.IndexOf("4G provision", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        if (uint.TryParse(line, out iParseNum) != false)
                        {
                            //log_msg.Add("7L2SDP Prepaid 4G 一般卡供裝數量為" + line + "\r\n");
                            iPrePaidTotalNum += uint.Parse(line);
                            iSDPTotalNum[iSDPIndex] += uint.Parse(line);
                        }
                    }
                    // 取得prepaid 4G 門號卡的供裝數
                    if (line.IndexOf("4G Fix provision", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        if (uint.TryParse(line, out iParseNum) != false)
                        {
                            //log_msg.Add("7L2SDP Prepaid 4G 門號卡供裝數量為" + line + "\r\n");
                            iPrePaidTotalNum += uint.Parse(line);
                            iSDPTotalNum[iSDPIndex] += uint.Parse(line);
                        }
                    }
                }
                // 取得7L2SDP postpaid的總供裝數
                if (iPostPaidTotalNum != 0) log_msg.Add("高雄Postpaid總供裝數量為" + iPostPaidTotalNum + "\r\n");

                // 取得7L2SDP prepaid的總供裝數
                if (iPrePaidTotalNum != 0) log_msg.Add("高雄Prepaid總供裝數量為" + iPrePaidTotalNum + "\r\n");

                // 取得7L2SDP1的總供裝數目
                if (iSDPTotalNum[0] != 0) log_msg.Add("高雄SDP1總供裝數量為" + iSDPTotalNum[0] + "\r\n");
                // 取得7L2SDP2的總供裝數目
                if (iSDPTotalNum[1] != 0) log_msg.Add("高雄SDP2總供裝數量為" + iSDPTotalNum[1] + "\r\n");
                // 取得7L2SDP3的總供裝數目
                if (iSDPTotalNum[2] != 0) log_msg.Add("高雄SDP3總供裝數量為" + iSDPTotalNum[2] + "\r\n");
                str.Close();
            }

            /* Debug - Verify all data is correct */
            if (!log_stat) log_msg.ForEach(i => Debug.WriteLine("{0}\t", i));

            /* Save all parse result to structure*/
            result.temp = 0;
            result.cpu = 0;
            result.mem = 0;
            result.disk = 0;
            result.log_stat = log_stat;
            result.syslog_msg = log_msg;
            result.intf_stat = false;
            result.ntp_stat = false;
        }

        /// <summary>
        /// 取得PCRF授權數及高雄、台北IP session數
        /// The Function can parse 7L2PCRF_License_view log, and it will extract three value,
        /// 1. PCRF global license max number
        /// 2. PCRF provision number
        /// 3. PCRF session monitor
        /// </summary>
        /// <param name="log">7L2PCRF_License_view.log</param>
        /// <param name="result"></param>
        public void parser_PCRFLicense_log(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            bool log_stat = true;
            List<string> log_msg = new List<string>();

            //Read raw file, and process parser content
            Debug.WriteLine("@log name: " + log);
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    Debug.WriteLine(line);
                    if (line.IndexOf("PCRFlicense", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        bool err = false;
                        line = str.ReadLine();
                        line = str.ReadLine();
                        if (!err) err = true;
                        log_msg.Add("7L2PCRF Session License 數量為" + line + "000" + "\r\n");
                        if (err) log_stat = false;
                    }
                    // 取得PCRF目前的供裝數
                    if (line.IndexOf("PCRFSubscription", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        line = str.ReadLine();
                        log_msg.Add("7L2PCRF 目前供裝數量為" + line + "\r\n");
                    }
                    // 取得PCRF目前的Session數
                    if (line.IndexOf("PCRFSession", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        line = str.ReadLine();
                        line = str.ReadLine();
                        log_msg.Add("7L2PCRF 目前的Session數量為" + line + "\r\n");
                    }
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            if (!log_stat) log_msg.ForEach(i => Debug.WriteLine("{0}\t", i));

            /* Save all parse result to structure*/
            result.temp = 0;
            result.cpu = 0;
            result.mem = 0;
            result.disk = 0;
            result.log_stat = log_stat;
            result.syslog_msg = log_msg;
            result.intf_stat = false;
            result.ntp_stat = false;
        }

        /// <summary>
        /// 確認所有HP ILOM是否有硬體異常的紀錄
        /// </summary>
        /// <param name="log">ilo_check.log</param>
        /// <param name="result"></param>
        public void parser_ILO_Check_log(String log, ref DevResource.devSysChkResult result)
        {
            //Extract variable
            bool log_stat = true; // true: not found key message
            List<string> log_msg = new List<string>();

            //Read raw file, and process parser content
            Debug.WriteLine("@log name: " + log);
            using (StreamReader str = new StreamReader(@log, Encoding.Default))
            {
                String line;
                while ((line = str.ReadLine()) != null)
                {
                    if (line.IndexOf("component", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        //Debug.WriteLine(line);
                        if (log_stat) log_stat = false; //Flag found key message in log
                        string hostname = string.Copy(line).Replace(" has fault component!!", "");
                        var xpath_list = new List<string>();
                        log_msg.Add(hostname + "有下列障礙：\r\n");
                        while (line.IndexOf("========", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            line = str.ReadLine();
                            if (line.IndexOf("BIOS_HARDWARE", StringComparison.OrdinalIgnoreCase) >= 0)
                            { log_msg.Add("\tILOM BIOS錯誤\r\n"); }
                            if (line.IndexOf("FANS", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                log_msg.Add("\t設備風扇異常\r\n");
                                xpath_list.Add("/ribcl/fans/*");
                            }
                            if (line.IndexOf("TEMPERATURE", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                log_msg.Add("\t設備溫度異常\r\n");
                                xpath_list.Add("/ribcl/temperature/*");
                            }
                            if (line.IndexOf("POWER_SUPPLIES", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                log_msg.Add("\t設備電源異常\r\n");
                                xpath_list.Add("/ribcl/power_supplies/*");
                            }
                            if (line.IndexOf("BATTERY", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                log_msg.Add("\t設備Storage電池異常\r\n");
                                xpath_list.Add("/ribcl/power_supplies/*");
                            }
                            if (line.IndexOf("PROCESSOR", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                log_msg.Add("\t設備CPU異常\r\n");
                                xpath_list.Add("/ribcl/processors/*");
                            }
                            if (line.IndexOf("MEMORY", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                log_msg.Add("\t設備記憶體異常\r\n");
                                xpath_list.Add("/ribcl/memory/*");
                            }
                            if (line.IndexOf("NETWORK", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                log_msg.Add("\t設備網路元件異常\r\n");
                                xpath_list.Add("/ribcl/nic_information/*");
                            }
                            if (line.IndexOf("STORAGE", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                log_msg.Add("\t設備Storage異常\r\n");
                                xpath_list.Add("/ribcl/storage/*");
                            }
                        }
                        #region Extract detail of failure component
                        /*
                        string hostXML = string.Empty;
                        //Load detail info to HtmlAgilityPack object
                        while (line.IndexOf("--------", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            line = str.ReadLine();
                            hostXML = string.Concat(hostXML, line + "\r\n");
                        }
                        log_msg.Add(hostname + "異常元件資訊如下: ");
                        foreach (var xpath in xpath_list)
                        {
                            //value = comm_parse_hp_status_value(line);
                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(hostXML);
                            foreach (HtmlNode hnValue in doc.DocumentNode.SelectNodes(xpath))
                                log_msg.Add(hnValue.InnerHtml + "\r\n");
                        }
                        */
                        #endregion
                    }
                }
                str.Close();
            }

            /* Debug - Verify all data is correct */
            Debug.WriteLine("HP found error: " + !log_stat);
            if (!log_stat) log_msg.ForEach(i => Debug.WriteLine("{0}\t", i));

            /* Save all parse result to structure*/
            result.temp = 0;
            result.cpu = 0;
            result.mem = 0;
            result.disk = 0;
            result.log_stat = log_stat;
            result.syslog_msg = log_msg;
            result.intf_stat = false;
            result.ntp_stat = false;
        }
    }
}
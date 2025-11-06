using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OCS_Parser_Minoss
{

    class DevResource
    {
        /*
        struct devResTemplate
        {
            private string host_name;
            private string host_group;
            private string ip;
            private int port;
            private string login_username;
            private string login_password;
            private string supper_username;
            private string supper_password;
            private Boolean enable;
        };
        */
        public struct devSysChkResult
        {
            int _temp;
            float _cpu;
            float _mem;
            int _disk;
            bool _log_stat;
            bool _intf_stat;
            bool _ntp_stat;
            List<string> _syslog_msg;
            public int temp
            {
                get { return _temp; }
                set { if (value >= 0 & value <=100) _temp = value; }
            }
            public float cpu
            {
                get { return _cpu; }
                set
                {
                    if (value >= 0 & value <= 100)
                        if (value < 1)
                            _cpu = 1;
                        else
                            _cpu = value; 
                }
            }
            public float mem
            {
                get { return _mem; }
                set { if (value >= 0 & value <= 100) _mem = value; }
            }
            public int disk
            {
                get { return _disk; }
                set { if (value >= 0 & value <= 100) _disk = value; }
            }
            public bool log_stat
            {
                get { return _log_stat; }
                set { _log_stat = value; }
            }
            public List<string> syslog_msg
            {
                get { return _syslog_msg; }
                set { _syslog_msg = value; }
            }
            public bool intf_stat
            {
                get { return _intf_stat; }
                set { _intf_stat = value; }
            }
            public bool ntp_stat
            {
                get { return _ntp_stat; }
                set { _ntp_stat = value; }
            }
        }
        public void initialResource()
        {
            return;
        }
    }
}

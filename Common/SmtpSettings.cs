using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class SmtpSettings
    {
        public static string Host { get; set; } = "smtp.gmail.com";
        public static int Port { get; set; } = 587;
        public static bool EnableSsl { get; set; } = true;
        public static string Username { get; set; } = "yousof.hosseinpour@gmail.com";
        public static string Password { get; set; } = "kwfp manq grzv rkgc";
        public static string FromEmail { get; set; } = "yousof.hosseinpour@gmail.com";
        public static string FromName { get; set; } = "Rad";
    }
}

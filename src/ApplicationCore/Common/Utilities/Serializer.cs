using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ApplicationCore.Common.Utilities
{
    public static class Serializer
    {
        public static string GetSerializedData<T1>(T1 data)
        {
            if (Equals(data, default(T1)))
            {
                return string.Empty;
            }

            foreach (var pi in data.GetType().GetProperties())
            {
                try
                {
                    if (pi.GetValue(data).GetType().ToString() == "System.String" && pi.GetValue(data).ToString().Length > 50)
                    {
                        pi.SetValue(data, string.Empty);
                    }
                }
                catch
                {
                    // ..
                }
            }

            return JsonConvert.SerializeObject(data);
        }

        public static string GetAuthHeaderValue(string url, string path)
        {
            var input = string.Concat(url, path.Split('?')[0], DateTime.UtcNow.ToString("yyyyMMdd hh"));
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();

            foreach (var h in hash)
            {
                sb.Append(h.ToString("X2"));
            }

            return sb.ToString();
        }

        public static string GetAuthHeaderValueV2(string url, string path)
        {
            var ip = GetIpAddress();
            var input = string.Concat(url, path.Split('?')[0], ip);
            var sha256 = SHA256.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = sha256.ComputeHash(inputBytes);
            var sb = new StringBuilder();

            foreach (var h in hash)
            {
                sb.Append(h.ToString("X2"));
            }

            return sb.ToString();
        }

        public static string GetIpAddress()
        {
            var ipHostInfo = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList.First(it => it.ToString().Split('.').Length == 4);
            return ipAddress.ToString();
        }
    }
}
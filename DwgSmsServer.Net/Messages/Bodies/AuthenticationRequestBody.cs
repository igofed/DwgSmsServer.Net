using System.Linq;
using System.Text;

namespace DwgSmsServerNet.Messages.Bodies
{
    class AuthenticationRequestBody : DwgMessageBody
    {
        public AuthenticationRequestBody(byte[] bytes)
        {
            User = Encoding.ASCII.GetString(bytes.Take(16).ToArray()).Trim('\0');
            Password = Encoding.ASCII.GetString(bytes.Skip(16).Take(16).ToArray()).Trim('\0');
        }

        public string User { get; private set; }
        public string Password { get; private set; }

        public override string ToString()
        {
            return string.Format("User:{0}; Password:{1}", User, Password);
        }
    }
}

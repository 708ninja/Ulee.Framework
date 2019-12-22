using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulee.Device
{
    public class DeviceException : ApplicationException
    {
        public int Code { get; private set; }

        public DeviceException(string msg = "Occurred Device Exception!", int code = 0)
            : base(msg)
        {
            Code = code;
        }
    }
}

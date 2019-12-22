using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ulee.Device;

namespace Ulee.Device.Ajin
{
    public class AxtException : ApplicationException
    {
        public AXT_FUNC_RESULT Code { get; private set; }

        public AxtException(string msg = "Occurred Ajinextek device exception!", 
            AXT_FUNC_RESULT code = AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            : base(msg)
        {
            Code = code;
        }
    }

    public enum EAxtDio { DI, DO };

    public enum EAxtAio { AI, AO };

    public abstract class AxtObject
    {
        public AxtObject()
        {
        }

        protected void Validate(AXT_FUNC_RESULT code, string msg)
        {
            if (code == AXT_FUNC_RESULT.AXT_RT_SUCCESS) return;

            msg = $"Occurred {code.ToString()}({(UInt32)code}) exception in {msg}";

            throw new AxtException(msg, code);
        }
    }
}

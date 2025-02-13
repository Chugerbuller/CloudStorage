using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.BL.Exceptions
{
    public class LoginException : Exception
    {
        public LoginException() : base("Nonexistent login")
        {            
        }
    }
}

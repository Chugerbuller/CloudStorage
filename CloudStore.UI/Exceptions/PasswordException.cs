using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.UI.Exceptions
{
    public class PasswordException : Exception
    {
        public PasswordException() : base("Incorrect password")
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.UI.Exceptions
{
    public class NotValidException : Exception
    {
        public NotValidException() : base("Not valid password or login.")
        {
            
        }
    }
}

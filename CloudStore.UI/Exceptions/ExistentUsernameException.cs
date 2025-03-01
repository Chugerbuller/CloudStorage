using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.UI.Exceptions
{
    public class ExistentUsernameException : Exception
    {
        public ExistentUsernameException() : base("This user-name is exist")
        {
            
        }
    }
}

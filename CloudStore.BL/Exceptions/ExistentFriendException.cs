using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.BL.Exceptions
{
    public class ExistentFriendException : Exception
    {
        public ExistentFriendException(): base("Friend is exist in list")
        {           
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions
{
    public class NotFoundEntityException:Exception
    {
        public NotFoundEntityException(string message) : base(message)
        {
        }
        public NotFoundEntityException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}

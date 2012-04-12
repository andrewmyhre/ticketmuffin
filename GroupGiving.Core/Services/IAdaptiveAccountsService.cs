using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupGiving.Core.Services
{
    public interface IAdaptiveAccountsService
    {
        bool AccountIsVerified(string email, string firstname, string lastname);
    }
}

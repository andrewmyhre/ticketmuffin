using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.PayPal;

namespace GroupGiving.Core.Services
{
    public interface IAdaptiveAccountsService
    {
        GetVerifiedStatusResponse AccountIsVerified(string email, string firstname, string lastname);
    }
}

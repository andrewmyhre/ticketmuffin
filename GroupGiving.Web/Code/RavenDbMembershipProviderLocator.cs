using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace GroupGiving.Web.Code
{
    public interface IMembershipProviderLocator
    {
        MembershipProvider Provider();
    }

    public class RavenDbMembershipProviderLocator : IMembershipProviderLocator
    {
        public MembershipProvider Provider()
        {
            return Membership.Provider;
        }
    }
}
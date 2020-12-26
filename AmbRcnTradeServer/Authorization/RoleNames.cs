using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Authorization
{
    public sealed class RoleNames
    {
        public const string ADMIN = "admin";
        public const string USER = "user";
        public const string INSPECTOR = "inspector";
        public const string GUEST = "guest";
        public const string GUEST_NO_PRICES = "guestNoPrices";
    }

    public sealed class AuthorizeRoles
    {
        public const string ADMIN = RoleNames.ADMIN;
        public const string EVERYONE = ADMIN + "," + RoleNames.USER; // + "," + RoleNames.WAREHOUSE_MANAGER + "," + RoleNames.INSPECTOR;
        public const string COMPANY_USERS = ADMIN + "," + RoleNames.USER;
    }
}
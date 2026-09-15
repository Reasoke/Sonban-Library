using Microsoft.AspNetCore.Mvc;
using Sonban.Common.Classes;

namespace Sonban.Api.Controllers
{
    [ApiController]
    public class BaseApiController : Controller {

        protected bool Authorized => User.Identity != null && User.Identity.IsAuthenticated && User.Identity is SonbanUserIdentity;
        protected SonbanUserIdentity CurrentIdentity => User.Identity as SonbanUserIdentity;
    }
}

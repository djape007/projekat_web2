using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using WebApp.Models;

namespace WebApp.Providers
{
    public class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            ApplicationUserManager userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null)
            {
                context.SetError("invalid_grant", "Email i/ili šifra nisu ispravni.");
                return;
            }

            //if (!user.EmailConfirmed)
            //{
            //    context.SetError("invalid_grant", "AppUser did not confirm email.");
            //    return;
            //}

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, "JWT");
          
            var ticket = new AuthenticationTicket(oAuthIdentity, null);
            //custom claims                                ako bude null pa da ne padne server
            ticket.Identity.AddClaim(new Claim("userType", (user.Type == null ? "" : user.Type)));
            ticket.Identity.AddClaim(new Claim("userStatus", (user.Status == null ? "" : user.Status)));
            ticket.Identity.AddClaim(new Claim("userFiles", (user.Files == null ? "" : user.Files)));
            ticket.Identity.AddClaim(new Claim("userHasDocuments", user.HasDocument.ToString()));

            context.Validated(ticket);
        }
    }
}
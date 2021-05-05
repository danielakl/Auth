using System;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Api.Controllers
{
    public class AuthorizationController : ControllerBase
    {
        [HttpPost("connect/token")]
        public IActionResult Exchange()
        {
            var openIdRequest = HttpContext.GetOpenIddictServerRequest() ??
                                throw new InvalidOperationException("OpenID Connect request cannot be retrieved");
            
            ClaimsPrincipal claimsPrincipal;

            if (openIdRequest.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.
                
                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                
                // Subject (sub) is a required field, we use the client id as the subject identifier here.
                identity.AddClaim(OpenIddictConstants.Claims.Subject, openIdRequest.ClientId!);

                // Add some claim, don't forget to add destination otherwise it won't be added to the access token.
                identity.AddClaim("some-claim", "some-value", OpenIddictConstants.Destinations.AccessToken);

                claimsPrincipal = new ClaimsPrincipal(identity);

                claimsPrincipal.SetScopes(openIdRequest.GetScopes());
            }
            else
            {
                throw new InvalidOperationException("Specified grant type is not supported.");
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
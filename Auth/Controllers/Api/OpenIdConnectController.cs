using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Auth.Controllers.Api
{
    [ApiController]
    [Route("api/connect")]
    public class OpenIdConnectController : ControllerBase
    {
        [HttpGet("authorize")]
        [HttpPost("authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var openIdRequest = HttpContext.GetOpenIddictServerRequest() ??
                                throw new InvalidOperationException("OpenId Connect request cannot be retrieved");

            // Retrieve the user principal stored in the authentication cookie.
            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // If the user principal can't be extracted, redirect the user to the login page.
            if (!authResult.Succeeded)
            {
                return Challenge(
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form : Request.Query)
                    });
            }

            // Create a new claims principal
            var claims = new[]
            {
                // 'subject' claim which is required
                new Claim(OpenIddictConstants.Claims.Subject, authResult.Principal!.Identity!.Name!),

                // Add Access Claims
                new Claim("some claim", "some value").SetDestinations(OpenIddictConstants.Destinations.AccessToken),

                // Add Identity Claims
                new Claim(OpenIddictConstants.Claims.Name, "some").SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
                new Claim(OpenIddictConstants.Claims.Username, "someusername").SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
                new Claim(OpenIddictConstants.Claims.Email, "some@email.com").SetDestinations(OpenIddictConstants.Destinations.IdentityToken)
            };

            var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Set requested scopes (this is not done automatically)
            // TODO: Implement user consent screen to determine which scopes to set
            claimsPrincipal.SetScopes(openIdRequest.GetScopes());

            // Signing in with the OpenIddict authentication scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("token")]
        public async Task<IActionResult> Exchange()
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
            else if (openIdRequest.IsAuthorizationCodeGrantType())
            {
                // Retrieve the claims principal stored in the authorization code
                var authResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                claimsPrincipal = authResult.Principal;
            }
            else
            {
                throw new InvalidOperationException("Specified grant type is not supported.");
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("userinfo")]
        public async Task<IActionResult> UserInfo()
        {
            var authResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var claimsPrincipal = authResult.Principal;

            return Ok(new
            {
                Name = claimsPrincipal!.GetClaim(OpenIddictConstants.Claims.Subject),
                Occupation = "Developer",
                Age = 43
            });
        }
    }
}
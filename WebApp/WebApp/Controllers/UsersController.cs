using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using WebApp.Models;
using WebApp.Providers;
using WebApp.Results;
using WebApp.Persistence.UnitOfWork;
using System.Web.Http.Description;
using System.Data.Entity.Infrastructure;
using System.Net;
using System.IO;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using WebApp.App_Start;

namespace WebApp.Controllers
{
    [Authorize]
    [RoutePrefix("api/Users")]
    public class UsersController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;


        private void CreateUserFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string GetUserFolderPath(string userId)
        {
            return System.Web.Hosting.HostingEnvironment.MapPath("~/imgs/users/" + userId);
        }

        private bool IsCorrectFileExtension(string fileName)
        {
            var fileExtension = fileName.Split('.').Last();

            if (fileExtension == "jpg" || fileExtension == "jpeg" || fileExtension == "png" || fileExtension == "bmp")
            {
                return true;
            }
            return false;
        }

        // GET: api/Users
        [Authorize(Roles = "Admin")]
        public List<ApplicationUser> GetUsers()
        {
            return UserManager.Users.ToList();
        }

        [Route("ProcessingUsers")]
        [Authorize(Roles = "Controller")]
        [HttpGet]
        public List<ApplicationUser> GetProcessingUsers()
        {
            return UserManager.Users.ToList().FindAll(x => x.Status == "processing");
        }

        // GET: api/Users/5
        [ResponseType(typeof(ApplicationUser))]
        [Authorize(Roles = "Admin, AppUser, Controller")]
        public IHttpActionResult GetUser(string id)
        {
            ApplicationUser user = UserManager.Users.ToList().Find(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        [Authorize(Roles = "Admin, AppUser, Controller")]
        public async Task<IHttpActionResult> PutUser(string id, ApplicationUser user)
        {
            if (!ModelState.IsValid || user == null)
            {
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                return BadRequest();
            }

            ApplicationUser userInDB = UserManager.Users.Where(x => x.Id == id).FirstOrDefault();

            if (userInDB == null)
            {
                return BadRequest("Korisnik ne postoji");
            }

            userInDB.Address = user.Address;
            userInDB.DateOfBirth = user.DateOfBirth;
            //userInDB.EmailConfirmed = user.EmailConfirmed;
            //userInDB.Type = user.Type;
            userInDB.Name = user.Name;
            userInDB.Surname = user.Surname;

            IdentityResult result = await UserManager.UpdateAsync(userInDB);

            if (!result.Succeeded) {
                return BadRequest();
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(ApplicationUser))]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult DeleteUser(string id)
        {
            ApplicationUser user = UserManager.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            UserManager.Delete(user);

            return Ok(user);
        }

        [AllowAnonymous]
        public bool UserExists(string email)
        {
            return UserManager.Users.FirstOrDefault(x => x.Email == email) != null;
        }

        public UsersController()
        {

        }

        public UsersController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        //PUT api/users/verify/ID
        [Route("Verify/{id}")]
        [Authorize(Roles = "Controller")]
        [HttpPut]
        public IHttpActionResult VerifyUserDocuments(string id) {
            var userInDb = UserManager.Users.FirstOrDefault(x => x.Id == id);

            if (userInDb == null) {
                return BadRequest("User does not exist");
            }

            if (!UserManager.IsInRole(id, "AppUser")) {
                return BadRequest();
            }

            userInDb.Status = "verified";
            UserManager.Update(userInDb);

            //string subject = "Status klijenta";
            //string body = "<p>Poštovani " + userInDb.Name + " " + userInDb.Surname + "</p><p>Vas zahtev je ODOBREN.</p><p><b>Vinko Klocna Prevoz</b></p>";
            //SendEmailConfig.Execute(userInDb.Email, subject, body);

            return Ok();
        }

        //PUT api/users/deny/ID
        [Route("Deny/{id}")]
        [Authorize(Roles = "Controller")]
        [HttpPut]
        public IHttpActionResult DenyUserDocuments(string id) {
            var userInDb = UserManager.Users.FirstOrDefault(x => x.Id == id);

            if (userInDb == null) {
                return BadRequest("User does not exist");
            }

            if (!UserManager.IsInRole(id, "AppUser")) {
                return BadRequest();
            }

            userInDb.Status = "denied";
            UserManager.Update(userInDb);

            //string subject = "Status klijenta";
            //string body = "<p>Poštovani " + userInDb.Name + " " + userInDb.Surname + "</p><p>Vas zahtev je ODBIJEN.</p><p><b>Vinko Klocna Prevoz</b></p>";
            //SendEmailConfig.Execute(userInDb.Email, subject, body);

            return Ok();
        }

        [Route("UploadFiles")]
        [ResponseType(typeof(string))]
        [HttpPost]
        public IHttpActionResult FileUpload() {
            var request = HttpContext.Current.Request;

            var userId = User.Identity.GetUserId();

            var userInDb = UserManager.Users.FirstOrDefault(x => x.Id == userId);

            if (userInDb == null) {
                return BadRequest("User does not exist");
            }

            var mappedPath = GetUserFolderPath(userId);
            CreateUserFolder(mappedPath);

            if (request.Files.Count == 0) {
                return BadRequest("No files selected");
            }

            List<string> listaNazivaUploadovanihFajlova = new List<string>();
            foreach (string file in request.Files) {
                var postedFile = request.Files[file];

                if (IsCorrectFileExtension(postedFile.FileName)) {
                    postedFile.SaveAs(mappedPath + "/" + postedFile.FileName);
                    listaNazivaUploadovanihFajlova.Add(postedFile.FileName);
                }
            }

            if (listaNazivaUploadovanihFajlova.Count == 0) {
                return BadRequest("Invalid file type");
            }

            if (userInDb.Files != null & userInDb.Files.Length > 0) {
                userInDb.Files += "," + String.Join(",", listaNazivaUploadovanihFajlova);
            } else {
                userInDb.Files = String.Join(",", listaNazivaUploadovanihFajlova);
            }

            userInDb.Status = "processing";
            UserManager.Update(userInDb);

            return Ok(userInDb.Files);
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                   OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [ResponseType(typeof(ApplicationUser))]
        [Route("Register")]
        //public async Task<IHttpActionResult> Register([FromBody]ApplicationUser newUser, [FromBody]HttpPostedFile postedFile)
        /*public async Task<IHttpActionResult> Register(
            [FromBody] string Email,
            [FromBody] string DateOfBirth,
            [FromBody] string Address,
            [FromBody] string Name,
            [FromBody] string Surname,
            [FromBody] string Type,
            [FromBody] string Password,
            [FromBody] HttpPostedFile postedFile)*/
        public async Task<IHttpActionResult> Register()
        {
    
            var request = HttpContext.Current.Request;
            

            string[] validate = { "Email", "DateOfBirth", "Password", "Name", "Surname"};
            foreach(var itemToValidate in validate)
            {
                if (request.Form.Get(itemToValidate) == null || request.Form.Get(itemToValidate).Trim().Length == 0)
                {
                    return BadRequest(itemToValidate + " is required");
                }
            }


            var status = "verified";

            if (request.Form.Get("Type") != "Obican")
            {
                status = "not verified";
            }

            var user = new ApplicationUser()
            {
                UserName = request.Form.Get("Email"),
                Email = request.Form.Get("Email"),
                DateOfBirth = DateTime.Parse(request.Form.Get("DateOfBirth")),
                Address = request.Form.Get("Address"),
                Name = request.Form.Get("Name"),
                Surname = request.Form.Get("Surname"),
                Status = status,
                Type = request.Form.Get("Type"),
                Files = ""
            };

            IdentityResult result = await UserManager.CreateAsync(user, request.Form.Get("Password"));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            } else {
                UserManager.AddToRole(user.Id, "AppUser");

                //ako je dodao usera, onda cemo i fajl da sacuvamo (ako ga je poslao)
                if (request.Files.Count > 0)
                {
                    var mappedPath = GetUserFolderPath(user.Id);
                    CreateUserFolder(mappedPath);
                    
                    List<string> listaNazivaUploadovanihFajlova = new List<string>();
                    foreach (string file in request.Files)
                    {
                        var postedFile = request.Files[file];

                        if (IsCorrectFileExtension(postedFile.FileName))
                        {
                            postedFile.SaveAs(mappedPath + "/" + postedFile.FileName);
                            listaNazivaUploadovanihFajlova.Add(postedFile.FileName);
                        }
                    }

                    if (listaNazivaUploadovanihFajlova.Count > 0)
                    {
                        if (user.Files != null & user.Files.Length > 0)
                        {
                            user.Files += "," + String.Join(",", listaNazivaUploadovanihFajlova);
                        }
                        else
                        {
                            user.Files = String.Join(",", listaNazivaUploadovanihFajlova);
                        }

                        user.Status = "processing";
                        UserManager.Update(user);
                    }
                }
            }

            //if(user.Status == "processing") {
            //    string subject = "Status klijenta";
            //    string body = "<p>Poštovani " + user.Name + " " + user.Surname + "</p><p>Vas status se verifikuje.</p><p><b>Vinko Klocna Prevoz</b></p>";
            //    SendEmailConfig.Execute(user.Email, subject, body);
            //}

            return Ok(user);
        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result); 
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}

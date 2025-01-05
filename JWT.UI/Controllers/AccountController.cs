using JWT.UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace JWT.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        //Interface'i doğrudan alan olarak saklamanın yerine, somut HttpClient sınıfı saklanıyor.
        //Birden fazla yerde API çağrısı yapmak istediğinizde, her seferinde CreateClient çağırmanıza gerek kalmaz

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }
        [Authorize]
        public async Task<IActionResult> UserInfo()
        {
            UserModel userModel = new UserModel();
            ResponseMainModel<UserModel> responseModel = null;
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Büyük/küçük harf farkını görmezden gelir
            };

            //Kodda kullanılan (User.Identity.IsAuthenticated) ifadesi, ASP.NET Core framework'ü tarafından sağlanan bir özelliktir ve kullanıcı kimlik doğrulamasını kontrol etmek için kullanılır.
            //Bu ifade, ControllerBase sınıfından türeyen kontrolörlerde bulunan User nesnesinin bir özelliğidir.
            if (User.Identity.IsAuthenticated) 
            {
                var accessToken = User.Claims.FirstOrDefault(x =>x.Type == "access_token")?.Value;

                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }
            var result = await _httpClient.GetAsync("api/User/GetUserInfoFromLogined");

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync();
                return Redirect(String.Format("~/Account/Login?ReturnUrl=/{0}/{1}",
                    ControllerContext.ActionDescriptor.ControllerName,
                    ControllerContext.ActionDescriptor.ActionName));
            }

            if (result.IsSuccessStatusCode)
            {
                var kontrol = await result.Content.ReadAsStringAsync();
                responseModel = JsonSerializer.Deserialize<ResponseMainModel<UserModel>>(await result.Content.ReadAsStringAsync(), options);
                userModel = responseModel?.Data ?? new UserModel() { };
                //?? new UserModel(): Eğer responseModel null ya da Data boş ise, yeni bir UserModel nesnesi oluşturur.
            }
            return View(userModel);
        }

        

        public IActionResult Forbidden()
        {
            return View();
        }

        public IActionResult Logout(string ReturnUrl = null)
        {
            HttpContext.SignOutAsync();
            return Redirect("~/");
        }

        [HttpGet]
        public IActionResult Login(string ReturnUrl = null)
        {

            return View(new LoginModel()
            {
                ReturnUrl = ReturnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (!ModelState.IsValid) 
            {
                return View(loginModel);
            }
            string jsonString = JsonSerializer.Serialize(loginModel);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await _httpClient.PostAsync("/api/Auth/Login", content);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Büyük/küçük harf farkını görmezden gelir
            };

            var responseModel = JsonSerializer.Deserialize<ResponseMainModel<ResponseTokenModel>>(await result.Content.ReadAsStringAsync(),options);
            if (result.IsSuccessStatusCode)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(responseModel.Data.AccessToken);

                var claims = new List<Claim>
                {
                    //MVC tarafında yeniden claim oluşturmak gereklidir, çünkü API'den gelen token'ın sadece bir string olduğunu unutmayın.
                    //MVC uygulamasının bu string'deki claim bilgilerini okuyup ClaimsPrincipal nesnesine dönüştürmesi gerekir. Bu olmadan kullanıcı oturumu açılmaz.
                    //Ancak, burada tekrar aynı işlemleri yapıyor gibi görünse de farklı bir amaç güdülüyor:
                    //API Tarafında: Token oluşturulurken claim bilgileri JWT içine gömülür.
                    //MVC Tarafında: Bu token çözülür ve claim'ler MVC'nin kimlik doğrulama mekanizması(CookieAuthentication) için kullanılır.
                    new Claim(ClaimTypes.NameIdentifier, jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value),
                    new Claim(ClaimTypes.Email,  jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value ),
                    new Claim(ClaimTypes.Name,  jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value ),
                    new Claim("access_token",  responseModel.Data.AccessToken ),
                    new Claim("refresh_token",  responseModel.Data.RefreshToken ),
                };
                var roleClaims = jwtSecurityToken.Claims.Where(x => x.Type == ClaimTypes.Role);

                if (!roleClaims.Any())
                {
                    claims.Add(new Claim(ClaimTypes.Role, "User"));
                }

                claims.AddRange(roleClaims);

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = responseModel.Data.AccessTokenExpiration,
                };
                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

                return Redirect(loginModel.ReturnUrl ?? "~/");
                //Kullanıcı oturum açtıktan sonra, geldiği sayfaya (veya varsayılan ana sayfaya) yönlendirilir
            }

            else
            {
                foreach (var error in responseModel.Errors)
                {
                    ModelState.AddModelError("other", error);
                    return View(loginModel);
                }
            }
            ModelState.AddModelError("", "Girilen kullanıcı adı veya parola yanlış");
            return View(loginModel);
        }
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            RegisterModel model = new RegisterModel();
            model.RoleSelectList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();

            string jsonString = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await _httpClient.GetAsync("/api/User/roles");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var responseMainModel = JsonSerializer.Deserialize<ResponseMainModel<List<string>>>(await result.Content.ReadAsStringAsync(),options);

            if (responseMainModel.IsSuccessful)
            {
                foreach (var item in responseMainModel.Data)
                {
                    //ASP.NET MVC'de bir SelectListItem nesnesi, bir HTML <select> (dropdown) öğesinin içindeki <option> etiketlerini temsil eder.
                    // TextDropdown listesinde kullanıcıya görünen metni temsil eder.
                    //Dropdown'dan seçim yapıldığında, form gönderilirken arka uca iletilen değeri temsil eder.
                    model.RoleSelectList.Add(new SelectListItem { Text = item, Value = item });
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerModel);
            }

            string jsonString = JsonSerializer.Serialize(registerModel);
            var content = new StringContent(jsonString,Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var result = await _httpClient.PostAsync("/api/User/Register", content);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var responseModel = JsonSerializer.Deserialize<ResponseMainModel<RegisterModel>>(await result.Content.ReadAsStringAsync(),options);

            if (result.IsSuccessStatusCode)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                foreach (var error in responseModel.Errors)
                {
                    ModelState.AddModelError("other", error);
                    return View(registerModel);
                }
            }
            return View(registerModel);
        }


    }
}

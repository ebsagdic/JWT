using JWT.UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace JWT.UI.Controllers
{
    [Authorize(Roles = "ProductManager,Admin")]
    public class CustomerController : Controller
    {
        private readonly HttpClient _httpClient;
        public CustomerController(IHttpClientFactory httpClient )
        {
            _httpClient = httpClient.CreateClient("ApiClient");
        }

        public async Task<IActionResult> Index()
        {
            CustomerListModel customerListModel = new CustomerListModel();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (User.Identity.IsAuthenticated)
            {
                var accessToken = User.Claims.FirstOrDefault(x => x.Type == "access_token").Value;
                var refresh_token = User.Claims.FirstOrDefault(x => x.Type == "refresh_token").Value;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            ResponseMainModel<IEnumerable<CustomerModel>> responseModel = null;
            var result = await _httpClient.GetAsync("api/Customer/");

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync();
                return Redirect(String.Format("~/Account/Login?ReturnUrl=/{0}/{1}", ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName));
            }

            responseModel = JsonSerializer.Deserialize<ResponseMainModel<IEnumerable<CustomerModel>>>(await result.Content.ReadAsStringAsync(), options);

            if (!result.IsSuccessStatusCode || responseModel != null)
            {
                if (responseModel != null)
                {
                    foreach (var error in responseModel.Errors)
                    {
                        ModelState.AddModelError("other", error);
                    }
                }
                else
                {
                    ModelState.AddModelError("other", result.StatusCode.ToString());
                }

            }
            customerListModel.Customers = responseModel.Data.ToList();

            return View(customerListModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerModel model)
        {
            if (ModelState.IsValid)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                if (User.Identity.IsAuthenticated)
                {
                    var accessToken = User.Claims.FirstOrDefault(x => x.Type == "access_token").Value;
                    var refresh_token = User.Claims.FirstOrDefault(x => x.Type == "refresh_token").Value;
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                string strModel = model.ToString();
                string jsonString = JsonSerializer.Serialize(model);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                ResponseMainModel<CustomerModel> responseModel = null;

                var result = await _httpClient.PostAsync("Customer", content);

                responseModel = JsonSerializer.Deserialize<ResponseMainModel<CustomerModel>>(await result.Content.ReadAsStringAsync(),options);

                if (result.IsSuccessStatusCode && responseModel != null)
                {
                    TempData["Success"] = "Yeni Ürün İşleminiz gerçekleştirildi";
                    return RedirectToAction("Index");
                }
                else
                {
                    if (responseModel != null)
                    {
                        foreach (var error in responseModel.Errors)
                        {
                            ModelState.AddModelError("other", error);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("other", result.StatusCode.ToString());
                    }

                }
            }
            return View(model);
        }

        public async Task<IActionResult> Update(int id)
        {
            CustomerModel model = new CustomerModel();
            CustomerListModel viewModel = new CustomerListModel();

            if (User.Identity.IsAuthenticated)
            {
                var accessToken = User.Claims.FirstOrDefault(x => x.Type == "access_token").Value;
                var refresh_token = User.Claims.FirstOrDefault(x => x.Type == "refresh_token").Value;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            }
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            ResponseMainModel<CustomerModel> responseModel = null;
            var result = await _httpClient.GetAsync("api/Customer/" + id);

            responseModel = JsonSerializer.Deserialize<ResponseMainModel<CustomerModel>>(await result.Content.ReadAsStringAsync(), options);

            if (!result.IsSuccessStatusCode || responseModel != null)
            {
                if (responseModel != null)
                {
                    foreach (var error in responseModel.Errors)
                    {
                        ModelState.AddModelError("other", error);
                    }
                }
                else
                {
                    ModelState.AddModelError("other", result.StatusCode.ToString());
                }

            }

            model = responseModel.Data;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CustomerModel model)
        {
            if (ModelState.IsValid) 
            {
                if (User.Identity.IsAuthenticated)
                {
                    var accessToken = User.Claims.FirstOrDefault(x => x.Type == "access_token").Value;
                    var refresh_token = User.Claims.FirstOrDefault(x => x.Type == "refresh_token").Value;
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                string strModel = model.ToString();
                string jsonString = JsonSerializer.Serialize(model);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                ResponseMainModel<CustomerModel> responseModel = null;
                var result = await _httpClient.PutAsync("api/Customer/" + model.Id, content);

                responseModel = JsonSerializer.Deserialize<ResponseMainModel<CustomerModel>>(await result.Content.ReadAsStringAsync(),options);

                if (result.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Güncelleme İşleminiz gerçekleştirildi";
                    return RedirectToAction("Index");
                }
                else
                {
                    if (responseModel != null)
                    {
                        foreach (var error in responseModel.Errors)
                        {
                            ModelState.AddModelError("other", error);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("other", result.StatusCode.ToString());
                    }

                }
            }
            return View(model);

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (User.Identity.IsAuthenticated)
            {
                var accessToken = User.Claims.FirstOrDefault(x => x.Type == "access_token").Value;
                var refresh_token = User.Claims.FirstOrDefault(x => x.Type == "refresh_token").Value;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            ResponseMainModel<NoDataModel> responseModel = null;
            var result = await _httpClient.DeleteAsync("api/Customer/" + id);

            responseModel = JsonSerializer.Deserialize<ResponseMainModel<NoDataModel>>(await result.Content.ReadAsStringAsync(), options);

            if (!result.IsSuccessStatusCode)
            {
                if (responseModel != null && responseModel.Errors != null)
                {
                    foreach (var error in responseModel.Errors)
                    {
                        throw new Exception(String.Join(", ", responseModel.Errors));
                    }
                }
                else
                {
                    throw new Exception(result.StatusCode.ToString());
                }
            }
            return Json(id);
        }

    }
}

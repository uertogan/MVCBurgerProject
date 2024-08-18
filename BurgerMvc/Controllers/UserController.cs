using AutoMapper;
using BurgerMvc.BLL.AbstractServices;
using BurgerMvc.BLL.Dtos;
using BurgerMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace BurgerMvc.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet] //Kayıt olma sayfasını gösterir.
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost] //Kullanıcı kaydı yapar.
        public async Task<IActionResult> Register(UserViewModel userViewModel)
        {
            // Fotoğraf yükleme işlemi
            if (userViewModel.PhotoUrl != null)
            {
                var fileName = Path.GetFileName(userViewModel.PhotoUrl.FileName);// Dosya adını alır.
                var filePath = Path.Combine("wwwroot", "Images", fileName);// Dosya yolunu belirler.

                // Dosyayı belirtilen konuma kaydeder.
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await userViewModel.PhotoUrl.CopyToAsync(stream);
                }
                userViewModel.Photo = fileName; // Fotoğraf adını ViewModel'e atar.
            }

            // UserViewModel'i UserDto'ya dönüştürür ve kullanıcı kaydı yapar.
            var userDto = _mapper.Map<UserDto>(userViewModel);
            await _userService.Register(userDto);
            return RedirectToAction("Login"); // Kayıttan sonra Login sayfasına yönlendirir.
        }

        public IActionResult Login() //Giriş sayfasını gösterir.
        {
            return View();
        }

        [HttpPost] //Kullanıcı giriş işlemi yapar.
        public async Task<IActionResult> Login(string email, string password)
        {
            var userDto = await _userService.Login(email, password); // Kullanıcıyı doğrular.
            if (userDto != null)
            {
                var userViewModel = _mapper.Map<UserViewModel>(userDto); // Dto'yu ViewModel'e dönüştürür.

                // Kullanıcı bilgilerini session'a kaydeder.
                HttpContext.Session.SetInt32("UserId", userViewModel.Id);
                HttpContext.Session.SetString("Email", userViewModel.Email);
                HttpContext.Session.SetString("IsAdmin", userViewModel.IsAdmin.ToString());

                // Kullanıcı admin ise admin sayfasına, değilse müşteri sayfasına yönlendirir.
                if (userViewModel.IsAdmin == true)
                {
                    return RedirectToAction("Admin", "Order", userViewModel);

                }
                return RedirectToAction("Index", "OrderCustomer", userViewModel);
            }
            // Giriş başarısız olursa hata mesajı gösterir.
            ViewData["Error"] = "E-posta veya şifre yanlış.";
            return View();
        }

        public async Task<IActionResult> Profille() //Kullanıcı profilini gösterir.
        {
            var userlogin = HttpContext.Session.GetInt32("UserId"); // Giriş yapmış kullanıcıyı alır.

            var users = await _userService.GetAllUsers(); // Tüm kullanıcıları getirir.
            var user = users.FirstOrDefault(x => x.Id == userlogin); // Giriş yapan kullanıcıyı bulur.
            var mapUser = _mapper.Map<UserViewModel>(user); // Kullanıcıyı ViewModel'e dönüştürür.

            return View(mapUser); // Profili gösterir ve profile kullanıcının bilgilerini gönderir.
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); //Kullanıcı çıkış yaptığında o kullanıcının giriş bilgileri sıfırlansın.
            return RedirectToAction("Login"); // Çıkış yaptıktan sonra Login sayfasına yönlendirir.
        }
    }
}


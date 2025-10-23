using AplicacionBase.Constantes;
using inventario_coprotab.Models.DBSeguridadCoprotab;
using inventario_coprotab.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace inventario_coprotab.Controllers
{
    public class AccountController : Controller
    {

        private readonly SeguridadCoprotabContext _seguridadCoprotabContext;

        public AccountController(SeguridadCoprotabContext seguridadCoprotabContext)
        {
            _seguridadCoprotabContext = seguridadCoprotabContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid) return View(loginViewModel);

            //var datos = await _seguridadCoprotabContext.Users.FromSqlRaw($@"EXEC Membership_GetUser '{ConstantesSistema.Sistema}','{loginViewModel.Email}','{loginViewModel.Password}'").ToListAsync();
            var datos = await _seguridadCoprotabContext.Users
                            .FromSqlInterpolated($@"EXEC Membership_GetUser {ConstantesSistema.Sistema}, {loginViewModel.Email}, {loginViewModel.Password}")
                            .ToListAsync();

            if (datos.Count != 0)
            {
                var user = await _seguridadCoprotabContext.Users.Where(x => x.UserId == datos[0].UserId).FirstAsync();
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("Email",datos[0].Email)
                };

                //var roles = await _seguridadCoprotabContext.Permisos.FromSqlRaw($@"EXEC UsersInPermisos_GetPermisosForUser '{ConstantesSistema.Sistema}','{datos[0].UserId}'").ToListAsync();
                var roles = await _seguridadCoprotabContext.Permisos
                            .FromSqlInterpolated($@"EXEC UsersInPermisos_GetPermisosForUser {ConstantesSistema.Sistema}, {datos[0].UserId}")
                            .ToListAsync();

                foreach (var rol in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol.PermisoName));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));


                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = loginViewModel.RememberMe,  // activa la cookie persistente
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) // duración de la cookie
                    });


                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.UserName.ToString());
                HttpContext.Session.SetString("Image", user.Image.ToString());
                HttpContext.Session.SetString("Email", user.Email.ToString());

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Error"] = "Credenciales Erroneas";
                return View(loginViewModel);
            }

        }


        public IActionResult ExtenderSesion()
        {
            // Solo tocar la sesión para renovarla
            HttpContext.Session.SetString("UltimaRenovacion", DateTime.Now.ToString());
            return Ok();
        }

        public async Task<IActionResult> Close()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Account");
        }



    }
}

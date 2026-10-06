using System.Security.Claims;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    public class AuthenticateController : ControllerBase
    {
 
        private readonly UserManager<IdentityUser> _userManager;// para manejar usuarios
        private readonly SignInManager<IdentityUser> _signInManager;// para manejar inicio de sesion
        private readonly JwtTokenService _Service;// para manejar el token JWT
        private readonly CustomerServices _customerService;// para manejar los clientes

        public AuthenticateController(UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager,JwtTokenService service,CustomerServices customerServices)
            {
            _userManager = userManager;
            _signInManager = signInManager;
            _Service = service;
            _customerService = customerServices;
        }

        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return Unauthorized("Usuario o contraseña incorrectos");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);// false para no bloquear la cuenta despues de varios intentos fallidos
            if (!result.Succeeded) 
            {
            return Unauthorized("Usuario o contraseña incorrectos");
            }

            string role; // por defecto el rol es cliente

            if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                role = "Customer";
            }else if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                role = "Admin";
            }
            else
            {
            role= "Employee"; // si no es cliente ni admin, es empleado
            }

                var token = _Service.GenerateToken(user.UserName, role);// Generar el token JWT
            return Ok(new { token });
        }

        [HttpPost("Register/Customers")]
         public async Task<IActionResult> RegisterCustomer([FromBody] RegisterModel request)
        {// validar mail
            var user = new IdentityUser() {
                Email = request.Email,
                UserName=request.Username,
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            
            await _userManager.AddToRoleAsync(user,"Customer"); // Asignar el rol de cliente al usuario

                var customer = await _customerService.AddCustomer(new CustomerModel.RequestCustomer
                        (
                        request.Name,
                        request.Email,
                        request.PhoneNumber,
                        user.UserName
                        ));

            // Aqui se puede agregar el rol del usuario si es necesario
            return Ok(customer);
        }

        [HttpPost("Register/Employee")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterModel request)
        {// validar mail

            var user = new IdentityUser()
            {
                Email = request.Email,
                UserName = request.Username,
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            // Obtener el ID del usuario recién creado


            await _userManager.AddToRoleAsync(user, "Employee"); // Asignar el rol de cliente al usuario


            // Aqui se puede agregar el rol del usuario si es necesario
            return Ok(user);
        }


    }
}

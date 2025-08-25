using Microsoft.AspNetCore.Mvc;
using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using Org.BouncyCastle.Crypto;
using AutoMapper;
using FeedUploader.Server.DTOModels;
using FeedUploader.Data.Services;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
namespace FeedUploader.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : Controller
	{
		private readonly IUserService _userService;
		private readonly IMapper _mapper;
		private readonly IJwtService _jwtService;
		public UserController(IUserService userService, IMapper mapper, IJwtService jwtService)
		{
			_userService = userService;
			_mapper = mapper;
			_jwtService = jwtService;
		}
		[HttpGet]
		public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
		{
			try
			{
				var users = await _userService.GetAll();
				var userDtos = _mapper.Map<IEnumerable<UserDTO>>(users);
				return Ok(userDtos);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while retrieving users");
			}
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<UserDTO>> GetUserById(int id)
		{
			try
			{
				var user = await _userService.GetById(id);
				if (user == null)
					return NotFound("User not found");
				var userDto = _mapper.Map<UserDTO>(user);
				return Ok(userDto);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while retrieving the user");
			}
		}

		[HttpGet("email/{email}")]
		public async Task<ActionResult<UserDTO>> GetUserByEmail(string email)
		{
			try
			{
				var user = await _userService.GetByEmail(email);
				if (user == null)
					return NotFound("User not found");
				var userDto = _mapper.Map<UserDTO>(user);
				return Ok(userDto);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while retrieving the user");
			}
		}

		[HttpPost]
		public async Task<ActionResult<UserDTO>> PostUser([FromBody] CreateUserDTO user)
		{
			if (!ModelState.IsValid)
				return BadRequest("Invalid user data");

			try
			{
				user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
				var userEntity = _mapper.Map<User>(user);
				if (!await _userService.Create(userEntity))
					return StatusCode(500, "Could not create user");
				var userDto = _mapper.Map<UserDTO>(userEntity);
				return CreatedAtAction(nameof(GetUserById), new { id = userDto.Id }, userDto);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while creating the user");
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(int id)
		{
			try
			{
				var user = await _userService.GetById(id);
				if (user == null)
					return NotFound("User not found");

				await _userService.RemoveById(id);
				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while deleting the user");
			}
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest("Invalid login data");

			try
			{
				var user = await _userService.GetByEmail(request.Email);
				if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
					return Unauthorized("Invalid email or password");

				var token = _jwtService.GenerateToken(user);
				return Ok(new { Token = token, User = new { user.Id, user.Name, user.Email, user.Role } });
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred during login");
			}
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest("Invalid registration data");

			try
			{
				var existingUser = await _userService.GetByEmail(request.Email);
				if (existingUser != null)
					return BadRequest("Email already in use");

				var newUser = new User
				{
					Name = request.Name,
					Surname = request.Surname,
					Email = request.Email,
					Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
					ContactNumber = request.ContactNumber,
					Role = request.Role
				};

				await _userService.Create(newUser);
				return Ok(new
				{
					Message = "Registration successful",
					User = new { newUser.Id, newUser.Name, newUser.Surname, newUser.Email, newUser.ContactNumber, newUser.Role }
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred during registration");
			}
		}
		public class LoginRequest
		{
			public string Email { get; set; }
			public string Password { get; set; }
		}
		public class RegisterRequest
		{
			public string Name { get; set; }
			public string Surname { get; set; }
			public string Email { get; set; }
			public string Password { get; set; }
			public string ContactNumber { get; set; }
			public string Role { get; set; }
		}
	}
}

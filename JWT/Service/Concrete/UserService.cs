using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Business.Abstracts;
using Entity.ModelsDtos;
using Microsoft.AspNetCore.Identity;
using JWT.Core.Model;
using JWT.Model;

namespace Business.Services
{
    public class UserService : IUserService
    {
      
        private readonly IMapper _mapper;
        private readonly UserManager<CustomUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._mapper = mapper;
        }
        public async Task<Response<UserDto>> CreateUserAsync(UserForRegisterDto userForRegisterDto)
        {
            var roleExists = await _roleManager.RoleExistsAsync(userForRegisterDto.SelectedRole);

            if (!roleExists)
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(userForRegisterDto.SelectedRole));
                if (!createRoleResult.Succeeded)
                {
                    var errors = createRoleResult.Errors.Select(x => x.Description).ToList();
                    return Response<UserDto>.Fail(400, errors);
                }
            }

            var user = new CustomUser { Email = userForRegisterDto.Email, UserName = userForRegisterDto.UserName, Name= userForRegisterDto.FirstName, Surname= userForRegisterDto.LastName };
           
            var result = await _userManager.CreateAsync(user, userForRegisterDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description).ToList();

                return Response<UserDto>.Fail(400, errors);
            }

             result = await _userManager.AddToRoleAsync(user, userForRegisterDto.SelectedRole);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description).ToList();

                return Response<UserDto>.Fail(400, errors);
            }

            UserDto userDto = _mapper.Map<UserDto>(user);
            return Response<UserDto>.Success(userDto, 200);
        }


        public Response<List<string>> GetAllRoles()
        {
            var roles = _roleManager.Roles.Select(x => x.Name).ToList();
            return Response<List<string>>.Success(roles, 200);
        }

        public async Task<Response<UserDto>> GetUserByNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
            var errors = new List<string>();
            
            if (user == null)
            {
                errors.Add("kullanıcı adı bulunamadı");
                return Response<UserDto>.Fail( 404, errors);
            }
            UserDto userDto = _mapper.Map<UserDto>(user);
            userDto.SelectedRoles = string.Join(", ", userRoles);
            userDto.FirstName = user.Name;
            userDto.LastName = user.Surname;
            return Response<UserDto>.Success(userDto, 200);
        }
    }
}

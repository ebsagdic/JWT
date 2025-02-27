﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Business.Abstracts;
using Entity.ModelsDtos;
using JWT.Core.Abstracts;
using JWT.Core.Model;
using JWT.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace Business.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<CustomUser> _userManager;
        private readonly CustomTokenOption _tokenOption;
        private readonly IGenericRepository<UserRefreshToken> _genericRepository;
        public AuthenticationService(IMapper mapper, UserManager<CustomUser> userManager, IOptions<CustomTokenOption> tokenOption, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> genericRepository)
        {
            this._userManager = userManager;
            this._mapper = mapper;
            this._tokenOption = tokenOption.Value;
            this._unitOfWork = unitOfWork;
            this._genericRepository = genericRepository;
        }

        public async Task<Response<TokenDto>> LoginAsync(UserForLoginDto loginDto)
        {
            var errors = new List<string>();
            if (loginDto == null)
            {

                errors.Add("Kullacı bilgisi bulunamadı");
                return Response<TokenDto>.Fail(404, errors);
            }

            //kullanıcı var mı kontrolü
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                errors.Add("Email adresi veya şifre hatalı");
                return Response<TokenDto>.Fail(400, errors);
            }
            var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
            //kullanıcı adı şifre kontrolü
            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                errors.Add("Email adresi veya şifre hatalı");
                return Response<TokenDto>.Fail(400, errors);
            }

            TokenDto token = CreateToken(user, userRoles);

            var userRefreshToken = await _genericRepository.FindByCondition(x => x.UserId == user.Id).SingleOrDefaultAsync();

            if (userRefreshToken == null)
            {
                await _genericRepository.CreateAsync(new UserRefreshToken { CreateUser = user.Name ,UserId = user.Id, Code = token.RefreshToken, Expiration = token.RefreshTokenExpiration });
            }
            else
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
                userRefreshToken.CreateUser = user.Name;
                _genericRepository.Update(userRefreshToken);
            }

            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(token, 200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
        {
            var existRefreshToken = await _genericRepository.FindByCondition(x => x.Code == refreshToken).SingleOrDefaultAsync();

            if (existRefreshToken == null)
            {
                return Response<TokenDto>.Fail(404, new List<string> { "Refresh Token Bulunamadı" });
            }

            var user = await _userManager.FindByIdAsync(existRefreshToken.UserId.ToString());

            if (user == null)
            {
                return Response<TokenDto>.Fail(404, new List<string> { "Kullanıcı Bulunamadı" });
            }
            var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
            var tokenDto = CreateToken(user, userRoles);

            existRefreshToken.Code = tokenDto.RefreshToken;
            existRefreshToken.Expiration = tokenDto.RefreshTokenExpiration;
            _genericRepository.Update(existRefreshToken);

            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(tokenDto, 200);
        }

        public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
        {
            var existRefreshToken = await _genericRepository.FindByCondition(x => x.Code == refreshToken).SingleOrDefaultAsync();
            if (existRefreshToken == null)
            {
                return Response<NoDataDto>.Fail(404, new List<string> { "Refresh Token Bulunamadı" });
            }

            _genericRepository.Delete(existRefreshToken);

            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(null, 200);
        }

        #region Helper Methots
        private TokenDto CreateToken(CustomUser user, List<string> userRoles)
        {
            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_tokenOption.AccessTokenExpiration);
            var refreshTokenExpiration = DateTime.UtcNow.AddMinutes(_tokenOption.RefreshTokenExpiration);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOption.SecurityKey));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
               issuer: _tokenOption.Issuer,
               audience: _tokenOption.Audience.FirstOrDefault(),
               expires: accessTokenExpiration,
               notBefore: DateTime.UtcNow,
               claims: GetClaims(user, userRoles, _tokenOption.Audience),
               signingCredentials: signingCredentials);

            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            var tokenDto = new TokenDto
            {
                AccessToken = token,
                RefreshToken = CreateRefreshToken(),
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration
            };
            return tokenDto;
        }

        private IEnumerable<Claim> GetClaims(CustomUser user, List<string> userRoles, List<string> audiences)
        {
            var userList = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier,user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name,user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            userList.AddRange(userRoles.Select(x => new Claim(ClaimTypes.Role, x)));
            userList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

            return userList;
        }
        private string CreateRefreshToken()

        {
            var numberByte = new Byte[32];

            using var rnd = RandomNumberGenerator.Create();

            rnd.GetBytes(numberByte);

            return Convert.ToBase64String(numberByte);
        }
        #endregion





    }
}

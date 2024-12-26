using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Business.Abstracts;
using Data.Concretes;
using Entity.ModelsDtos;
using JWT.Core.Abstracts;
using JWT.Core.Model;
using Microsoft.EntityFrameworkCore;

namespace Business.Services
{
    public class UserRefreshTokenService : IUserRefreshTokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<UserRefreshToken> _genericRepository;
        public UserRefreshTokenService(IUnitOfWork unitOfWork, IMapper mapper, IGenericRepository<UserRefreshToken> genericRepository)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._genericRepository = genericRepository;
        }

        public async Task<UserRefreshToken> CreateAsync(UserRefreshToken entity)
        {
            await _genericRepository.CreateAsync(entity);
            await _unitOfWork.CommitAsync();
            return entity;
        }

        public async Task<IEnumerable<UserRefreshToken>> FindByCondition(Expression<Func<UserRefreshToken, bool>> expression)
        {
            var list = await _genericRepository.FindByCondition(expression).ToListAsync();
            return list;
        }

        public async Task<UserRefreshToken> Update(UserRefreshToken entity)
        {
            var _refToken =_genericRepository.GetByIdAsync(entity.Id);
            if (_refToken != null)
            {
                _refToken.Result.Code = entity.Code;
            }
            await _unitOfWork.CommitAsync();

            return entity;
        }

    }
}

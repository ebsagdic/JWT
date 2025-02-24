﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Entity.ModelsDtos;
using JWT.Core.Model;

namespace Business.Abstracts
{
    public interface IUserRefreshTokenService
    {
        Task<UserRefreshToken> CreateAsync(UserRefreshToken entity);
        Task<IEnumerable<UserRefreshToken>> FindByCondition(Expression<Func<UserRefreshToken, bool>> expression);

        Task<UserRefreshToken> Update(UserRefreshToken entity);
    }
}

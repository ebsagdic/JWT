
using JWT.Core.Abstracts;
using JWT.Core.Model;
using JWT.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Concretes
{
    public class UserRefreshTokenRepository : GenericRepository<UserRefreshToken>, IUserRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<UserRefreshToken> _dbSet;
        public UserRefreshTokenRepository(AppDbContext context) : base(context)
        {
            //_context = (_context ?? (FirstDbContext)context);
            _context = context;
            _dbSet = context.Set<UserRefreshToken>();
        }
    }
}

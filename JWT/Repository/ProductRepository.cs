using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JWT.Core.Abstracts;
using JWT.Core.Model;
using JWT.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Concretes
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Product> _dbSet;

        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<Product>();
        }

    }
}

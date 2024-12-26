using System;
using JWT.Core.Abstracts;
using JWT.Core.Model;
using JWT.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Concretes
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Customer> _dbSet;

        public CustomerRepository(AppDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<Customer>();
        }
    }

}

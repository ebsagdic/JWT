using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.Abstracts;
using Data.Concretes;
using Entity.ModelsDtos;
using AutoMapper;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using JWT.Core.Abstracts;
using JWT.Model;
using JWT.Core.Model;

namespace Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Customer> _genericRepository;
        public CustomerService(IUnitOfWork unitOfWorkSecond, IMapper mapper, IGenericRepository<Customer> genericRepository)
        {
            this._unitOfWork = unitOfWorkSecond;
            this._mapper = mapper;
            _genericRepository = genericRepository;
        }

        public async Task<Response<CustomerDto>> CreateAsync(CustomerDto dto)
        {
            Customer entity = _mapper.Map<Customer>(dto);
            entity.CreateUser = dto.LastUpdateUser;
            await _genericRepository.CreateAsync(entity);
            await _unitOfWork.CommitAsync();
            var retModel = _mapper.Map<CustomerDto>(entity);
            return Response<CustomerDto>.Success(retModel, 200);
        }

        public async Task<Response<CustomerDto>> GetByIdAsync(int id)
        {
            var entity = await _genericRepository.GetByIdAsync(id);
            if (entity == null)
            {
                var errors = new List<string>();
                errors.Add("ürün id'si bulunamadı");
                return Response<CustomerDto>.Fail(404, errors);
            }
            var retModel = _mapper.Map<CustomerDto>(entity);
            return Response<CustomerDto>.Success(retModel, 200);
        }


        public async Task<Response<IEnumerable<CustomerDto>>> GetAllAsync()
        {
            var entities = await _genericRepository.GetAllAsync();
            var retModel = _mapper.Map<List<CustomerDto>>(entities);
            return Response<IEnumerable<CustomerDto>>.Success(retModel, 200);
        }

        public Response<IEnumerable<CustomerDto>> FindByCondition(Expression<Func<Customer, bool>> expression)
        {
            var list = _genericRepository.FindByCondition(expression);
            var retModel = _mapper.Map<List<CustomerDto>>(list.ToListAsync());
            return Response<IEnumerable<CustomerDto>>.Success(retModel, 200);
        }


        public async Task<Response<CustomerDto>> Update(CustomerDto dto)
        {
            Customer entity = _mapper.Map<Customer>(dto);
            var isExistEntity = await _genericRepository.GetByIdAsync(entity.Id);
            if (isExistEntity == null)
            {
                var errors = new List<string>();
                errors.Add("Güncellem yapılacak ürün id'si bulunamadı");
                return Response<CustomerDto>.Fail(404, errors);
            }

            isExistEntity.FirstName = dto.FirstName;
            isExistEntity.LastName = dto.LastName;
            isExistEntity.PhoneNumber = dto.PhoneNumber;
            isExistEntity.Email = dto.Email;
            isExistEntity.City = dto.City;
            isExistEntity.LastUpdateUser = dto.LastUpdateUser;
            isExistEntity.LastUpdate = DateTime.Now;
            _genericRepository.Update(isExistEntity);
            await _unitOfWork.CommitAsync();
            var retModel = _mapper.Map<CustomerDto>(entity);
            return Response<CustomerDto>.Success(retModel, 200);
        }


        public async Task<Response<NoDataDto>> Delete(CustomerDto dto)
        {
            Customer entity = _mapper.Map<Customer>(dto);
            var isExistEntity = await _genericRepository.GetByIdAsync(entity.Id);
            if (isExistEntity == null)
            {
                var errors = new List<string>();
                errors.Add("Silinecek ürünün id'si bulunamadı");
                return Response<NoDataDto>.Fail(404, errors);
            }
            _genericRepository.Delete(isExistEntity);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(null, 200);

        }
    }
}

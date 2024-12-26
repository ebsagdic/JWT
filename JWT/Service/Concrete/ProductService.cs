using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.Abstracts;
using Data.Concretes;
using Entity.ModelsDtos;
using AutoMapper;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using JWT.Core.Abstracts;
using JWT.Model;
using JWT.Core.Model;

namespace Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Product> _genericRepository;
        public ProductService(IUnitOfWork unitOfWorkFirst, IMapper mapper, IGenericRepository<Product> genericRepository)
        {
            this._unitOfWork = unitOfWorkFirst;
            this._mapper = mapper;
            _genericRepository = genericRepository;
        }

        public async Task<Response<ProductDto>> CreateAsync(ProductDto dto)
        {
            Product entity = _mapper.Map<Product>(dto);
            entity.CreateUser = dto.LastUpdateUser;
            await _genericRepository.CreateAsync(entity);
            await _unitOfWork.CommitAsync();
            var retModel = _mapper.Map<ProductDto>(entity);
            return Response<ProductDto>.Success(retModel, 200);
        }

        public async Task<Response<ProductDto>> GetByIdAsync(int id)
        {
            var entity = await _genericRepository.GetByIdAsync(id);
            if (entity == null)
            {
                var errors = new List<string>();
                errors.Add("ürün id'si bulunamadı");
                return Response<ProductDto>.Fail(404, errors);
            }
            var retModel = _mapper.Map<ProductDto>(entity);
            return Response<ProductDto>.Success(retModel, 200);
        }


        public async Task<Response<IEnumerable<ProductDto>>> GetAllAsync()
        {
            var entities = await _genericRepository.GetAllAsync();
            var retModel = _mapper.Map<List<ProductDto>>(entities);
            return Response<IEnumerable<ProductDto>>.Success(retModel, 200);
        }

        public Response<IEnumerable<ProductDto>> FindByCondition(Expression<Func<Product, bool>> expression)
        {
            var list = _genericRepository.FindByCondition(expression);
            var retModel = _mapper.Map<List<ProductDto>>(list.ToListAsync());
            return Response<IEnumerable<ProductDto>>.Success(retModel, 200);
        }


        public async Task<Response<ProductDto>> Update(ProductDto dto)
        {
            var isExistEntity = await _genericRepository.GetByIdAsync(dto.Id);
            if (isExistEntity == null)
            {
                var errors = new List<string>();
                errors.Add("Güncellem yapılacak ürün id'si bulunamadı");
                return Response<ProductDto>.Fail(404, errors);
            }

            isExistEntity.Name = dto.Name;
            isExistEntity.Description = dto.Description;
            isExistEntity.Price = dto.Price;
            isExistEntity.LastUpdateUser = dto.LastUpdateUser;
            isExistEntity.LastUpdate = DateTime.Now;
            _genericRepository.Update(isExistEntity);
            await _unitOfWork.CommitAsync();
            var retModel = _mapper.Map<ProductDto>(isExistEntity);
            return Response<ProductDto>.Success(retModel, 200);
        }


        public async Task<Response<NoDataDto>> Delete(ProductDto dto)
        {
            Product entity = _mapper.Map<Product>(dto);
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

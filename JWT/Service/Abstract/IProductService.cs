using JWT.Core.Model;
using JWT.Model;
using System.Linq.Expressions;

namespace Business.Abstracts
{
    public interface IProductService
    {
        Task<Response<ProductDto>> CreateAsync(ProductDto entity);
        Task<Response<ProductDto>> GetByIdAsync(int id);
        Task<Response<IEnumerable<ProductDto>>> GetAllAsync();


        Response<IEnumerable<ProductDto>> FindByCondition(Expression<Func<Product, bool>> expression);

        Task<Response<ProductDto>> Update(ProductDto entity);
        Task<Response<NoDataDto>> Delete(ProductDto entity);
    }
}

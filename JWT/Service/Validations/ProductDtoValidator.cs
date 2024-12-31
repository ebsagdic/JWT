using FluentValidation;
using JWT.Model;

namespace JWT.Service.Validations
{
    public class ProductDtoValidator : AbstractValidator<ProductDto>
    {
        //Fluent Validationsların alternatifi data anotatinslardır.
        //bu FluentValidation kütüphanesinden inherit alır , verilen dto nesnesinin tip kontrolünü merkezden yapabilmede yardımcı olur
        public ProductDtoValidator()
        {
            RuleFor(x => x.Price).InclusiveBetween(1, int.MaxValue).WithMessage("{PropertyName} must be greater 0");
        }
    }

}

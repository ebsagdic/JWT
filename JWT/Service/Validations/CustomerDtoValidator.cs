using FluentValidation;
using JWT.Model;

namespace JWT.Service.Validations
{
    public class CustomerDtoValidator : AbstractValidator<CustomerDto>
    {
        //Fluent Validationsların alternatifi data anotatinslardır.
        //bu FluentValidation kütüphanesinden inherit alır , verilen dto nesnesinin tip kontrolünü merkezden yapabilmede yardımcı olur
        public CustomerDtoValidator()
        {
            RuleFor(x => x.FirstName).NotNull().WithMessage("{PropertyName} is required}");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Telefon numarası boş olamaz.").Matches(@"^\+?\d{10,15}$").WithMessage("Telefon numarası geçerli bir formatta olmalıdır.");
            RuleFor(x => x.City).NotEmpty().WithMessage("Şehir adı boş olamaz.").Matches("^[a-zA-ZçÇğĞıİöÖşŞüÜ\\s]+$").WithMessage("Şehir adı yalnızca harflerden oluşmalıdır.")
            .MaximumLength(50).WithMessage("Şehir adı 50 karakteri geçemez.");

        }
    }

}

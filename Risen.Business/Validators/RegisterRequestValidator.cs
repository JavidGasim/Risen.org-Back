using FluentValidation;
using Risen.Contracts.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email boş ola bilməz.")
                .EmailAddress().WithMessage("Email formatı səhvdir.")
                .MaximumLength(256).WithMessage("Email 256 simvoldan uzun ola bilməz.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifrə boş ola bilməz.")
                .MinimumLength(8).WithMessage("Şifrə ən az 8 simvol olmalıdır.")
                .MaximumLength(64).WithMessage("Şifrə 64 simvoldan uzun ola bilməz.")
                .Matches("[A-Z]").WithMessage("Şifrədə ən az 1 böyük hərf olmalıdır.")
                .Matches("[0-9]").WithMessage("Şifrədə ən az 1 rəqəm olmalıdır.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ad boş ola bilməz.")
                .MaximumLength(64).WithMessage("Ad 64 simvoldan uzun ola bilməz.")
                .Matches(@"^[\p{L}\s\-]+$").WithMessage("Ad yalnız hərf, boşluq və tire içerə bilər.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyad boş ola bilməz.")
                .MaximumLength(64).WithMessage("Soyad 64 simvoldan uzun ola bilməz.")
                .Matches(@"^[\p{L}\s\-]+$").WithMessage("Soyad yalnız hərf, boşluq və tire içerə bilər.");

            RuleFor(x => x.UniversityName)
                .MaximumLength(256).WithMessage("Universitet adı 256 simvoldan uzun ola bilməz.")
                .When(x => !string.IsNullOrEmpty(x.UniversityName));
        }
    }
}

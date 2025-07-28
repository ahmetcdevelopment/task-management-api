using FluentValidation;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;

namespace TaskManagement.API.Helpers.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi gereklidir")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(100).WithMessage("Email adresi 100 karakterden uzun olamaz");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre gereklidir")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Şifre 100 karakterden uzun olamaz");
    }
}

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad gereklidir")
            .MaximumLength(50).WithMessage("Ad 50 karakterden uzun olamaz")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Ad sadece harf ve boşluk içerebilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad gereklidir")
            .MaximumLength(50).WithMessage("Soyad 50 karakterden uzun olamaz")
            .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Soyad sadece harf ve boşluk içerebilir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi gereklidir")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(100).WithMessage("Email adresi 100 karakterden uzun olamaz");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre gereklidir")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Şifre 100 karakterden uzun olamaz")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$")
            .WithMessage("Şifre en az bir küçük harf, bir büyük harf ve bir rakam içermelidir");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Şifre onayı gereklidir")
            .Equal(x => x.Password).WithMessage("Şifreler eşleşmiyor");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Geçerli bir rol seçiniz");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[+]?[0-9\s\-\(\)]+$").WithMessage("Geçerli bir telefon numarası giriniz")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Departman adı 100 karakterden uzun olamaz")
            .When(x => !string.IsNullOrEmpty(x.Department));
    }
}

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut şifre gereklidir");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre gereklidir")
            .MinimumLength(6).WithMessage("Yeni şifre en az 6 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Yeni şifre 100 karakterden uzun olamaz")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$")
            .WithMessage("Yeni şifre en az bir küçük harf, bir büyük harf ve bir rakam içermelidir")
            .NotEqual(x => x.CurrentPassword).WithMessage("Yeni şifre mevcut şifreden farklı olmalıdır");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Yeni şifre onayı gereklidir")
            .Equal(x => x.NewPassword).WithMessage("Yeni şifreler eşleşmiyor");
    }
}

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi gereklidir")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");
    }
}

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi gereklidir")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token gereklidir");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre gereklidir")
            .MinimumLength(6).WithMessage("Yeni şifre en az 6 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Yeni şifre 100 karakterden uzun olamaz")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$")
            .WithMessage("Yeni şifre en az bir küçük harf, bir büyük harf ve bir rakam içermelidir");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Yeni şifre onayı gereklidir")
            .Equal(x => x.NewPassword).WithMessage("Yeni şifreler eşleşmiyor");
    }
}
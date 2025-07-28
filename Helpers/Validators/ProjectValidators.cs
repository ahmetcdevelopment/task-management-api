using FluentValidation;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;

namespace TaskManagement.API.Helpers.Validators;

public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Proje adı gereklidir")
            .MaximumLength(200).WithMessage("Proje adı 200 karakterden uzun olamaz")
            .MinimumLength(3).WithMessage("Proje adı en az 3 karakter olmalıdır");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Proje açıklaması 1000 karakterden uzun olamaz")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ManagerId)
            .NotEmpty().WithMessage("Proje yöneticisi gereklidir")
            .Must(BeValidObjectId).WithMessage("Geçerli bir proje yöneticisi ID'si giriniz");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi gereklidir")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Başlangıç tarihi bugünden önce olamaz");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.TeamMemberIds)
            .Must(x => x == null || x.All(BeValidObjectId))
            .WithMessage("Geçerli takım üyesi ID'leri giriniz")
            .Must(x => x == null || x.Count <= 50)
            .WithMessage("Bir projede en fazla 50 takım üyesi olabilir");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Geçerli bir öncelik seviyesi seçiniz");
    }

    private static bool BeValidObjectId(string id)
    {
        return !string.IsNullOrEmpty(id) && id.Length == 24 && id.All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
    }
}

public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
{
    public UpdateProjectDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Proje adı 200 karakterden uzun olamaz")
            .MinimumLength(3).WithMessage("Proje adı en az 3 karakter olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Proje açıklaması 1000 karakterden uzun olamaz")
            .When(x => !string.IsNullOrEmpty(x.Description));

        //RuleFor(x => x.ManagerId)
        //    .Must(BeValidObjectId).WithMessage("Geçerli bir proje yöneticisi ID'si giriniz")
        //    .When(x => !string.IsNullOrEmpty(x.ManagerId));

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Başlangıç tarihi bugünden önce olamaz")
            .When(x => x.StartDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır")
            .When(x => x.EndDate.HasValue && x.StartDate.HasValue);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Geçerli bir öncelik seviyesi seçiniz")
            .When(x => x.Priority.HasValue);

        //RuleFor(x => x.Status)
        //    .IsInEnum().WithMessage("Geçerli bir proje durumu seçiniz")
        //    .When(x => x.Status.HasValue);
    }

    private static bool BeValidObjectId(string id)
    {
        return !string.IsNullOrEmpty(id) && id.Length == 24 && id.All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
    }
}

public class AddTeamMemberDtoValidator : AbstractValidator<AddTeamMemberDto>
{
    public AddTeamMemberDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si gereklidir")
            .Must(BeValidObjectId).WithMessage("Geçerli bir kullanıcı ID'si giriniz");

        //RuleFor(x => x.Role)
        //    .MaximumLength(50).WithMessage("Rol adı 50 karakterden uzun olamaz")
        //    .When(x => !string.IsNullOrEmpty(x.Role));
    }

    private static bool BeValidObjectId(string id)
    {
        return !string.IsNullOrEmpty(id) && id.Length == 24 && id.All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
    }
}

public class ProjectFilterDtoValidator : AbstractValidator<ProjectFilterDto>
{
    public ProjectFilterDtoValidator()
    {
        //RuleFor(x => x.Name)
        //    .MaximumLength(200).WithMessage("Proje adı filtresi 200 karakterden uzun olamaz")
        //    .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.ManagerId)
            .Must(BeValidObjectId).WithMessage("Geçerli bir proje yöneticisi ID'si giriniz")
            .When(x => !string.IsNullOrEmpty(x.ManagerId));

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Geçerli bir proje durumu seçiniz")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Geçerli bir öncelik seviyesi seçiniz")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.StartDateFrom)
            .LessThanOrEqualTo(x => x.StartDateTo)
            .WithMessage("Başlangıç tarihi 'dan' değeri 'a' değerinden küçük olmalıdır")
            .When(x => x.StartDateFrom.HasValue && x.StartDateTo.HasValue);

        RuleFor(x => x.EndDateFrom)
            .LessThanOrEqualTo(x => x.EndDateTo)
            .WithMessage("Bitiş tarihi 'dan' değeri 'a' değerinden küçük olmalıdır")
            .When(x => x.EndDateFrom.HasValue && x.EndDateTo.HasValue);
    }

    private static bool BeValidObjectId(string id)
    {
        return !string.IsNullOrEmpty(id) && id.Length == 24 && id.All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
    }
}
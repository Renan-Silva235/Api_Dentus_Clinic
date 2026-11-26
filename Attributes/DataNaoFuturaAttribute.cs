using DentusClinic.API.Utils;
using System.ComponentModel.DataAnnotations;

namespace DentusClinic.API.Attributes;

public class DataNaoFuturaAttribute : ValidationAttribute
{
    public DataNaoFuturaAttribute()
    {
        ErrorMessage = "A data de nascimento não pode ser uma data futura.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateOnly data && data > HorarioBrasil.Hoje())
            return new ValidationResult(ErrorMessage);

        return ValidationResult.Success;
    }
}

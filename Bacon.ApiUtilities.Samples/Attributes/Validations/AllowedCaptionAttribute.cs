using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Samples.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Bacon.ApiUtilities.Samples.Attributes.Validations;

/// <summary>
/// 
/// </summary>
/// <param name="maxCaptionTextLength"></param>
public class AllowedCaptionAttribute(int maxCaptionTextLength) : ValidationAttribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        #region Type Casting Validation

        if (value == null)
        {
            return ValidationResult.Success;
        }

        IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();

        if (!(value is IEnumerable<CaptionSet> || value is CaptionSet))
        {
            return modelStateValidation.CreateInvalidModelValidationResult(validationContext, "412014", HttpStatusCode.PreconditionFailed, 412014);
        }

        if (value is not IEnumerable<CaptionSet> captionList)
        {
            captionList = [(CaptionSet)value];
        }

        if (!captionList.Any())
        {
            return ValidationResult.Success;
        }

        #endregion Type Casting Validation

        IEnumerable<string> languageCodeSet = captionList.Select(s => s.LanguageCode!);

        #region Invalid Language Codes

        IEnumerable<string> languages = ["en-US", "fr-CA"];
        IEnumerable<string> invalidLanguageCodes = languageCodeSet.Where(w => !languages.Any(a => w.Equals(a, StringComparison.OrdinalIgnoreCase)));

        if (invalidLanguageCodes.Any())
        {
            return modelStateValidation.CreateInvalidModelValidationResult(validationContext, "412010", HttpStatusCode.PreconditionFailed, 412010, string.Join(", ", invalidLanguageCodes));
        }

        #endregion Invalid Language Codes

        #region Invalid Duplicated Language Codes

        IEnumerable<string> duplicatedLanguageCodes = languageCodeSet.GroupBy(g => g).Where(w => w.Count() > 1).Select(s => s.Key);
        if (duplicatedLanguageCodes.Any())
        {
            return modelStateValidation.CreateInvalidModelValidationResult(validationContext, "412012", HttpStatusCode.PreconditionFailed, 412012, string.Join(", ", duplicatedLanguageCodes));
        }

        #endregion Invalid Duplicated Language Codes

        #region Invalid Caption Text Length

        if (captionList.Any(a => a.Text!.Length > maxCaptionTextLength))
        {
            return modelStateValidation.CreateInvalidModelValidationResult(validationContext, "412011", HttpStatusCode.PreconditionFailed, 412011, maxCaptionTextLength.ToString());
        }

        #endregion Invalid Caption Text Length

        return ValidationResult.Success;
    }
}
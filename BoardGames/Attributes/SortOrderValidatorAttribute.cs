namespace BoardGamesAPI.Attributes
{
    public class SortOrderValidatorAttribute : ValidationAttribute
    {
        public SortOrderValidatorAttribute():base("Value must be one of the {0}") { }

        public string[] AllowedValues { get; set; } = new string[] { "ASC", "DESC" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var strValue = value as string;
            if (!string.IsNullOrEmpty(strValue) && AllowedValues.Contains(strValue))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(FormatErrorMessage(string.Join(", ", AllowedValues)));
        }
    }
}

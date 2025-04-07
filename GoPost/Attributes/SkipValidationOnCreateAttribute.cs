// GoPost/Attributes/SkipValidationOnCreateAttribute.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace GoPost.Attributes
{
    // Custom validation attribute that skips validation during the creation process
    public class SkipValidationOnCreateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            // Return true to skip validation for this property
            return true; // Always valid in this case to prevent validation
        }
    }
}

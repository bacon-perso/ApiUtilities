namespace Bacon.Utilities.Extensions;

/// <summary>
/// String extensions
/// </summary>
public static class StringExtensions
{
    extension(string value)
    {
        /// <summary>
        /// Mask String
        /// </summary>
        /// <param name="maskStart"></param>
        /// <param name="maskEnd"></param>
        /// <param name="maskChar"></param>
        /// <exception cref="ArgumentNullException">The text cannot be null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">The mask start cannot be lower than 0</exception>
        /// <exception cref="ArgumentOutOfRangeException">The mask end cannot be bigger than the text length</exception>
        /// <exception cref="ArgumentOutOfRangeException">The mask character cannot be a white space</exception>
        /// <exception cref="ArgumentOutOfRangeException">The mask start cannot be greater than mask end</exception>
        /// <returns>string</returns>
        public string MaskString(ushort maskStart, ushort maskEnd, char maskChar)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            if (char.IsWhiteSpace(maskChar))
            {
                throw new ArgumentException("The mask character cannot be a white space", nameof(maskChar));
            }

            if (maskStart > maskEnd)
            {
                throw new ArgumentException("The mask start cannot be greater than mask end", nameof(maskStart));
            }

            if ((maskEnd - maskStart) == 0)
            {
                return value;
            }

            if ((maskStart > value.Length))
            {
                return new(maskChar, maskEnd);
            }

            return value[..maskStart].PadRight(maskEnd, maskChar);
        }
    }   
}
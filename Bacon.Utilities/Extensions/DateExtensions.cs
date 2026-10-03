namespace Bacon.Utilities.Extensions;

/// <summary>
/// Date extensions
/// </summary>
public static class DateExtensions
{
    extension(DateTime value)
    {
        /// <summary>
        /// Validates if a date is between 2 dates.
        /// </summary>
        /// <param name="dateFrom">The lower date</param>
        /// <param name="dateTo">The greater date</param>
        /// <returns>true if the date to validate is between lower and greater date. Otherwise false. If both dateFrom and dateTo are null, the function will return true.</returns>
        public bool IsBetweenDates(DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom == null && dateTo == null)
            {
                return true;
            }

            return (value >= (dateFrom ?? DateTime.MinValue) && value <= (dateTo ?? DateTime.MaxValue));
        }
    }
}
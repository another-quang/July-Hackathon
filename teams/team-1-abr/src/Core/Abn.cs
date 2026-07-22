using System.Text.RegularExpressions;

namespace Team1Abr.Core;

/// <summary>
/// ABN (Australian Business Number) validation.
///
/// Pure logic — no UI, no network. Unit-tested in tests/AbnTests.cs.
///
/// An ABN is 11 digits and is validated with the ATO checksum algorithm:
///   1. Subtract 1 from the FIRST (leftmost) digit.
///   2. Multiply each of the 11 digits by its weight: [10,1,3,5,7,9,11,13,15,17,19].
///   3. Sum the results.
///   4. The ABN is valid if the sum is divisible by 89.
///
/// Security note: input is treated as untrusted. We validate type (null),
/// strip only formatting whitespace, and reject anything that is not exactly
/// 11 digits BEFORE running the checksum.
/// </summary>
public static class Abn
{
    /// <summary>Positional weights defined by the ATO ABN checksum algorithm.</summary>
    private static readonly int[] Weights = [10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19];

    private static readonly Regex WhitespacePattern = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex DigitsOnlyPattern = new(@"^\d+$", RegexOptions.Compiled);

    /// <summary>
    /// Remove spaces from a candidate ABN so "51 824 753 556" and "51824753556"
    /// are treated the same. Does not remove other characters — non-digits are
    /// rejected by validation so we surface a clear error instead of silently
    /// "fixing" bad input.
    /// </summary>
    public static string Normalise(string? input) =>
        input is null ? string.Empty : WhitespacePattern.Replace(input, string.Empty);

    /// <summary>
    /// Validate an ABN string. Returns a structured result rather than throwing,
    /// so the UI can show an accessible, specific error message.
    /// </summary>
    public static AbnValidationResult Validate(string? input)
    {
        // Boundary validation: never trust the input. A null value is the C#
        // equivalent of the "must be text" guard.
        if (input is null)
        {
            return new AbnValidationResult(false, "ABN must be text.", string.Empty);
        }

        var normalised = Normalise(input);

        if (normalised.Length == 0)
        {
            return new AbnValidationResult(false, "Enter an ABN.", string.Empty);
        }

        if (!DigitsOnlyPattern.IsMatch(normalised))
        {
            return new AbnValidationResult(false, "An ABN must contain digits only.", normalised);
        }

        if (normalised.Length != 11)
        {
            return new AbnValidationResult(false, "An ABN must be exactly 11 digits.", normalised);
        }

        var sum = 0;
        for (var index = 0; index < normalised.Length; index++)
        {
            // Step 1: subtract 1 from the first digit.
            var value = normalised[index] - '0';
            if (index == 0)
            {
                value -= 1;
            }

            sum += value * Weights[index];
        }

        if (sum % 89 != 0)
        {
            return new AbnValidationResult(
                false,
                "That ABN failed the checksum. Please double-check the number.",
                normalised);
        }

        return new AbnValidationResult(true, string.Empty, normalised);
    }

    /// <summary>Format an 11-digit ABN for display as "NN NNN NNN NNN".</summary>
    public static string Format(string normalised) =>
        normalised.Length != 11
            ? normalised
            : $"{normalised[..2]} {normalised[2..5]} {normalised[5..8]} {normalised[8..11]}";
}


/// <summary>Result of validating an ABN.</summary>
/// <param name="Valid">Whether the ABN passed validation.</param>
/// <param name="Reason">A short, user-safe reason when invalid. Empty string when valid.</param>
/// <param name="Normalised">The normalised 11-digit ABN (spaces removed) when the format is right.</param>
public sealed record AbnValidationResult(bool Valid, string Reason, string Normalised);

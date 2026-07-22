namespace Team1Abr.Core;

/// <summary>
/// Composes a short, plain-language summary of a business record.
///
/// Offline and deterministic: the text is generated from the record's own fields —
/// no network calls, no live model. This honours the hackathon rule to use fixtures
/// only and keeps the output unit-testable. The UI labels it as automatically
/// generated so its provenance is clear to the reader.
/// </summary>
public static class Summary
{
    public static string Generate(BusinessRecord record)
    {
        var statusClause = string.Equals(record.AbnStatus, "Active", StringComparison.OrdinalIgnoreCase)
            ? "Its ABN is currently active"
            : "Its ABN has been cancelled, so it may no longer be trading";

        var gstClause = record.GstRegistered
            ? " and it is registered for GST."
            : ". It is not registered for GST.";

        return $"{record.EntityName} is a {record.EntityType} based in {record.State} " +
               $"(postcode {record.Postcode}). {statusClause}{gstClause}";
    }
}

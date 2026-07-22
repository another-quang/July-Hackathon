using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Team1Abr.Core;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Team1Abr.Web.Components;

public partial class AbnLookup
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private string _abnInput = string.Empty;
    private LookupStatus _state = LookupStatus.Idle;
    private string _errorMessage = string.Empty;
    private string _notFoundAbn = string.Empty;
    private BusinessRecord? _record;
    private readonly List<string> _recentSearches = new();
    private string _copyMessage = string.Empty;
    private bool _hasSearched;
    private bool _isLoading;

    private bool HasError => _state == LookupStatus.Error;

    private void OnInput(ChangeEventArgs args) => _abnInput = args.Value?.ToString() ?? string.Empty;

    private async Task HandleSubmit()
    {
        ResetMessages();
        _hasSearched = true;
        _isLoading = true;
        _state = LookupStatus.Idle;
        _errorMessage = string.Empty;
        _notFoundAbn = string.Empty;
        _record = null;

        // Render the loading state first so assistive technologies can announce it.
        await Task.Yield();

        var result = Lookup.SearchAbn(_abnInput);
        _isLoading = false;

        if (result.Status == LookupStatus.Error)
        {
            _state = LookupStatus.Error;
            _errorMessage = result.Reason;
            _record = null;
            _notFoundAbn = string.Empty;
            return;
        }

        AddRecentSearch(result.NormalisedAbn);

        if (result.Status == LookupStatus.NotFound)
        {
            _state = LookupStatus.NotFound;
            _notFoundAbn = Abn.Format(result.NormalisedAbn);
            _record = null;
            return;
        }

        _record = result.Record;
        _state = LookupStatus.Found;
        _notFoundAbn = string.Empty;
    }

    private void AddRecentSearch(string normalisedAbn)
    {
        Lookup.AddRecentSearch(_recentSearches, normalisedAbn);
    }

    private void ClearRecentSearches()
    {
        Lookup.ClearRecentSearches(_recentSearches);
        _copyMessage = string.Empty;
        _errorMessage = string.Empty;
        _notFoundAbn = string.Empty;
        _record = null;
        _state = LookupStatus.Idle;
        _abnInput = string.Empty;
    }

    private async Task CopyAbnToClipboardAsync()
    {
        if (_record is null)
        {
            return;
        }

        try
        {
            await JS.InvokeAsync<object>("navigator.clipboard.writeText", _record.Abn);
            _copyMessage = $"Copied {_record.Abn} to clipboard.";
        }
        catch
        {
            _copyMessage = "Copy failed. Use your browser or device controls to copy the ABN.";
        }
    }

    private void ResetMessages()
    {
        _copyMessage = string.Empty;
        _errorMessage = string.Empty;
    }

    private string GetRecentSearchAriaLabel(string normalisedAbn) =>
        $"Search for ABN {Abn.Format(normalisedAbn)}";

    private void HandleRecentSearch(string normalisedAbn)
    {
        ResetMessages();
        _hasSearched = true;
        _isLoading = false;
        _abnInput = Abn.Format(normalisedAbn);
        var record = Lookup.LookupAbn(normalisedAbn);
        if (record is null)
        {
            _state = LookupStatus.NotFound;
            _notFoundAbn = Abn.Format(normalisedAbn);
            _record = null;
            return;
        }

        _record = record;
        _state = LookupStatus.Found;
        _notFoundAbn = string.Empty;
    }
}

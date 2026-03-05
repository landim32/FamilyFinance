using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyFinance.DTOs;
using FamilyFinance.Services;

namespace FamilyFinance.ViewModels;

public partial class ExportViewModel : ObservableObject
{
    private readonly IPersonRepository _personRepo;
    private readonly IMigrationService _migration;
    private readonly IMapper _mapper;

    public ExportViewModel(IPersonRepository personRepo, IMigrationService migration, IMapper mapper)
    {
        _personRepo = personRepo;
        _migration = migration;
        _mapper = mapper;
    }

    [ObservableProperty]
    private ObservableCollection<PersonExportDisplay> people = new();

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isStatusVisible;

    [RelayCommand]
    public async Task LoadPeopleAsync()
    {
        var list = await _personRepo.GetAllAsync();
        var displayList = new List<PersonExportDisplay>();

        foreach (var p in list)
        {
            var count = await _personRepo.GetAccountCountAsync(p.Id);
            displayList.Add(new PersonExportDisplay
            {
                PersonId = p.Id,
                PersonName = p.Name,
                AccountCount = count
            });
        }

        People = new ObservableCollection<PersonExportDisplay>(displayList);
    }

    [RelayCommand]
    public async Task ExportPersonAsync(PersonExportDisplay person)
    {
        try
        {
            var path = await _migration.ExportJsonForPersonAsync(person.PersonId);
            await ShareFileAsync(path);
            await ShowStatusAsync($"Exported: {person.PersonName}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Export failed: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task ExportAllAsync()
    {
        try
        {
            var paths = await _migration.ExportAllAsync();

            if (paths.Count == 0)
            {
                await Shell.Current.DisplayAlert("Info", "No people to export.", "OK");
                return;
            }

            var shareFiles = paths.Select(p => new ShareFile(p)).ToList();
            await Share.Default.RequestAsync(new ShareMultipleFilesRequest
            {
                Title = "Export All Migration Files",
                Files = shareFiles
            });

            await ShowStatusAsync($"{paths.Count} file(s) generated successfully!");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Export failed: {ex.Message}", "OK");
        }
    }

    private async Task ShareFileAsync(string filePath)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Share Migration File",
            File = new ShareFile(filePath)
        });
    }

    private async Task ShowStatusAsync(string message)
    {
        StatusMessage = message;
        IsStatusVisible = true;

        await Task.Delay(4000);

        IsStatusVisible = false;
        StatusMessage = null;
    }
}

public class PersonExportDisplay
{
    public int PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public int AccountCount { get; set; }
}

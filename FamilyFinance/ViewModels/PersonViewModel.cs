using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyFinance.DTOs;
using FamilyFinance.Models;
using FamilyFinance.Services;
using FamilyFinance.Views;

namespace FamilyFinance.ViewModels;

public partial class PersonViewModel : ObservableObject
{
    private readonly IPersonRepository _personRepo;
    private readonly IMapper _mapper;

    public PersonViewModel(IPersonRepository personRepo, IMapper mapper)
    {
        _personRepo = personRepo;
        _mapper = mapper;
    }

    [ObservableProperty]
    private ObservableCollection<PersonDisplayInfo> people = new();

    // ---- Form properties for PersonFormPage ----

    [ObservableProperty]
    private PersonInfo? editingPerson;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? phone;

    [ObservableProperty]
    private string? email;

    [ObservableProperty]
    private string? photoBase64;

    [ObservableProperty]
    private ImageSource? photoImageSource;

    [RelayCommand]
    public async Task LoadPeopleAsync()
    {
        var list = await _personRepo.GetAllAsync();
        var displayList = new List<PersonDisplayInfo>();

        foreach (var p in list)
        {
            var count = await _personRepo.GetAccountCountAsync(p.Id);
            var info = _mapper.Map<PersonInfo>(p);
            displayList.Add(new PersonDisplayInfo
            {
                Person = info,
                AccountCount = count
            });
        }

        People = new ObservableCollection<PersonDisplayInfo>(displayList);
    }

    [RelayCommand]
    public async Task DeletePersonAsync(PersonDisplayInfo personDisplay)
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete {personDisplay.Person.Name}? Linked accounts will have their person cleared.",
            "Delete", "Cancel");

        if (!confirm)
            return;

        await _personRepo.DeleteAsync(personDisplay.Person.Id);
        await LoadPeopleAsync();
    }

    [RelayCommand]
    public async Task GoToAddPersonAsync()
    {
        await Shell.Current.GoToAsync(nameof(PersonFormPage));
    }

    [RelayCommand]
    public async Task GoToEditPersonAsync(PersonDisplayInfo personDisplay)
    {
        await Shell.Current.GoToAsync(nameof(PersonFormPage), new Dictionary<string, object>
        {
            { "PersonInfo", personDisplay.Person }
        });
    }

    public void SetEditingPerson(PersonInfo person)
    {
        EditingPerson = person;
    }

    [RelayCommand]
    public void LoadFormData()
    {
        if (EditingPerson is not null)
        {
            Name = EditingPerson.Name;
            Phone = EditingPerson.Phone;
            Email = EditingPerson.Email;
            PhotoBase64 = EditingPerson.PhotoBase64;
            UpdatePhotoImageSource();
        }
    }

    [RelayCommand]
    public async Task PickPhotoAsync()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select a profile photo"
            });

            if (result is not null)
                await ProcessPhotoAsync(result);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Unable to pick photo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task TakePhotoAsync()
    {
        try
        {
            var result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take a profile photo"
            });

            if (result is not null)
                await ProcessPhotoAsync(result);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Unable to take photo: {ex.Message}", "OK");
        }
    }

    private async Task ProcessPhotoAsync(FileResult result)
    {
        using var stream = await result.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        PhotoBase64 = Convert.ToBase64String(memoryStream.ToArray());
        UpdatePhotoImageSource();
    }

    private void UpdatePhotoImageSource()
    {
        if (!string.IsNullOrWhiteSpace(PhotoBase64))
        {
            try
            {
                var bytes = Convert.FromBase64String(PhotoBase64);
                PhotoImageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch
            {
                PhotoImageSource = ImageSource.FromFile("person_placeholder.png");
            }
        }
        else
        {
            PhotoImageSource = ImageSource.FromFile("person_placeholder.png");
        }
    }

    [RelayCommand]
    public async Task SavePersonAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlert("Validation", "Name is required.", "OK");
            return;
        }

        try
        {
            if (EditingPerson is not null)
            {
                var entity = await _personRepo.GetByIdAsync(EditingPerson.Id);
                if (entity is null) return;
                entity.Update(Name.Trim(), Phone, Email, PhotoBase64);
                var error = entity.Validate();
                if (error != null) { await Shell.Current.DisplayAlert("Error", error, "OK"); return; }
                await _personRepo.SaveAsync(entity);
            }
            else
            {
                var entity = Person.Create(Name.Trim(), Phone, Email, PhotoBase64);
                await _personRepo.SaveAsync(entity);
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}

public class PersonDisplayInfo
{
    public PersonInfo Person { get; set; } = new();
    public int AccountCount { get; set; }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyFinance.Models;
using FamilyFinance.Services;
using FamilyFinance.Views;

namespace FamilyFinance.ViewModels;

public partial class PersonViewModel : ObservableObject
{
    private readonly DatabaseService _db;

    public PersonViewModel(DatabaseService db)
    {
        _db = db;
    }

    [ObservableProperty]
    private ObservableCollection<PersonDisplay> people = new();

    // ---- Form properties for PersonFormPage ----

    [ObservableProperty]
    private Person? editingPerson;

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
        var list = await _db.GetPeopleAsync();
        var displayList = new List<PersonDisplay>();

        foreach (var p in list)
        {
            var count = await _db.GetAccountCountByPersonAsync(p.Id);
            displayList.Add(new PersonDisplay
            {
                Person = p,
                AccountCount = count
            });
        }

        People = new ObservableCollection<PersonDisplay>(displayList);
    }

    [RelayCommand]
    public async Task DeletePersonAsync(PersonDisplay personDisplay)
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete {personDisplay.Person.Name}? Linked accounts will have their person cleared.",
            "Delete", "Cancel");

        if (!confirm)
            return;

        await _db.DeletePersonAsync(personDisplay.Person);
        await LoadPeopleAsync();
    }

    [RelayCommand]
    public async Task GoToAddPersonAsync()
    {
        await Shell.Current.GoToAsync(nameof(PersonFormPage));
    }

    [RelayCommand]
    public async Task GoToEditPersonAsync(PersonDisplay personDisplay)
    {
        await Shell.Current.GoToAsync(nameof(PersonFormPage), new Dictionary<string, object>
        {
            { "Person", personDisplay.Person }
        });
    }

    public void SetEditingPerson(Person person)
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

        var person = EditingPerson ?? new Person();
        person.Name = Name.Trim();
        person.Phone = Phone;
        person.Email = Email;
        person.PhotoBase64 = PhotoBase64;

        await _db.SavePersonAsync(person);
        await Shell.Current.GoToAsync("..");
    }
}

public class PersonDisplay
{
    public Person Person { get; set; } = new();
    public int AccountCount { get; set; }
}

using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopClient.Navigation;
using DesktopClient.Services;
using ElectroDepotClassLibrary.Stores;
using ElectroDepotClassLibrary.Utility;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using ElectroDepotClassLibrary.Models;
using System.Text.RegularExpressions;
using MsBox.Avalonia.Enums;
using System.Linq;
using System.IO;

namespace DesktopClient.ViewModels
{
    public partial class AIAssistantPageViewModel : RootNavigatorViewModel
    {
        [ObservableProperty]
        private string _productUrl;
        
        [ObservableProperty]
        private string _apiKey;
        
        [ObservableProperty]
        private string _aiAnalysisResult;

        [ObservableProperty]
        private bool _isProcessing;

        [ObservableProperty]
        private int _initialQuantity = 1;

        private Component _parsedComponent;

        public AIAssistantPageViewModel(RootPageViewModel defaultRootPageViewModel, DatabaseStore databaseStore, MessageBoxService messageBoxService, ApplicationConfig appConfig) : base(defaultRootPageViewModel, databaseStore, messageBoxService, appConfig)
        {
            AiAnalysisResult = "Wklej link do produktu i kliknij 'Przetwórz'. Twoje API Key dla Gemini jest wymagane.";
            ApiKey = ""; 
            LoadApiKey();
        }

        private string GetApiKeyPath()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElectroDepot");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "gemini_api.key");
        }

        private void LoadApiKey()
        {
            var path = GetApiKeyPath();
            if (File.Exists(path))
            {
                ApiKey = File.ReadAllText(path).Trim();
            }
        }

        [RelayCommand]
        public async Task SaveApiKey()
        {
            try
            {
                var path = GetApiKeyPath();
                File.WriteAllText(path, ApiKey ?? "");
                await MsBoxService.DisplayMessageBox("Klucz API został pomyślnie zapisany.", Icon.Success);
            }
            catch(Exception ex)
            {
                await MsBoxService.DisplayMessageBox("Błąd podczas zapisywania klucza: " + ex.Message, Icon.Error);
            }
        }

        [RelayCommand]
        public async Task ProcessLink()
        {
            if (string.IsNullOrWhiteSpace(ProductUrl) || string.IsNullOrWhiteSpace(ApiKey))
            {
                await MsBoxService.DisplayMessageBox("Please provide both URL and API Key.", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            IsProcessing = true;
            AiAnalysisResult = "Pobieranie zawartości...";

            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                string html = await client.GetStringAsync(ProductUrl);
                
                AiAnalysisResult = "Przetwarzanie z Gemini...";

                var existingCategories = string.Join(", ", DatabaseStore.CategorieStore.Categories.Select(c => c.Name));
                var prompt = $"Jesteś asystentem AI. Poniżej znajduje się kod HTML/tekst strony produktu elektronicznego. Zidentyfikuj i wyciągnij: Nazwę (Name), Krótki opis (ShortDescription), Producenta (Manufacturer), kategorię (CategoryName) oraz główny obrazek produktu (ImageUrl - bezwzględny adres URL, musi zaczynać się od http. BEZWZGLĘDNY ZAKAZ: nie wybieraj logo sklepu, banerów reklamowych ani nawigacji po stronie. Szukaj zdjęcia stricte tego konkretnego produktu, najlepiej znajdującego się w meta og:image, twitter:image lub w głównym slajderze na stronie). Istniejące kategorie to: {existingCategories}. Zwróć jedną z nich, lub jeśli żadna nie pasuje, zaproponuj nową, krótką nazwę kategorii. Zwróć TYLKO czysty obiekt JSON bez formatowania markdown. Właściwości JSON: Name, ShortDescription, Manufacturer, CategoryName, ImageUrl. Tekst strony: " + (html.Length > 50000 ? html.Substring(0, 50000) : html);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = prompt } }
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite-preview:generateContent?key={ApiKey}", content);

                if (!response.IsSuccessStatusCode)
                {
                   AiAnalysisResult = $"Błąd Gemini: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
                   IsProcessing = false;
                   return;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(jsonString);
                var aiText = geminiResponse.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                var cleanJson = aiText.Trim();
                if (cleanJson.StartsWith("```json")) cleanJson = cleanJson.Substring(7);
                if (cleanJson.EndsWith("```")) cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);

                var generatedData = JsonSerializer.Deserialize<JsonElement>(cleanJson);
                
                string parsedName = generatedData.TryGetProperty("Name", out var n) ? n.GetString() : "Nieznany";
                string parsedDesc = generatedData.TryGetProperty("ShortDescription", out var d) ? d.GetString() : "";
                string parsedManuf = generatedData.TryGetProperty("Manufacturer", out var m) ? m.GetString() : "Nieznany";
                string parsedCatName = generatedData.TryGetProperty("CategoryName", out var c) ? c.GetString() : "Inne";
                string parsedImageUrl = generatedData.TryGetProperty("ImageUrl", out var img) ? img.GetString() : "";
                
                Category targetCategory = DatabaseStore.CategorieStore.Categories.FirstOrDefault(x => x.Name.Equals(parsedCatName, StringComparison.OrdinalIgnoreCase));
                
                if (targetCategory == null)
                {
                    AiAnalysisResult = $"Tworzenie nowej kategorii: {parsedCatName}...";
                    var newCategory = new Category(0, parsedCatName, "Kategoria wygenerowana przez AI", Array.Empty<byte>());
                    bool success = await DatabaseStore.CategorieStore.DB.CreateCategory(newCategory);
                    if (success)
                    {
                        await DatabaseStore.CategorieStore.ReloadCategoriesData();
                        targetCategory = DatabaseStore.CategorieStore.Categories.FirstOrDefault(x => x.Name.Equals(parsedCatName, StringComparison.OrdinalIgnoreCase));
                    }
                }
                
                targetCategory ??= DatabaseStore.CategorieStore.Categories.FirstOrDefault();
                
                byte[] imageBytes = null;
                string imageStatus = "Brak (użyto domyślnego)";

                if (!string.IsNullOrWhiteSpace(parsedImageUrl))
                {
                    try
                    {
                        if (!parsedImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            Uri baseUri = new Uri(ProductUrl);
                            Uri absoluteUri = new Uri(baseUri, parsedImageUrl);
                            parsedImageUrl = absoluteUri.ToString();
                        }
                        
                        imageBytes = await client.GetByteArrayAsync(parsedImageUrl);
                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            imageStatus = "Pobrano";
                        }
                    }
                    catch
                    {
                        imageStatus = "Błąd pobierania (użyto domyślnego)";
                        imageBytes = null;
                    }
                }

                if (imageBytes == null || imageBytes.Length == 0)
                {
                    var defaultBitmap = DesktopClient.ImageHelper.LoadFromResource(new Uri("avares://ElectroDepot/Assets/NoImage.png"));
                    imageBytes = defaultBitmap != null ? ElectroDepotClassLibrary.Utility.ImageConverterUtility.BitmapToBytes(defaultBitmap) : new byte[] { };
                }
                
                string categoryStatus = targetCategory?.Name == parsedCatName ? targetCategory?.Name : $"{targetCategory?.Name} (zamiast {parsedCatName})";
                
                AiAnalysisResult = $"Wykryty komponent: {parsedName}\nProducent: {parsedManuf}\nKategoria: {categoryStatus}\nZdjęcie: {imageStatus}\nOpis: {parsedDesc}\n\nGotowy do dodania do magazynu.";
                
                _parsedComponent = new Component(0, targetCategory?.ID ?? 1, targetCategory, parsedName, parsedManuf, parsedDesc, "", "", imageBytes);
            }
            catch (Exception ex)
            {
                AiAnalysisResult = "Błąd: " + ex.Message;
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        public async Task AddToInventory()
        {
            if (_parsedComponent == null)
            {
                await MsBoxService.DisplayMessageBox("Musisz najpierw przetworzyć produkt.", Icon.Warning);
                return;
            }

            IsProcessing = true;
            try
            {
                bool success = await DatabaseStore.ComponentStore.InsertNewComponent(_parsedComponent, InitialQuantity);
                if (success)
                {
                    await MsBoxService.DisplayMessageBox($"Pomyślnie dodano do magazynu. Ilość dostępna: {InitialQuantity}", Icon.Success);
                    AiAnalysisResult = "Gotowe! Dodano do magazynu.";
                    _parsedComponent = null;
                    ProductUrl = "";
                    InitialQuantity = 1;
                }
                else
                {
                    await MsBoxService.DisplayMessageBox("Nie udało się dodać produktu do bazy.", Icon.Error);
                }
            }
            catch (Exception ex)
            {
                await MsBoxService.DisplayMessageBox("Błąd: " + ex.Message, Icon.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
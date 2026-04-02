using System.Net.Http.Json;
using ElectroDepotClassLibrary.DTOs;
using Microsoft.AspNetCore.Identity;

namespace WebClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // User API methods
        public async Task<UserSession?> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _httpClient.GetFromJsonAsync<UserDTO>(
                    $"/ElectroDepot/Users/GetUserByEMail/{Uri.EscapeDataString(email)}");
                if (user == null) return null;

                var hasher = new PasswordHasher<UserDTO>();
                var result = hasher.VerifyHashedPassword(user, user.Password, password);
                if (result != PasswordVerificationResult.Success) return null;

                return new UserSession(user.ID, user.Username, user.Email, user.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return null;
            }
        }

        public async Task<UserSession?> RegisterAsync(string username, string email, string password, string name)
        {
            var tempDto = new CreateUserDTO(Username: username, Email: email, Password: password, Name: name);
            var hasher = new PasswordHasher<CreateUserDTO>();
            string hashedPassword = hasher.HashPassword(tempDto, password);
            var dtoWithHash = tempDto with { Password = hashedPassword };

            var response = await _httpClient.PostAsJsonAsync("/ElectroDepot/Users/Create", dtoWithHash);
            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<UserDTO>();
                if (created == null) return null;
                return new UserSession(created.ID, created.Username, created.Email, created.Name);
            }
            return null;
        }

        // Component API methods
        public async Task<List<ComponentDTO>> GetComponentsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ComponentDTO>>("/ElectroDepot/Components/GetAll") ?? new List<ComponentDTO>();
        }

        public async Task<ComponentDTO?> GetComponentByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ComponentDTO>($"/ElectroDepot/Components/GetComponentByID/{id}");
        }

        public async Task<ComponentDTO?> CreateComponentAsync(CreateComponentDTO component)
        {
            var response = await _httpClient.PostAsJsonAsync("/ElectroDepot/Components/Create", component);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ComponentDTO>();
            }
            return null;
        }

        public async Task<bool> UpdateComponentAsync(int id, UpdateComponentDTO component)
        {
            var response = await _httpClient.PutAsJsonAsync($"/ElectroDepot/Components/Update/{id}", component);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteComponentAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/ElectroDepot/Components/Delete/{id}");
            return response.IsSuccessStatusCode;
        }

        // Project API methods
        public async Task<List<ProjectDTO>> GetProjectsAsync(int? userId = null)
        {
            if (userId.HasValue)
            {
                return await _httpClient.GetFromJsonAsync<List<ProjectDTO>>($"/ElectroDepot/Projects/GetAllOfUser/{userId}") ?? new List<ProjectDTO>();
            }
            return await _httpClient.GetFromJsonAsync<List<ProjectDTO>>("/ElectroDepot/Projects/GetAll") ?? new List<ProjectDTO>();
        }

        public async Task<ProjectDTO?> GetProjectByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ProjectDTO>($"/ElectroDepot/Projects/GetByID/{id}");
        }

        public async Task<ProjectDTO?> CreateProjectAsync(CreateProjectDTO project)
        {
            var response = await _httpClient.PostAsJsonAsync("/ElectroDepot/Projects/Create", project);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProjectDTO>();
            }
            return null;
        }

        public async Task<bool> UpdateProjectAsync(int id, UpdateProjectDTO project)
        {
            var response = await _httpClient.PutAsJsonAsync($"/ElectroDepot/Projects/Update/{id}", project);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/ElectroDepot/Projects/Delete/{id}");
            return response.IsSuccessStatusCode;
        }

        // Purchase API methods
        public async Task<List<PurchaseDTO>> GetPurchasesAsync(int? userId = null)
        {
            if (userId.HasValue)
            {
                return await _httpClient.GetFromJsonAsync<List<PurchaseDTO>>($"/ElectroDepot/Purchases/GetAllByUserID/{userId}") ?? new List<PurchaseDTO>();
            }
            return await _httpClient.GetFromJsonAsync<List<PurchaseDTO>>("/ElectroDepot/Purchases/GetAll") ?? new List<PurchaseDTO>();
        }

        public async Task<PurchaseDTO?> GetPurchaseByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<PurchaseDTO>($"/ElectroDepot/Purchases/GetByID/{id}");
        }

        public async Task<PurchaseDTO?> CreatePurchaseAsync(CreatePurchaseDTO purchase)
        {
            var response = await _httpClient.PostAsJsonAsync("/ElectroDepot/Purchases/Create", purchase);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PurchaseDTO>();
            }
            return null;
        }

        public async Task<bool> UpdatePurchaseAsync(int id, UpdatePurchaseDTO purchase)
        {
            var response = await _httpClient.PutAsJsonAsync($"/ElectroDepot/Purchases/Update/{id}", purchase);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePurchaseAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/ElectroDepot/Purchases/Delete/{id}");
            return response.IsSuccessStatusCode;
        }

        // Supplier API methods
        public async Task<List<SupplierDTO>> GetSuppliersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<SupplierDTO>>("/ElectroDepot/Suppliers/GetAll") ?? new List<SupplierDTO>();
        }

        public async Task<SupplierDTO?> GetSupplierByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<SupplierDTO>($"/ElectroDepot/Suppliers/GetByID/{id}");
        }

        // Category API methods
        public async Task<List<CategoryDTO>> GetCategoriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<CategoryDTO>>("/ElectroDepot/Categories/GetAll") ?? new List<CategoryDTO>();
        }
    }
}

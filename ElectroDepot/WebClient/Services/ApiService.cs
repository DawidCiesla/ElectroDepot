using System.Net.Http.Json;
using ElectroDepotClassLibrary.DTOs;
using ElectroDepotClassLibrary.Models;

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
        public async Task<UserDTO?> LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/User/login", new { email, password });
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDTO>();
            }
            return null;
        }

        public async Task<UserDTO?> RegisterAsync(string name, string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/User/register", new { name, email, password });
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDTO>();
            }
            return null;
        }

        // Component API methods
        public async Task<List<ComponentDTO>> GetComponentsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ComponentDTO>>("/api/Component") ?? new List<ComponentDTO>();
        }

        public async Task<ComponentDTO?> GetComponentByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ComponentDTO>($"/api/Component/{id}");
        }

        public async Task<ComponentDTO?> CreateComponentAsync(ComponentDTO component)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Component", component);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ComponentDTO>();
            }
            return null;
        }

        public async Task<bool> UpdateComponentAsync(int id, ComponentDTO component)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Component/{id}", component);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteComponentAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/Component/{id}");
            return response.IsSuccessStatusCode;
        }

        // Project API methods
        public async Task<List<ProjectDTO>> GetProjectsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ProjectDTO>>("/api/Project") ?? new List<ProjectDTO>();
        }

        public async Task<ProjectDTO?> GetProjectByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ProjectDTO>($"/api/Project/{id}");
        }

        public async Task<ProjectDTO?> CreateProjectAsync(ProjectDTO project)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Project", project);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProjectDTO>();
            }
            return null;
        }

        public async Task<bool> UpdateProjectAsync(int id, ProjectDTO project)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Project/{id}", project);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/Project/{id}");
            return response.IsSuccessStatusCode;
        }

        // Purchase API methods
        public async Task<List<PurchaseDTO>> GetPurchasesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<PurchaseDTO>>("/api/Purchase") ?? new List<PurchaseDTO>();
        }

        public async Task<PurchaseDTO?> GetPurchaseByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<PurchaseDTO>($"/api/Purchase/{id}");
        }

        public async Task<PurchaseDTO?> CreatePurchaseAsync(PurchaseDTO purchase)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Purchase", purchase);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PurchaseDTO>();
            }
            return null;
        }

        public async Task<bool> UpdatePurchaseAsync(int id, PurchaseDTO purchase)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Purchase/{id}", purchase);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePurchaseAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/Purchase/{id}");
            return response.IsSuccessStatusCode;
        }

        // Supplier API methods
        public async Task<List<SupplierDTO>> GetSuppliersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<SupplierDTO>>("/api/Supplier") ?? new List<SupplierDTO>();
        }

        public async Task<SupplierDTO?> GetSupplierByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<SupplierDTO>($"/api/Supplier/{id}");
        }

        // Category API methods
        public async Task<List<CategoryDTO>> GetCategoriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<CategoryDTO>>("/api/Category") ?? new List<CategoryDTO>();
        }
    }
}

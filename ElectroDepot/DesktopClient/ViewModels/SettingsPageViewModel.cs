using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopClient.Services;
using DesktopClient.Utils;
using ElectroDepotClassLibrary.Stores;
using ElectroDepotClassLibrary.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.ViewModels
{
    public partial class SettingsPageViewModel : RootNavigatorViewModel
    {
        private ApplicationConfig applicationConfig;
        public SettingsPageViewModel(RootPageViewModel defaultPageViewModel, DatabaseStore databaseStore, MessageBoxService msgBoxService, ApplicationConfig appConfig) : base(defaultPageViewModel, databaseStore, msgBoxService, appConfig)
        {
            applicationConfig = appConfig;
            IP = applicationConfig.ServerConfig.ConnectionURL;
        }

        [ObservableProperty]
        private string _iP;

        [RelayCommand]
        public void CopyIP()
        {
            ClipboardManager.SetText(IP);
        }

        [RelayCommand]
        public async Task DeleteAllComponents()
        {
            var box = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                "Potwierdzenie kasowania",
                "Czy na pewno chcesz usunąć WSZYSTKIE części z bazy danych? Tej operacji nie można cofnąć.",
                MsBox.Avalonia.Enums.ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Warning);

            var result = await box.ShowAsync();

            if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                bool success = await DatabaseStore.ComponentStore.DeleteAllComponents();
                
                if (success)
                {
                    var infoBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Sukces", "Pomyślnie usunięto wszystkie części.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                    await infoBox.ShowAsync();
                }
                else
                {
                    var errorBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Błąd", "Wystąpił błąd podczas usuwania części.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await errorBox.ShowAsync();
                }
            }
        }
    }
}

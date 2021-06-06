using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Tabs
{
    public partial class AzureTable : ContentPage
    {
        public AzureTable()
        {
            InitializeComponent();
        }

        async void Handle_ClickedAsync(object sender, EventArgs e)
        {
            loading.IsRunning = true;
            NotesList.ItemsSource = await AzureManager.AzureManagerInstance.GetAllNotes();
            loading.IsRunning = false;
        }
    }
}

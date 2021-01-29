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
            var notes = await AzureManager.AzureManagerInstance.GetAllNotes();
            NotesList.ItemsSource = notes;
            loading.IsRunning = false;
        }
    }
}

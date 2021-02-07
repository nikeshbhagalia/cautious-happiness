using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tabs
{
    public class AzureManager
    {
        private static AzureManager _instance;
        private MobileServiceClient _client;
        private IMobileServiceTable<nbha675> _notesTable;

        public MobileServiceClient AzureClient
        {
            get { return _client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                _instance =_instance ?? new AzureManager();

                return _instance;
            }
        }

        public async Task<List<nbha675>> GetAllNotes() =>
            await _notesTable.ToListAsync();

        public async Task PostNote(nbha675 noteModel)
        {
            await _notesTable.InsertAsync(noteModel);
        }
        
        private AzureManager()
        {
            _client = new MobileServiceClient("https://nbha675.azurewebsites.net");
            _notesTable = this.client.GetTable<nbha675>();
        }
    }
}

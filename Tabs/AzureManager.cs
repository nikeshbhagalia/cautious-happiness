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
        private IMobileServiceTable<nbha675> notesTable;

        private AzureManager()
        {
            _client = new MobileServiceClient("https://nbha675.azurewebsites.net");
            this.notesTable = this.client.GetTable<nbha675>();
        }

        public MobileServiceClient AzureClient
        {
            get { return _client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new AzureManager();
                }

                return _instance;
            }
        }

        public async Task<List<nbha675>> GetAllNotes()
        {
            return await this.notesTable.ToListAsync();
        }

        public async Task PostNote(nbha675 noteModel)
        {
            await this.notesTable.InsertAsync(noteModel);
        }
    }
}

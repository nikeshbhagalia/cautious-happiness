using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Tabs
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<nbha675> notesTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("https://nbha675.azurewebsites.net");
            this.notesTable = this.client.GetTable<nbha675>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
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
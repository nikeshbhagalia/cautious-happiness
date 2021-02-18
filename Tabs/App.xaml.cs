﻿using Xamarin.Forms;

namespace Tabs
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new TabsPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

using Plugin.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace TravelrecordApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private bool hasLocationPermisson = false;
        public MapPage()
        {
            InitializeComponent();
            GetPremissions();
        }

        private async void GetPremissions()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationWhenInUsePermission>();
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.LocationWhenInUse))
                    {
                        await DisplayAlert("Need your location", "We beed to acess your location", "Ok");
                    }
                    var results = await CrossPermissions.Current.RequestPermissionAsync<LocationWhenInUsePermission>();
                    if (results == PermissionStatus.Granted) 
                    {
                        status = results;
                    }
                }
                if (status == PermissionStatus.Granted)
                {
                    locationsMap.IsShowingUser = true;
                    hasLocationPermisson = true;
                    GetLocation();
                }
                else
                {
                    await DisplayAlert("Location denied", "You didn't give us permisson to access location, so we can't show your location", "Ok");
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }
        protected override async void OnAppearing()
        {
            
            base.OnAppearing();
            if (hasLocationPermisson)
            {
                var locator = CrossGeolocator.Current;
                locator.PositionChanged += Locator_PositionChanged;
                await locator.StartListeningAsync(TimeSpan.Zero, 100);
            }
            GetLocation();
            
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CrossGeolocator.Current.StopListeningAsync();
            CrossGeolocator.Current.PositionChanged -= Locator_PositionChanged;
        }

        private void Locator_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            MoveMap(e.Position);
        }
        private void MoveMap(Position position)
        {
            var center = new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude);
            var span = new Xamarin.Forms.Maps.MapSpan(center, 1, 1);
            locationsMap.MoveToRegion(span);

        }
        private async void GetLocation()
        {
            if (hasLocationPermisson)
            {
                var locator = CrossGeolocator.Current;
                var position = await locator.GetPositionAsync();
                MoveMap(position);
            }
        }
    }
}
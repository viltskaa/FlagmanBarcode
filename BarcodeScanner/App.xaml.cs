using BarcodeScannerBusinessLogic.BusinessLogic;
using BarcodeScannerContracts.BusinessLogicContracts;
using BarcodeScannerContracts.StorageContracts;
using BarcodeScannerDatabaseImplement.Implements;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace BarcodeScanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ServiceProvider? _serviceProvider;
        public static ServiceProvider? ServiceProvider => _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                var startWindow = _serviceProvider.GetRequiredService<MainWindow>();
                startWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка при открытии MainWindow", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            base.OnStartup(e);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddTransient<IBarcodeProductStorage, BarcodeProductStorage>();
            services.AddTransient<IQrStuffStorage, QrStuffStorage>();

            services.AddTransient<IBarcodeProductLogic, BarcodeProductLogic>();
            services.AddTransient<IQrStuffLogic, QrStuffLogic>();

            services.AddTransient<MainWindow>();
            services.AddTransient<QuantityInputDialog>();
        }
    }

}

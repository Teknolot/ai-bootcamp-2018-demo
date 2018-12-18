using Autofac;
using demo.Pages;
using demo.Services;
using demo.ViewModels;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace demo
{
    public partial class App : Application
    {
        public static IContainer Container { get; set; }
        public App()
        {
            InitializeComponent();

            var builder = new ContainerBuilder();

            builder.RegisterType<LoginPageViewModel>();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                   .AssignableTo<IBaseService>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            Container = builder.Build();

            MainPage = new LoginPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
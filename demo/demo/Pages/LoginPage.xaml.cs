using Autofac;
using demo.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace demo.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{
		public LoginPage ()
		{


			InitializeComponent ();
            var viewModel = App.Container.Resolve(typeof(LoginPageViewModel));
            this.BindingContext = viewModel;
        }
	}
}
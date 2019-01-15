using demo.Helpers;
using demo.Models;
using demo.Provider;
using demo.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace demo.ViewModels
{
    public class LoginPageViewModel
    {
        private readonly IServiceManager _serviceManager;
        private readonly IDialogService _dialogService;
        private readonly ICamService _camService;
        public UserModel User { get; set; }
        private string _url;
        public LoginPageViewModel(IServiceManager serviceManager,
            ICamService camService, IDialogService dialogService)
        {
            _serviceManager = serviceManager;
            _dialogService = dialogService;
            _camService = camService;
            User = new UserModel
            {
                File = "",
                Name = ""
            };
        }

        public ICommand LoginCommand => new Command(async () =>
        {
            if (await TakeImage())
            {
                try
                {
                    _dialogService.ShowLoading();
                    var face = await _serviceManager.Post<ValidateResponseModel, ValidateRequestModel>
                    (new ValidateRequestModel
                    {
                        imageUrl = _url
                    }, Constraints.ValidateUrl);

                    if (face != null)
                        await _dialogService.ShowAlertAsync($"Your face Id is : {face.personId}",
                            $"Welcome {face.name}",
                            "Ok");
                    else
                        await _dialogService.ShowAlertAsync("No Face :(", "Warning", "Ok");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync(ex.Message, "Service Exception", "Ok");
                }
                finally
                {
                    _dialogService.HideLoading();
                    User.Name = string.Empty;
                    User.File = string.Empty;
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("No Face :(", "Warning", "Ok");
            }
        });
        public ICommand RegisterCommand => new Command(async () =>
        {
            if (await TakeImage())
            {
                try
                {
                    _dialogService.ShowLoading();
                    var addFace = await _serviceManager.Post<AddFaceResponseModel, AddFaceRequestModel>(
                    new AddFaceRequestModel
                    {
                        imageUrl = _url,
                        personName = User.Name
                    }, Constraints.AddFaceUrl);

                    if (addFace != null)
                        await _dialogService.ShowAlertAsync($"Your face Id is : {addFace.faceId}",
                            $"Your person Id is : {addFace.personId}",
                            "Ok");
                    else
                        await _dialogService.ShowAlertAsync("No Face :(", "Warning", "Ok");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync(ex.Message, "Service Exception", "Ok");
                }
                finally
                {
                    _dialogService.HideLoading();
                    User.Name = string.Empty;
                    User.File = string.Empty;
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("No Face :(", "Warning", "Ok");
            }
        });

        private async Task<bool> TakeImage()
        {
            await _camService.Init();

            var media = await _camService.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                CustomPhotoSize = 300,
                CompressionQuality = 100,
                DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                Directory = "teknolot",
                RotateImage = false,
                SaveMetaData = false,
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                AllowCropping = true,
                SaveToAlbum = false
            });

            if (media == null)
                return false;
            _url = await _serviceManager.Upload(media);
            User.File = ImageSource.FromUri(new Uri(_url));
            return String.IsNullOrEmpty(_url);
        }
    }
}
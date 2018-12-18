using demo.Services;
using Plugin.Media.Abstractions;
using System.Threading.Tasks;

namespace demo.Provider
{
    public interface IServiceManager : IBaseService
    {
        Task<T> Post<T, K>(K req, string url);
        Task<string> Upload(MediaFile media);
    }
}
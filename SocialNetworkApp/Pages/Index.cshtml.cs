using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SocialNetworkApp.Options;
using SocialNetworkApp.Storage;
using SocialNetworkApp.ViewModels;

namespace SocialNetworkApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IPictureStorageClient _pictureStorageClient;

        private readonly FunctionApiOptions _functionApiOptions;

        [BindProperty]
        [Required]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }

        public HappinessPerDayProjectionViewModel HappinessPerDayProjectionViewModel { get; set; } = new HappinessPerDayProjectionViewModel();

        public IndexModel(IPictureStorageClient pictureStorageClient, 
            IOptions<FunctionApiOptions> functionApiOptions, ILogger<IndexModel> logger)
        {
            _pictureStorageClient = pictureStorageClient;
            _functionApiOptions = functionApiOptions.Value;
            _logger = logger;
        }

        public async Task OnGet()
        {
            using var httpClient = new HttpClient();

            var baseUri = new Uri(_functionApiOptions.BaseUri);
            var relativeUri = new Uri("/api/HappinessPerDay", UriKind.Relative);

            var responseMessage = await httpClient.GetAsync(new Uri(baseUri, relativeUri));
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            
            var happinessPerDayViewModels = 
                JsonConvert.DeserializeObject<IEnumerable<HappinessPerDayViewModel>>(responseContent);

            happinessPerDayViewModels.ToList().ForEach(
                vm => HappinessPerDayProjectionViewModel.Add(vm.PostedOn, vm));
        }

        public async Task<IActionResult> OnPostUploadAsync(IFormFile formFile)
        {
            try
            {
                await using var stream = FormFile.OpenReadStream();
                await _pictureStorageClient.SaveAsync(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(null, ex);
            }
            return RedirectToPage();
        }
    }
}

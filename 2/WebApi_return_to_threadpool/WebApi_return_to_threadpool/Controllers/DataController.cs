using Microsoft.AspNetCore.Mvc;
using WebApi_return_to_threadpool.Services;

namespace WebApi_return_to_threadpool.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DataController : ControllerBase
    {
        private readonly DataService _dataService;

        public DataController(DataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDefault()
        {
            var threadIdBefore = Environment.CurrentManagedThreadId;

            var data = await _dataService.GetDataAsync();

            var threadIdAfter = Environment.CurrentManagedThreadId;

            return Ok(new
            {
                Method = "Without ConfigureAwait",
                ThreadBefore = threadIdBefore,
                ThreadAfter = threadIdAfter,
                Data = data
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetWithConfigureAwait()
        {
            var threadIdBefore = Environment.CurrentManagedThreadId;

            var data = await _dataService.GetDataWithConfigureAwaitAsync();

            var threadIdAfter = Environment.CurrentManagedThreadId;

            return Ok(new
            {
                Method = "With ConfigureAwait(false)",
                ThreadBefore = threadIdBefore,
                ThreadAfter = threadIdAfter,
                Data = data
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetWithConfigureAwaitTrue()
        {
            var threadIdBefore = Environment.CurrentManagedThreadId;

            var data = await _dataService.GetDataWithConfigureAwaitAsyncTrue();

            var threadIdAfter = Environment.CurrentManagedThreadId;

            return Ok(new
            {
                Method = "With ConfigureAwait(true)",
                ThreadBefore = threadIdBefore,
                ThreadAfter = threadIdAfter,
                Data = data
            });
        }
    }
}

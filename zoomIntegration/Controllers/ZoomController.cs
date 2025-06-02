using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using zoomIntegration.services;

namespace zoomIntegration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZoomController : ControllerBase
    {
        private readonly ZoomService _zoomService;

        public ZoomController(ZoomService zoomService)
        {
            _zoomService = zoomService;
        }

        [HttpGet("createmeeting")]
        public async Task<IActionResult> CreateMeeting()
        {
            var result = await _zoomService.CreateMeetingAsync();
            return Ok(result);
        }
    }
}
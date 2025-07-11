using GeoFieldsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoFieldsApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class FieldsController : ControllerBase
    {
        private readonly FieldService _service;

        public FieldsController(FieldService service)
        {
            _service = service;
        }

        [HttpGet("allFields")]
        public IActionResult GetAll() => Ok(_service.GetAllFields());

        [HttpGet("{id}/size")]
        public IActionResult GetSize(string id) => Ok(_service.GetSizeById(id));

        [HttpGet("{id}/distance")]
        public IActionResult GetDistance(string id, [FromQuery] double lat, [FromQuery] double lng)
            => Ok(_service.GetDistanceToCenter(id, lat, lng));

        [HttpGet("contains")]
        public IActionResult PointInside([FromQuery] double lat, [FromQuery] double lng)
            => Ok(_service.CheckPointBelongs(lat, lng));
    }
}

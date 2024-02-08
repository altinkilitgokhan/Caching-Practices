using Caching_Practices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Caching_Practices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CachingPracticeController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;

        public CachingPracticeController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet(Name = "GetInMemoryCache")]
        public IActionResult Get()
        {
            var cachedData = _memoryCache.Get<List<UserInfoModel>>("UserInfo");
            if (cachedData != null)
            {
                return Ok(cachedData.Select(s => new UserInfoModelResponse() { Id = s.Id, Name = s.Name, IsReadFromCache = true }));
            }

            cachedData = GetUserMockInfo();
            _memoryCache.Set("UserInfo", cachedData, DateTimeOffset.Now.AddHours(1));
            return Ok(cachedData.Select(s => new UserInfoModelResponse() { Id = s.Id, Name = s.Name, IsReadFromCache = false }));
        }

        private List<UserInfoModel> GetUserMockInfo()
        {
            using (StreamReader r = new StreamReader("./MockData/UserInfoMock.json"))
            {
                string json = r.ReadToEnd();
                List<UserInfoModel> items = JsonConvert.DeserializeObject<List<UserInfoModel>>(json);
                return items;
            }
        }
    }
}
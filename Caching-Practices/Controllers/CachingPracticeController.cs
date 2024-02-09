using Caching_Practices.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Caching_Practices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CachingPracticeController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDatabase _distributedCache;

        public CachingPracticeController(IMemoryCache memoryCache, IConnectionMultiplexer conRedis)
        {
            _memoryCache = memoryCache;
            _distributedCache = conRedis.GetDatabase();
        }

        [HttpGet("GetInMemoryCache")]
        public IActionResult GetInMemoryCache()
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

        [HttpGet("GetDistributedCache")]
        public IActionResult GetDistributedCache()
        {
            var result = _distributedCache.StringGet("UserInfo");
            bool isReadFromCache = true;
            if (result.IsNull)
            {
                result = JsonConvert.SerializeObject(GetUserMockInfo());
                _distributedCache.StringSet("UserInfo", result, TimeSpan.FromHours(1));
                isReadFromCache = false;
            }

            var cachedData = JsonConvert.DeserializeObject<List<UserInfoModel>>(result);
            return Ok(cachedData.Select(s => new UserInfoModelResponse() { Id = s.Id, Name = s.Name, IsReadFromCache = isReadFromCache }));
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
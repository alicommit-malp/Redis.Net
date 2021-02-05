# Local Test


# Usage Method 1
Single redis server

Prerequisite
```bash
docker run --name redis1 -d -p 6379:6379  redis redis-server --appendonly yes
```

```c#
public void ConfigureServices(IServiceCollection services)
{
    var redis2ConfigurationOptions = ConfigurationOptions.Parse("localhost:6379");
    redis2ConfigurationOptions.ReconnectRetryPolicy = new ExponentialRetry(1000);
    services.AddRedisDotNet(new RedisCacheOptions()
    {
        ConfigurationOptions = redis2ConfigurationOptions
    });
}

public class TestController : Controller
{
    private readonly IRedisService _redisService;
    public TestController(IRedisService redisService){
        _redisService = redisService;
    }

    public IActionResult Get(string key){
        var result = _redisService.GetString(key);
        return OK(result);
    }

    public async Task<IActionResult> GetAsync(string key){
        var result = await _redisService.GetStringAsync(key);
        return OK(result);
    }
}


```

# Usage Method 2
Multiple Redis servers

Prerequisite
```bash
docker run --name redis1 -d -p 6379:6379  redis redis-server --appendonly yes
docker run --name redis2 -d -p 6380:6380  redis redis-server --appendonly yes
```

```c#
public void ConfigureServices(IServiceCollection services)
{
    var redis1ConfigurationOptions = ConfigurationOptions.Parse("localhost:6379");
    redis1ConfigurationOptions.ReconnectRetryPolicy = new ExponentialRetry(1000);
    services.AddRedisDotNet<Redis1>(new RedisCacheOptions()
    {
       ConfigurationOptions = redis1ConfigurationOptions
    });

    var redis2ConfigurationOptions = ConfigurationOptions.Parse("localhost:6380");
    redis2ConfigurationOptions.ReconnectRetryPolicy = new ExponentialRetry(1000);
    services.AddRedisDotNet<Redis2>(new RedisCacheOptions()
    {
       ConfigurationOptions = redis2ConfigurationOptions
    });
}


public class Redis1 : RedisService<Redis1>
{
    public Redis1(IOptionsMonitor<RedisCacheOptions> cacheOptions) : base(cacheOptions)
    {
    }
}


public class Redis2 : RedisService<Redis2>
{
    public Redis2(IOptionsMonitor<RedisCacheOptions> cacheOptions) : base(cacheOptions)
    {
    }
}

public class Redis1Controller : ControllerBase
{
    private readonly Redis1 _redis1;

    public Redis1Controller(Redis1 redis1)
    {
        _redis1 = redis1;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _redis1.GetStringAsync("redis1"));
    }
}


public class Redis2Controller : ControllerBase
{
    private readonly Redis2 _redis2;

    public Redis2Controller(Redis2 redis2)
    {
        _redis2 = redis2;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _redis2.GetStringAsync("redis2"));
    }
}


```


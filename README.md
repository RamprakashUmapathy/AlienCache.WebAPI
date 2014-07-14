# AlienCache.WebAPI #

**AlienCache.WebAPI** provides cache features for Web API through [ActionFilterAttribute](http://msdn.microsoft.com/en-us/library/system.web.http.filters.actionfilterattribute(v=vs.118).aspx).


## Acknowledgement ##

This library is based on @[Emerson Soares](https://twitter.com/emerson_soares)' [WebApiCache](https://github.com/emersonsoares/WebApiCache).


## Getting Started ##

**AlienCache.WebAPI** is a custom action filter attribute, therefore, it should be used for either Web API controllers or individual actions.

```csharp
[WebApiCache(WebApiCacheConfigurationSettingsProviderType = typeof(WebApiCacheConfigurationSettingsProvider))]
public class SampleApiController : ApiController
{
    ...
}
```

In order to configure the `WebApiCacheAttribute` instance, `Web.config` should be considered.

```xml
<applicationSettings>
    <Aliencube.AlienCache.WebApi.Properties.Settings>
        <setting name="TimeSpan" serializeAs="String">
            <value>60</value>
        </setting>
        <setting name="UseAbsoluteUrl" serializeAs="String">
            <value>False</value>
        </setting>
        <setting name="UseQueryStringAsKey" serializeAs="String">
            <value>False</value>
        </setting>
        <setting name="QueryStringKey" serializeAs="String">
            <value />
        </setting>
        <setting name="CacheableStatusCodes" serializeAs="String">
            <value>200,304</value>
        </setting>
    </Aliencube.AlienCache.WebApi.Properties.Settings>
</applicationSettings>
```

* `TimeSpan`: Duration for how long the cache value is alive, in seconds. Default value is `60`.
* `UseAbsoluteUrl`: If it is set to `true`, the cache key will use the fully qualified URL to store cache value. Default value is `false`.
* `UseQueryStringAsKey`: If it is set to `true`, the cache key will use query string value corresponding to a specified key. Default value is `false`.
* `QueryStringKey`: The key from query string to consider cache key. If `UseQueryStringAsKey` is `false`, this value is ignored.
* `CacheableStatusCodes`: This is the list of HTTP status codes, delimited by comma, that allow to store into the cache. Default value is `200,304` that is equivalent to `OK` and `Not Modified`.


## License ##

**AlienCache.WebAPI** is released under [MIT License](http://opensource.org/licenses/MIT).

> The MIT License (MIT)
> 
> Copyright (c) 2014 [aliencube.org](http://aliencube.org)
> 
> Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
> 
> The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
> 
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

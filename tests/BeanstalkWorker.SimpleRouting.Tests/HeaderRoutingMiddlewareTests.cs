using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BeanstalkWorker.SimpleRouting.Tests
{
    public class HeaderRoutingMiddlewareTests
    {
        private HttpContext _context;
        private readonly NullLogger<HeaderRoutingMiddleware> _logger;
        
        private readonly HeaderRoutingMiddleware _target;

        public HeaderRoutingMiddlewareTests()
        {
            _logger = new NullLogger<HeaderRoutingMiddleware>();

            _target = new HeaderRoutingMiddleware(context => Task.CompletedTask);
        }

        [Fact]
        public void GivenGetInsteadOfPost_WhenInvoke_ThenBadRequest()
        {
            // Arrange

            _context = new HttpContextBuilder()
                .WithMethod(HttpMethod.Get);

            // Act

            _target.Invoke(_context, _logger);

            // Assert
            
            Assert.Equal(400, _context.Response.StatusCode);
        }
        
        [Fact]
        public void GivenNonRootPath_WhenInvoke_ThenBadRequest()
        {
            // Arrange

            _context = new HttpContextBuilder()
                .WithPath("/some-path");

            // Act

            _target.Invoke(_context, _logger);

            // Assert
            
            Assert.Equal(400, _context.Response.StatusCode);
        }
        
        [Fact]
        public void GivenMissingTaskHeader_WhenInvoke_ThenBadRequest()
        {
            // Arrange

            _context = new HttpContextBuilder()
                .WithoutTaskHeader();

            // Act

            _target.Invoke(_context, _logger);

            // Assert
            
            Assert.Equal(400, _context.Response.StatusCode);
        }
        
        [Fact]
        public void GivenValidContext_WhenInvoke_ThenSetPath()
        {
            // Arrange

            _context = new HttpContextBuilder();

            // Act

            _target.Invoke(_context, _logger);

            // Assert
            
            Assert.Equal(200, _context.Response.StatusCode);
            Assert.Equal("/do-work", _context.Request.Path);
        }

        private class HttpContextBuilder
        {
            private readonly HttpContext _context;
            private const string TaskHeaderKey = "X-Aws-Sqsd-Attr-Task";

            public HttpContextBuilder()
            {
                _context =  new DefaultHttpContext();
                _context.Request.Method = "POST";
                _context.Request.Path = "/";
                _context.Request.Headers.Add(TaskHeaderKey, "do-work");
            }

            public HttpContextBuilder WithPath(string path)
            {
                _context.Request.Path = path;

                return this;
            }

            public HttpContextBuilder WithMethod(HttpMethod method)
            {
                _context.Request.Method = method.Method;

                return this;
            }

            public HttpContextBuilder WithoutTaskHeader()
            {
                _context.Request.Headers.Remove(TaskHeaderKey);
                
                return this;
            }

            public static implicit operator HttpContext(HttpContextBuilder b)
            {
                return b._context;
            }
        }
    }
}
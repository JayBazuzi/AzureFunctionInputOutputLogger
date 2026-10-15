using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace Bazuzi.AzureFunctionInputOutputLogger.Tests;

public class InputOutputLoggingMiddlewareTests
{
    private static InputOutputLoggingMiddleware CreateMiddleware(
        CapturingLogger<InputOutputLoggingMiddleware> logger,
        bool requireMarkerAttribute = false)
        => new(
            logger,
            requireMarkerAttribute,
            context => new ValueTask<HttpRequestData?>(((FakeFunctionContext)context).Request),
            context => ((FakeFunctionContext)context).Response,
            context => ((FakeFunctionContext)context).MarkedParts);

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task MarkerIsOnlyRequiredWhenOptionIsEnabled(bool requireMarkerAttribute, bool isMarked)
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var context = new FakeFunctionContext { MarkedParts = isMarked ? LoggedParts.InputAndOutput : LoggedParts.None };
        context.Request = new FakeHttpRequestData(context, "GET", "https://example.test/api/x", "");
        var called = false;

        await CreateMiddleware(logger, requireMarkerAttribute).Invoke(context, ctx =>
        {
            called = true;
            context.Response = new FakeHttpResponseData(ctx, HttpStatusCode.OK, "");
            return Task.CompletedTask;
        });

        Assert.True(called);
        Assert.Equal(!requireMarkerAttribute || isMarked, logger.Messages.Count > 0);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task LogsOnlyTheMarkedParts(bool input, bool output)
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var context = new FakeFunctionContext();
        context.Mark(input, output);
        context.Request = new FakeHttpRequestData(context, "POST", "https://example.test/api/orders", """{"item":"widget"}""");

        await CreateMiddleware(logger, requireMarkerAttribute: true).Invoke(context, ctx =>
        {
            context.Response = new FakeHttpResponseData(ctx, HttpStatusCode.Created, """{"id":42}""");
            return Task.CompletedTask;
        });

        await Verify(logger.Messages).UseParameters(input, output);
    }

    [Fact]
    public async Task LogsRequestAndResponseAsSeparateEntries()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var context = new FakeFunctionContext();
        context.Request = new FakeHttpRequestData(context, "POST", "https://example.test/api/orders?a=1&b=two", """{"item":"widget"}""");

        await CreateMiddleware(logger).Invoke(context, ctx =>
        {
            context.Response = new FakeHttpResponseData(ctx, HttpStatusCode.Created, """{"id":42}""");
            return Task.CompletedTask;
        });

        await Verify(logger.Messages);
    }

    [Fact]
    public async Task RedactsFunctionKeyInQuery()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var context = new FakeFunctionContext();
        context.Request = new FakeHttpRequestData(context, "GET", "https://example.test/api/orders?Code=secret1&code=secret2&name=widget&tag=a&tag=b", "");

        await CreateMiddleware(logger).Invoke(context, ctx =>
        {
            context.Response = new FakeHttpResponseData(ctx, HttpStatusCode.OK, "");
            return Task.CompletedTask;
        });

        await Verify(logger.Messages);
    }

    [Fact]
    public async Task BodiesRemainReadableAfterLogging()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var context = new FakeFunctionContext();
        var request = new FakeHttpRequestData(context, "POST", "https://example.test/api/x", "payload");
        context.Request = request;

        string? seenByFunction = null;
        await CreateMiddleware(logger).Invoke(context, async _ =>
            seenByFunction = await new StreamReader(request.Body).ReadToEndAsync());

        Assert.Equal("payload", seenByFunction);
    }

    [Fact]
    public async Task NonHttpInvocationsAreNotLogged()
    {
        var logger = new CapturingLogger<InputOutputLoggingMiddleware>();
        var called = false;

        await CreateMiddleware(logger).Invoke(new FakeFunctionContext(), _ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        Assert.True(called);
        Assert.Empty(logger.Messages);
    }
}

//-----------------------------------------------------------------------
// <copyright file="AddressValidationService.cs" company="Procare Software, LLC">
//     Copyright © 2021-2025 Procare Software, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Procare.AddressValidation.Tester;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "This is instantiated via DI.")]
internal sealed class AddressValidationService
{
    internal const string HttpClientName = "AddressValidationHttpClient";

    private readonly IHttpClientFactory httpClientFactory;

    public AddressValidationService(IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory, nameof(httpClientFactory));

        this.httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetAddressesAsync(AddressValidationRequest request, CancellationToken token = default)
    {
        try
        {
        using HttpClient httpClient = this.httpClientFactory.CreateClient(HttpClientName);
        using HttpRequestMessage httpRequest = request.ToHttpRequest();
        using HttpResponseMessage response = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new AddressValidationException(
                "Network error during address validation",
                statusCode: ex.StatusCode,
                innerException: ex);
        }
        catch (Exception ex)
        {
            throw new AddressValidationException("Error calling address validation service.", null, 0, ex);
        }
    }
}

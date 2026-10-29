using System.Text.Json;
using Jsonata.Net.Native;
using Microsoft.Extensions.Logging;



namespace ModuloFivePadClient.BotConnections.Http
{

    public partial class HttpBotConnectionBuilder
    {
        private class BotConnectionImpl : IHttpBotConnection
        {
            private readonly ILogger<BotConnectionImpl> _logger;
            private HttpClient _httpClient;
            private readonly string _statusPath;
            private readonly JsonataQuery _jsonataStatusResponseQuery;
            private readonly string _actionListPath;
            private readonly JsonataQuery _jsonataActionListResponseQuery;
            private readonly string _actionInvocationPathFormatString;
            private readonly JsonataQuery _jsonataActionInvocationRequestQuery;
            // private readonly JsonataQuery _jsonataActionInvocationResponseQuery;

            public event ActionListRetreivedHandler? OnActionListRetrieved;
            public event ActionSuceededHandler? ActionSuceeded;
            public event ActionFailedHandler? ActionFailed;

            public BotConnectionImpl(HttpConfiguration cfg, ILogger<BotConnectionImpl> logger)
            {
                this.BotName = cfg.Name ?? "";
                this._logger = logger;

                #region URLs

                Uri baseUri;
                if (string.IsNullOrWhiteSpace(cfg.BaseUri))
                {
                    throw new BotConnectionConfigurationException($"Base URI was empty.");
                }
                try
                {
                    baseUri = new(cfg.BaseUri);

                    this._httpClient = new()
                    {
                        BaseAddress = baseUri
                    };
                }
                catch (UriFormatException ex)
                {
                    throw new BotConnectionConfigurationException($"Bad base URI {cfg.BaseUri}.", ex);
                }

                if (string.IsNullOrWhiteSpace(cfg.StatusRelativeUrl))
                {
                    throw new BotConnectionConfigurationException($"Status relative is was empty.");
                }
                try
                {
                    _ = new Uri(baseUri, cfg.StatusRelativeUrl);
                    _statusPath = cfg.StatusRelativeUrl;
                }
                catch (UriFormatException ex)
                {
                    throw new BotConnectionConfigurationException($"Status list relative URL {cfg.BaseUri}.", ex);
                }

                if (string.IsNullOrWhiteSpace(cfg.ActionListRelativeUrl))
                {
                    throw new BotConnectionConfigurationException($"Action list relative is was empty.");
                }
                try
                {
                    _ = new Uri(baseUri, cfg.ActionListRelativeUrl);
                    _actionListPath = cfg.ActionListRelativeUrl;
                }
                catch (UriFormatException ex)
                {
                    throw new BotConnectionConfigurationException($"Bad action list relative URL {cfg.BaseUri}.", ex);
                }

                if (string.IsNullOrWhiteSpace(cfg.ActionInvocationRelativeUrlFormatString))
                {
                    throw new BotConnectionConfigurationException($"Action invocation relative is was empty.");
                }
                try
                {
                    var testUri = string.Format(cfg.ActionInvocationRelativeUrlFormatString, Guid.Empty);
                    _ = new Uri(baseUri, cfg.ActionInvocationRelativeUrlFormatString);
                    _actionInvocationPathFormatString = cfg.ActionInvocationRelativeUrlFormatString;
                }
                catch (UriFormatException ex)
                {
                    throw new BotConnectionConfigurationException($"Bad action invocation relative URL {cfg.BaseUri}.", ex);
                }

                #endregion URLs

                #region Jsonata

                if (string.IsNullOrWhiteSpace(cfg.StatusTransform))
                {
                    throw new BotConnectionConfigurationException($"Status transformation is empty.");
                }
                try
                {
                    _jsonataStatusResponseQuery = new JsonataQuery(cfg.StatusTransform);
                }
                catch (JsonataException ex)
                {
                    _logger.LogError("Bad status response transformation JSON: {actionListTransformation}", cfg.StatusTransform);
                    throw new BotConnectionConfigurationException($"Status transformation was not valid Jsonata.", ex);
                }

                if (string.IsNullOrWhiteSpace(cfg.ActionListTransform))
                {
                    throw new BotConnectionConfigurationException($"Action invocation transformation is empty.");
                }
                try
                {
                    _jsonataActionListResponseQuery = new JsonataQuery(cfg.ActionListTransform);
                }
                catch (JsonataException ex)
                {
                    _logger.LogError("Bad action list response transformation JSON: {actionListTransformation}", cfg.ActionListTransform);
                    throw new BotConnectionConfigurationException($"Action list response transformation was not valid Jsonata.", ex);
                }

                if (string.IsNullOrWhiteSpace(cfg.ActionInvocationRequestTransform))
                {
                    throw new BotConnectionConfigurationException($"Action invocation request transformation is empty.");
                }
                try
                {
                    _jsonataActionInvocationRequestQuery = new JsonataQuery(cfg.ActionInvocationRequestTransform);
                }
                catch (JsonataException ex)
                {
                    _logger.LogError("Bad action invocation request transformation JSON: {actionListTransformation}", cfg.ActionListTransform);
                    throw new BotConnectionConfigurationException($"Action invocation request transformation was not valid JSON.", ex);
                }

                // if (string.IsNullOrWhiteSpace(cfg.ActionInvocationResponseTransform))
                // {
                //     throw new BotConnectionConfigurationException($"Action invocation response transformation is empty.");
                // }
                // try
                // {
                //     _jsonataActionInvocationResponseQuery = new JsonataQuery(cfg.ActionInvocationResponseTransform);
                // }
                // catch (JsonataException ex)
                // {
                //     _logger.LogError("Bad action invocation response transformation JSON: {actionListTransformation}", cfg.ActionListTransform);
                //     throw new BotConnectionConfigurationException($"Action invocation response transformation was not valid JSON.", ex);
                // }

                #endregion Jsonata

                BotActions = [];
            }

            public string? BaseUri { get => _httpClient?.BaseAddress?.ToString(); }
            public string BotName { get; private set; }
            public IList<BotAction> BotActions { get; private set; }

            public async Task<BotStatus> GetStatusAsync()
            {
                try
                {
                    var responseMessage = await this._httpClient.GetStreamAsync(_statusPath);
                    var responseString = await new StreamReader(responseMessage).ReadToEndAsync();

                    string statusJson = _jsonataStatusResponseQuery.Eval(responseString);
                    // Native AOT JSON Support 
                    BotStatus status = JsonSerializer.Deserialize(statusJson, BotActionJsonContext.Default.BotStatus);
                    return status;
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is UriFormatException)
                {
                    _logger.LogWarning(ex, "Action list path {_actionListPath} is invalid.", _actionListPath);
                }
                catch (HttpRequestException ex)
                {
                    switch (ex.HttpRequestError)
                    {
                        case HttpRequestError.NameResolutionError:
                            {
                                _logger.LogWarning(ex, "The base URL {baseUrl} is structurally valid but did not resolve correctly.", _httpClient.BaseAddress);
                                break;
                            }
                        default:
                            {
                                _logger.LogWarning(ex, "A network error occurred.");
                                break;
                            }
                    }
                }
                catch (JsonataException ex)
                {
                    _logger.LogWarning(ex, "Action list JSON response could not be transformed by the query.");
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Action list JSON response could not be parsed as a {type}.", nameof(BotStatus));
                }

                return BotStatus.Unavailable;
            }

            public async Task<IList<BotAction>> GetBotActionListsAsync()
            {
                _logger.LogDebug("In {class}.{func}.", nameof(BotConnectionImpl), nameof(GetBotActionListsAsync));

                try
                {
                    var responseMessage = await this._httpClient.GetStreamAsync(_actionListPath);
                    var responseString = await new StreamReader(responseMessage).ReadToEndAsync();

                    string botActionsJson = _jsonataActionListResponseQuery.Eval(responseString);
                    // Native AOT JSON Support 
                    BotActions =
                        JsonSerializer.Deserialize(
                            botActionsJson,
                            BotActionJsonContext.Default.BotActionArray) ?? [];

                    OnActionListRetrieved?.Invoke(this, BotActions);

                    _logger.LogDebug(
                        "In {class}.{func}, retrieved {number} bot actions.",
                        nameof(BotConnectionImpl),
                        nameof(GetBotActionListsAsync),
                        BotActions.Count
                    );
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is UriFormatException)
                {
                    _logger.LogWarning(ex, "Action list path {_actionListPath} is invalid.", _actionListPath);
                }
                catch (HttpRequestException ex)
                {
                    switch (ex.HttpRequestError)
                    {
                        case HttpRequestError.NameResolutionError:
                            {
                                _logger.LogWarning(ex, "The base URL {baseUrl} is structurally valid but did not resolve correctly.", _httpClient.BaseAddress);
                                break;
                            }
                        default:
                            {
                                _logger.LogWarning(ex, "A network error occurred.");
                                break;
                            }
                    }
                }
                catch (JsonataException ex)
                {
                    _logger.LogWarning(ex, "Action list JSON response could not be transformed by the query.");
                }

                return BotActions;
            }

            public async Task<bool> InvokeActionAsync(string botActionId, IEnumerable<BotArgument> arguments)
            {
                _logger.LogDebug("In {class}.{func}.", nameof(BotConnectionImpl), nameof(InvokeActionAsync));

                var jsonRequestBodyModuloFive = JsonSerializer.Serialize(arguments.ToArray(), BotActionJsonContext.Default.BotArgumentArray);
                _logger.LogDebug("In {class}.{func}, ModuloFive-style request body is {body}.", nameof(BotConnectionImpl), nameof(InvokeActionAsync), jsonRequestBodyModuloFive);

                var jsonRequestBodyBot = _jsonataActionInvocationRequestQuery.Eval(jsonRequestBodyModuloFive);
                _logger.LogDebug("In {class}.{func}, {bot}-style request body is {body}.", nameof(BotConnectionImpl), nameof(InvokeActionAsync), BotName, jsonRequestBodyBot);

                var path = string.Format(_actionInvocationPathFormatString, botActionId);
                _logger.LogDebug("In {class}.{func}, path is {path}.", nameof(BotConnectionImpl), nameof(InvokeActionAsync), path);

                try
                {
                    var responseMessage = await this._httpClient.PostAsync(path, new StringContent(jsonRequestBodyBot));

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        _logger.LogDebug("In {class}.{func}, action suceeded.", nameof(BotConnectionImpl), nameof(InvokeActionAsync));
                        this.ActionSuceeded?.Invoke(this, botActionId);
                        return true;
                    }
                    else
                    {
                        _logger.LogDebug(
                            "In {class}.{func}, action failed {code} with reason {reason}.",
                            nameof(BotConnectionImpl),
                            nameof(InvokeActionAsync),
                            responseMessage.StatusCode,
                            responseMessage.ReasonPhrase
                        );
                        this.ActionFailed?.Invoke(this, botActionId, new HttpActionFailureReason(responseMessage));
                        return false;
                    }
                }
                catch (HttpRequestException ex)
                {
                    this.ActionFailed?.Invoke(this, botActionId, new HttpActionFailureReason(ex));
                    return false;
                }
            }

            public bool InvokeAction(string botActionId, IEnumerable<BotArgument> arguments)
            {
                return InvokeActionAsync(botActionId, arguments).GetAwaiter().GetResult();
            }
        }
    }
}
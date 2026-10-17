using ModuloFivePadClient.BotConnections.Http;

namespace ModuloFivePadClient.BotConnections
{
    #warning firebot ALWAYS has a username arg
    public class TemporaryConfigProvider : IHttpBotConnectionConfigurationProvider
    {
        public System.Collections.Generic.IEnumerable<HttpConfiguration> Configurations
        {
            get
            {
                return [
                    new HttpConfiguration {
                        Name = "Firebot",
                        BaseUri = "http://localhost:7472/",
                        StatusRelativeUrl = "api/v1/status",
                        StatusTransform = """
[$keys(connections)][0]="chat"?'Available':'Unavailable'
""",
                        ActionListRelativeUrl = "api/v1/effects/preset",
                        ActionListTransform = """
$.{ "Identifier": id, "Name": name, "Parameters": [args.{ "Name": $, "Type": "string" }] }
""",
                        ActionInvocationRelativeUrlFormatString = "api/v1/effects/preset/{0}",
                        ActionInvocationRequestTransform = """
{ "username": null, "args": $.Name }
""",
                        ActionInvocationResponseTransform = "",
                    }
                ];
            }
        }
    }
}
//                         StatusTransform = """
// $.{"Status": [$keys(connections)][0]="chat"?'Available':'Unavailable'}
// """,
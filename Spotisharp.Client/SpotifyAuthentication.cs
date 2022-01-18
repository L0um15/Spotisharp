using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using Spotisharp.Client.Models;
using System.ComponentModel;
using System.Text.Json;

public static class SpotifyAuthentication
{

    private static PKCETokenModel _pkceTokenModel = new PKCETokenModel();
    
    private static string _tokenConfigDir =
        Path.Join
        (
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "spotisharp"
        );
    private static string _tokenConfigFile = Path.Join(_tokenConfigDir, "auth.json");

    public static async Task<SpotifyClient?> CreateSpotifyClient()
    {
        string clientId = "b1d70eb4c56440f5b56537e96e079c7d";

        if (File.Exists(_tokenConfigFile))
        {
            CConsole.Debug("Authentication data already exists. Loading");
            string tokenConfigContent = File.ReadAllText(_tokenConfigFile);
            var deserializedJson = JsonSerializer.Deserialize<PKCETokenModel>(tokenConfigContent);
            if(deserializedJson != null)
            {
                CConsole.Debug("Authentication data has been loaded successfully. Requesting new access token");
                var newResponse = await TryGetPKCERefreshTokenResponse(clientId,deserializedJson.RefreshToken);
                if(newResponse != null)
                {
                    CConsole.Debug("Access token has been received. Saving data");
                    deserializedJson.RefreshToken = newResponse.RefreshToken;
                    string serializedJson = JsonSerializer.Serialize(deserializedJson);
                    File.WriteAllText(_tokenConfigFile, serializedJson);
                    return new SpotifyClient(newResponse.AccessToken);
                }
            }
            CConsole.Debug("Failed to deserialize authentication data. Generating new access token");
        }

        var (verifier, challenge) = PKCEUtil.GenerateCodes(120);

        Uri serverUri = new Uri("http://localhost:5000/auth");
        var loginRequest = new LoginRequest(serverUri, clientId, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256"
        };

        BrowserUtil.Open(loginRequest.ToUri());
        CConsole.Warn("Web browser should open by itself, if it didnt then please visit link below");
        CConsole.Warn(loginRequest.ToUri());


        var getCallbackTask = GetCallbackFromServer(serverUri, clientId);

        if (getCallbackTask.Wait(40000))
        {
            string authorizationCode = await getCallbackTask;
            CConsole.Debug("User has granted access for application. Requesting new access token");
            var newResponse = await new OAuthClient().RequestToken
                (
                    new PKCETokenRequest
                        (
                            clientId,
                            authorizationCode,
                            serverUri,
                            verifier
                        )
                );

            CConsole.Debug("Saving refresh token");
            _pkceTokenModel.RefreshToken = newResponse.RefreshToken;
            string serializedJson = JsonSerializer.Serialize(_pkceTokenModel);
            File.WriteAllText(_tokenConfigFile, serializedJson);
            return new SpotifyClient(newResponse.AccessToken);
        }
        else
        {
            CConsole.Debug("Timed out");
            return null;
        }
    }
    private static async Task<string> GetCallbackFromServer(Uri redirectUri, string clientID)
    {
        var embededAuthServer = new EmbedIOAuthServer(redirectUri, 5000);
        await embededAuthServer.Start();
        string authorizationCode = string.Empty;

        embededAuthServer.AuthorizationCodeReceived += async (sender, response) =>
        {
            await embededAuthServer.Stop();
            authorizationCode = response.Code;
        };

        while (authorizationCode == string.Empty)
        {
            await Task.Delay(1000);
        }
        return authorizationCode;
    }

    private static async Task<PKCETokenResponse?> TryGetPKCERefreshTokenResponse(string clientId, string refreshToken)
    {
        try
        {
            return await new OAuthClient().RequestToken(new PKCETokenRefreshRequest(clientId, refreshToken));
        }
        catch (APIException)
        {
            return null;
        }
    }
}

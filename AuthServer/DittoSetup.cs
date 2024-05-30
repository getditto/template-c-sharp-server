
using DittoSDK;

public static class DittoInitializer 
{
    public static Ditto InitializeAndStartDitto(DittoAuthConfig dittoAuthConfig)
    {
        DittoLogger.SetLoggingEnabled(true);
        DittoLogger.SetMinimumLogLevel(DittoLogLevel.Debug);

        var serverIdentity = DittoIdentity.SharedKey(
            dittoAuthConfig.AppId,
            dittoAuthConfig.SharedKey
        );
        var ditto = new Ditto(serverIdentity);
        ditto.DisableSyncWithV3();
        ditto.DeviceName = "TestServer";

        ditto.TransportConfig = CreateTransportConfig(dittoAuthConfig);

        try
        {
            ditto.SetOfflineOnlyLicenseToken(dittoAuthConfig.OfflineLicenseToken);
            ditto.StartSync();
            Console.WriteLine("Ditto launched!");
            Console.WriteLine("Waiting for auth requests...");
        }
        catch (DittoException ex)
        {
            Console.WriteLine("There was an error starting Ditto.");
            Console.WriteLine("Here's the following error");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("Ditto cannot start sync but don't worry.");
            Console.WriteLine("Ditto will still work as a local database.");
        }

        ditto.DittoIdentityProviderAuthenticationRequest += (s, args) => OnDittoIdentityProviderAuthRequest(args, dittoAuthConfig);

        return ditto;
    }

    private static DittoTransportConfig CreateTransportConfig(DittoAuthConfig dittoAuthConfig) 
    {
        // Server is an HTTP/WebSocket server only
        var serverConfig = new DittoTransportConfig();
        serverConfig.Listen.Http.Enabled = true;
        serverConfig.Listen.Http.InterfaceIp = "127.0.0.1";
        serverConfig.Listen.Http.Port = 45002;
        serverConfig.Listen.Http.WebsocketSync = true;
        serverConfig.Listen.Http.IdentityProvider = true;
        /* Optional: for HTTPS
        serverConfig.Listen.Http.TlsKeyPath = "";
        serverConfig.Listen.Http.TlsCertificatePath = "";
        */
        serverConfig.Listen.Http.IdentityProviderSigningKey = dittoAuthConfig.SigningKey;
        serverConfig.Listen.Http.IdentityProviderVerifyingKeys.Add(dittoAuthConfig.VerifyingKey);

        return serverConfig;
    }

    private static void OnDittoIdentityProviderAuthRequest(DittoAuthenticationRequestEventArgs args, DittoAuthConfig dittoAuthConfig)
    {
        Console.WriteLine("- - -");
        Console.WriteLine("Got Request: ");
        Console.WriteLine(args.ThirdPartyToken);
        Console.WriteLine(args.AppId);
        if (args.AppId == dittoAuthConfig.AppId && args.ThirdPartyToken == "jellybeans")
        {
            var success = new DittoAuthenticationSuccess
            {
                AccessExpires = DateTime.Now + new TimeSpan(1, 0, 0),
                UserId = "bob",
                ReadEverythingPermission = true,
                WriteEverythingPermission = true
            };
            Console.WriteLine("Sign in successful!");
            args.Allow(success);
        }
        else
        {
            args.Deny();
        }
        Console.WriteLine("- - -");
    }
}
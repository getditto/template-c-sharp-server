using DittoSDK;

class AuthDelegate : IDittoAuthenticationDelegate
{
    public void AuthenticationRequired(DittoAuthenticator authenticator)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nAuth required!! → try \"--login jellybeans\"");
        Console.ResetColor();
    }

    public void AuthenticationExpiringSoon(DittoAuthenticator authenticator, long secondsRemaining)
    {
        Console.WriteLine($"Auth token expiring in {secondsRemaining} seconds");
    }
}
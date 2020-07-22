using System;

namespace GenerateMicrosoftIdentityWebTestScripts
{
    public class TemplateDescription
    {
        public string Description { get; set; }
        public string Template { get; set; }
    }

    enum Authentication
    {
        NoAuth,
        SingleOrg,
        B2C
    };

    enum Calls
    {
        NoDownstreamApi,
        CallsGraph,
        CallsWebApi
    };

    class Program
    {
        static TemplateDescription[] cases = new TemplateDescription[]{
          new TemplateDescription {Template = "webapp2", Description = "Razor web app"},
          new TemplateDescription {Template = "webapi2", Description = "Web api"},
          new TemplateDescription {Template = "mvc2", Description = "MVC Web app"},
          new TemplateDescription {Template = "blazorserver2", Description = "Blazor app"},
          new TemplateDescription {Template = "blazorwasm2", Description = "Blazor web assembly"},
        };


        public static string GetFlags(Authentication authentication)
        {
            switch (authentication)
            {
                case Authentication.NoAuth:
                    return string.Empty;
                case Authentication.SingleOrg:
                    return "--auth SingleOrg";
                case Authentication.B2C:
                    return "--auth IndividualOrgB2C";
            }
            return string.Empty;
        }

        public static string GetText(Authentication authentication)
        {
            switch (authentication)
            {
                case Authentication.NoAuth:
                    return " no authentication";
                case Authentication.SingleOrg:
                    return " SingleOrg";
                case Authentication.B2C:
                    return " B2C";
            }
            return string.Empty;
        }
        public static string GetFlags(Calls calls, Authentication authentication)
        {
            switch (calls)
            {
                case Calls.NoDownstreamApi:
                    return string.Empty;
                case Calls.CallsGraph:
                    return "--calls-graph";
                case Calls.CallsWebApi:
                    if (authentication == Authentication.B2C)
                    {
                        return "--called-api-url \"https://localhost:44332\" --called-api-scopes \"https://fabrikamb2c.onmicrosoft.com/tasks/read\"";
                    }
                    else
                    {
                        return "--called-api-url \"https://graph.microsoft.com/beta/me\"--called-api-scopes \"user.read\"";
                    }
                default:
                    return string.Empty;
            }
        }

        public static string GetText(Calls calls)
        {
            switch (calls)
            {
                case Calls.NoDownstreamApi:
                    return string.Empty;
                case Calls.CallsGraph:
                    return " calling Microsoft Graph";
                case Calls.CallsWebApi:
                    return " calling a web API";
                default:
                    return string.Empty;
            }
        }

        static void Main(string[] args)
        {
            foreach(TemplateDescription desc in cases)
            {
                Console.WriteLine($"REM {desc.Description}");
                Console.WriteLine($"mkdir {desc.Template}");
                Console.WriteLine($"cd {desc.Template}");
                foreach (Authentication authentication in Enum.GetValues(typeof(Authentication)))
                {
                    foreach(Calls calls in Enum.GetValues(typeof(Calls)))
                    {
                        if (!(authentication == Authentication.B2C  && calls == Calls.CallsGraph))
                        {
                            Console.WriteLine($"echo {desc.Template}{GetText(authentication)}{GetText(calls)}");
                            string folder = $"{desc.Template}-{Enum.GetName(typeof(Authentication), authentication)}-{Enum.GetName(typeof(Calls), calls)}";
                            Console.WriteLine($"mkdir {folder}");
                            Console.WriteLine($"cd {folder}");
                            Console.WriteLine($"dotnet new {desc.Template} {GetFlags(authentication)} {GetFlags(calls, authentication)}");
                            Console.WriteLine("cd ..");
                            Console.WriteLine();
                        }
                    }
                }
                Console.WriteLine("cd ..");
            }
        }
    }
}

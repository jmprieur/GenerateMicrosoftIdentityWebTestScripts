using System;
using System.Collections.Generic;

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
          new TemplateDescription {Template = "blazorserver2", Description = "Blazor server app"},
          new TemplateDescription {Template = "blazorwasm2", Description = "Blazor web assembly app"},
        };


        public static IEnumerable<bool> GetPossibleHosted(TemplateDescription templateDescription, Authentication authentication)
        {
            yield return false;

            if (templateDescription.Template == "blazorwasm2" && (authentication == Authentication.SingleOrg || authentication == Authentication.B2C))
            {
                yield return true;
            }
        }


        public static IEnumerable<Calls> GetPossibleCalls(TemplateDescription templateDescription, Authentication authentication, bool hosted)
        {
            yield return Calls.NoDownstreamApi;

            if (authentication == Authentication.SingleOrg || authentication == Authentication.B2C && (templateDescription.Template != "blazorwasm2" || hosted))
            {
                if (authentication != Authentication.B2C)
                {
                    yield return Calls.CallsGraph;
                }
                yield return Calls.CallsWebApi;
            }
        }


        public static string GetFlags(Authentication authentication)
        {
            switch (authentication)
            {
                case Authentication.NoAuth:
                    return string.Empty;
                case Authentication.SingleOrg:
                    return "--auth SingleOrg";
                case Authentication.B2C:
                    return "--auth IndividualB2C";
            }
            return string.Empty;
        }

        public static string GetText(Authentication authentication)
        {
            switch (authentication)
            {
                case Authentication.NoAuth:
                    return string.Empty;
                case Authentication.SingleOrg:
                    return "Single-Org";
                case Authentication.B2C:
                    return "B2C";
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
                        return "--called-api-url \"https://localhost:44332/api/todolist\" --called-api-scopes \"https://fabrikamb2c.onmicrosoft.com/tasks/read\"";
                    }
                    else
                    {
                        return "--called-api-url \"https://graph.microsoft.com/beta/me\" --called-api-scopes \"user.read\"";
                    }
                default:
                    return string.Empty;
            }
        }

        public static string GetFlags(bool hosted)
        {
            if (hosted)
            {
                return "--hosted";
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetText(bool hosted)
        {
            if (hosted)
            {
                return ", with hosted blazor server Web API";
            }
            else
            {
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
                    return ", calling Microsoft Graph";
                case Calls.CallsWebApi:
                    return ", calling a downstream web API";
                default:
                    return string.Empty;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("echo \"Test templates\"\n" +
                              "mkdir tests\n" +
                              "cd tests\n" +
                              "dotnet new sln --name tests\n");

            foreach (TemplateDescription desc in cases)
            {
                Console.WriteLine($"REM {desc.Description}");
                Console.WriteLine($"mkdir {desc.Template}");
                Console.WriteLine($"cd {desc.Template}");
                foreach (Authentication authentication in Enum.GetValues(typeof(Authentication)))
                {
                    foreach (bool hosted in GetPossibleHosted(desc, authentication))
                    {
                        foreach (Calls calls in GetPossibleCalls(desc, authentication, hosted))
                        {
                            GenerateSampleCreation(desc, authentication, calls, hosted);
                        }
                    }
                }
                Console.WriteLine("cd ..");
                Console.WriteLine();
            }
            Console.WriteLine("cd ..");
            Console.WriteLine("echo \"Build the solution with all the projects created by applying the templates\"");
            Console.WriteLine("dotnet build");
        }

        private static void GenerateSampleCreation(TemplateDescription desc, Authentication authentication, Calls calls, bool hosted)
        {
            Console.WriteLine($"echo \"Test {GetCommentName(desc, authentication, calls, hosted)}\"");
            string folder = GetFolderName(desc, authentication, calls, hosted);
            Console.WriteLine($"mkdir {folder}");
            Console.WriteLine($"cd {folder}");
            Console.WriteLine($"dotnet new {desc.Template} {GetFlags(authentication)} {GetFlags(calls, authentication)} {GetFlags(hosted)}");
            if (hosted)
            {
                Console.WriteLine($@"dotnet sln ..\..\tests.sln add Shared\{folder}.Shared.csproj");
                Console.WriteLine($@"dotnet sln ..\..\tests.sln add Server\{folder}.Server.csproj");
                Console.WriteLine($@"dotnet sln ..\..\tests.sln add Client\{folder}.Client.csproj");
            }
            else
            {
                Console.WriteLine($@"dotnet sln ..\..\tests.sln add {folder}.csproj");
            }
            Console.WriteLine("cd ..");
            Console.WriteLine();
        }

        private static string GetFolderName(TemplateDescription desc, Authentication authentication, Calls calls, bool hosted)
        {
            string authenticationText = Enum.GetName(typeof(Authentication), authentication);
            if (!string.IsNullOrEmpty(authenticationText))
            {
                authenticationText = "-" + authenticationText;
            }

            string callsText;
            if (calls == Calls.NoDownstreamApi)
            {
                callsText = string.Empty;
            }
            else
            {
                callsText = "-" + Enum.GetName(typeof(Calls), calls);
            }
            string hostedText = hosted ? "-hosted" : string.Empty;
            string folder = $"{desc.Template}{authenticationText}{callsText}{hostedText}".ToLowerInvariant();
            return folder;
        }

        private static string GetCommentName(TemplateDescription desc, Authentication authentication, Calls calls, bool hosted)
        {
            string authenticationText = GetText(authentication);
            if (!string.IsNullOrEmpty(authenticationText))
            {
                authenticationText = ", " + authenticationText;
            }
            else
            {
                authenticationText = ", no auth";
            }

            string callsText;
            if (calls == Calls.NoDownstreamApi)
            {
                callsText = string.Empty;
            }
            else
            {
                callsText = GetText(calls);
            }
            string hostedText = GetText(hosted);
            string comment = $"{desc.Template}{authenticationText}{hostedText}{callsText}".ToLowerInvariant();
            return comment;
        }
    }
}

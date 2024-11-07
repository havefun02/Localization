using Localization.Interfaces;

namespace Localization.Services
{
    public class GreetingService:IGreetingService
    {
        public string Greeting()
        {
            return "heelo";
        }
    }
}

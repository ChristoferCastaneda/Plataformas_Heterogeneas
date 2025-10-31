using Cookies.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cookies.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISessionService _sessionService;
        private const string SessionCookieName = "AuthSessionId";

        public HomeController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!Request.Cookies.TryGetValue(SessionCookieName, out var sessionId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _sessionService.ValidateSessionAsync(sessionId);

            if (user == null)
            {
                Response.Cookies.Delete(SessionCookieName);
                return RedirectToAction("Login", "Auth");
            }

            return View(user);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
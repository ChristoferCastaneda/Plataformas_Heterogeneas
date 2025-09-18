using Humanizer;
using Microsoft.AspNetCore.Mvc;
using NumericSeries.Models;

namespace NumericSeries.Controllers
{
    public class SeriesController : Controller
    {
        [HttpGet("/series/{series}/{n}")]
        public IActionResult Index(string series, int n = 0)
        {
            // Si no existe da error 404
            if (!SeriesViewModel.IsValidSeries(series))
            {
                return NotFound();
            }
            if (!SeriesViewModel.IsValidNumber(n))
            {
                return BadRequest();
            }
            try
            {
                var viewModel = new SeriesViewModel()
                {
                    Series = series.ApplyCase(LetterCasing.Sentence),
                    N = n
                };
                

                viewModel.SelectFun();

                return View(viewModel);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}

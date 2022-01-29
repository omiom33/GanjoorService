﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMuseum.Models.Ganjoor.ViewModels;
using RMuseum.Services;
using System.Net;
using System.Threading.Tasks;

namespace RMuseum.Controllers
{
    [Produces("application/json")]
    [Route("api/poetspecs")]
    public class GanjoorPoetSuggestedSpecLinesController : Controller
    {
        /// <summary>
        /// returns list of suggested spec lines for a poet
        /// </summary>
        /// <param name="id">poet id</param>
        /// <returns></returns>
        [HttpGet("poet/{id}")]
        [AllowAnonymous]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(GanjoorPoetSuggestedSpecLineViewModel[]))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetPoetSuggestedSpecLinesAsync(int id)
        {
            var res = await _ganjoorService.GetPoetSuggestedSpecLinesAsync(id);
            if (!string.IsNullOrEmpty(res.ExceptionString))
                return BadRequest(res.ExceptionString);
            return Ok(res.Result);
        }



        /// <summary>
        /// Ganjoor Service
        /// </summary>
        protected readonly IGanjoorService _ganjoorService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ganjoorService"></param>
        public GanjoorPoetSuggestedSpecLinesController(IGanjoorService ganjoorService)
        {
            _ganjoorService = ganjoorService;
        }
    }
}

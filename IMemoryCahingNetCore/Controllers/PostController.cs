using IMemoryCahingNetCore;
using IMemoryCahingNetCore.Dto;
using IMemoryCahingNetCore.Dto.Model;
using IMemoryCahingNetCore.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IMemoryCahingNetCore.ActionFilters;

namespace IMemoryCahingNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        public PostController()
        {

        }

        [HttpGet]
        [ProducesResponseType(typeof(ResultDto<Post>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [CacheActionFilter("GetPost")]
        public async Task<IActionResult> GetPosts()
        {
            ResultDto<Post> resultDto = new ResultDto<Post>();
            List<Post> posts = await ApiHandler<Post>.GetAll("https://jsonplaceholder.typicode.com/posts");
            resultDto.Data = posts;

            return Ok(new { resultDto });
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ResultDto<Post>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPostById(int id)
        {
            // method without filter
            ResultDto<Post> resultDto = new ResultDto<Post>();
            Post posts = await ApiHandler<Post>.GetById("https://jsonplaceholder.typicode.com/posts/" + id);
            resultDto.Data = new List<Post>() { posts };

            return Ok(new { result = resultDto });
        }
    }
}

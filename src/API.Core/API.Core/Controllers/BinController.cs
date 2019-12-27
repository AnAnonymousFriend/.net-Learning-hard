﻿using System.Threading.Tasks;
using API.Core.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Core.Controllers
{
    [Produces("application/json")]
    [Route("api/Bin")]

    [ApiController]
    public class BinController : ControllerBase
    {
        
        private readonly IBinServices _binArticleServices;
        public BinController(IBinServices BindvertisementServices)
        {
           
            this._binArticleServices = BindvertisementServices;
        }


        /// <summary>
        /// 获取Bin
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        
        public async Task<object> GetBin()
        {
            var model = await _binArticleServices.GetBinList();//调用该方法，这里 _blogArticleServices 是依赖注入的实例，不是类
            var data = new { success = true, data = model };
            return data;
        }

    }
}
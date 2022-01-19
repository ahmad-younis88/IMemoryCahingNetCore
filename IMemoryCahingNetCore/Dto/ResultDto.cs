using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace IMemoryCahingNetCore.Dto
{
    public class ResultDto<T> where T : class
    {
        public ResultDto()
        {
            StatusCode = HttpStatusCode.OK;
            Message = "Success";
        }

        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public List<T> Data { get; set; }
    }
}

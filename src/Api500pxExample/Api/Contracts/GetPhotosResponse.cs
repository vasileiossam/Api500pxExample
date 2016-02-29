using System.Collections.Generic;
using System.Runtime.Serialization;
using Api500pxExample.Api.Models;

namespace Api500pxExample.Api.Contracts
{
    [DataContract]
    public class GetPhotosResponse: PagedResponse
    {
        [DataMember(Name = "photos")]
        public List<Photo> Photos { get; set; }
    }
}

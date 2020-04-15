using Covid.Api.ResponseTypes;
using Covid.Common.Mapper;
using Covid.Repository.Facades;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Covid.Api.Controllers
{
    public class CovidControllerBase : ControllerBase
    {
        protected readonly ICovidRepositoryFacade _repositoryFacade;
        protected readonly IMapper _mapper;

        public CovidControllerBase(
            ICovidRepositoryFacade repositoryFacade,
            IMapper mapper)
        {
            _repositoryFacade = repositoryFacade ?? throw new ArgumentNullException(nameof(repositoryFacade));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public InternalServerErrorObjectResult InternalServerError()
        {
            return new InternalServerErrorObjectResult();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public InternalServerErrorObjectResult InternalServerError(object value)
        {
            return new InternalServerErrorObjectResult(value);
        }
    }
}

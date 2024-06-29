using AutoMapper;
using OCR_API.ModelsDto;

namespace OCR_API.Services
{
    public interface IPaginationService
    {
        PageResults<T> PreparePaginationResults<T, T2>(GetAllQuery queryParameters, IQueryable<T2> query, IMapper mapper);
    }

    public class PaginationService : IPaginationService
    {
        public PageResults<T> PreparePaginationResults<T, T2>(GetAllQuery queryParameters, IQueryable<T2> query, IMapper mapper)
        {
            int resultCount;
            List<T> resultDto;
            PageResults<T> result;
            if (queryParameters == null || queryParameters.PageNumber == 0 || queryParameters.PageSize == 0)
            {
                resultCount = query.Count();
                resultDto = query.Select(f => mapper.Map<T>(f)).ToList();
                result = new PageResults<T>(resultDto, resultCount, resultCount, 1);
            }
            else
            {
                int resultsToSkip = queryParameters.PageSize * (queryParameters.PageNumber - 1);
                resultCount = query.Count();
                var resultQuery = query.Skip(resultsToSkip).Take(queryParameters.PageSize);
                resultDto = resultQuery.Select(f => mapper.Map<T>(f)).ToList();
                result = new PageResults<T>(resultDto, resultCount, queryParameters.PageSize, queryParameters.PageNumber);
            }

            return result;
        }
    }
}
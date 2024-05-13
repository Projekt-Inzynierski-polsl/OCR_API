using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.NoteFileDtos;
using OCR_API.ModelsDto.UploadedModelDtos;
using OCR_API.ModelsDto.UserLogDtos;
using OCR_API.Specifications;
using OCR_API.Transactions;
using System.IO.Compression;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OCR_API.Services
{

    public interface IUserLogService
    {
        public IUnitOfWork UnitOfWork { get; }
        PageResults<UserLogDto> Get(GetAllQuery queryParameters, string type, long startTimestamp, long endTimestamp, int userId);
    }

    public class UserLogService : IUserLogService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly IPaginationService paginationService;

        public UserLogService(IUnitOfWork unitOfWork, IMapper mapper, IPaginationService paginationService)
        {
            UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.paginationService = paginationService;
        }

        public PageResults<UserLogDto> Get(GetAllQuery queryParameters, string type, long startTimestamp, long endTimestamp, int userId)
        {
            if(Enum.TryParse(typeof(EUserAction), type, true, out var actionType))
            {
                EUserAction action = (EUserAction)actionType;
                DateTime startDateTime = ConvertTimestampToDateTime(startTimestamp);
                DateTime endDateTime = ConvertTimestampToDateTime(endTimestamp);
                var spec = new UserLogByTypeAndDateSpecification(action, startDateTime, endDateTime, userId);
                var actions = UnitOfWork.UserLogs.GetBySpecification(spec);
                var result = paginationService.PreparePaginationResults<UserLogDto, UserLog>(queryParameters, actions, mapper);
                return result;
            }
            else
            {
                throw new BadRequestException("Cannot parse userAction to enumType.");
            }
        }

        private DateTime ConvertTimestampToDateTime(long timestamp)
        {
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            DateTime dateTime = offset.LocalDateTime;
            return dateTime;
        }
    }

}

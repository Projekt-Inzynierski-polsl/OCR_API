using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/note")]
    [ApiController]
    [Authorize]
    public class NoteControler : ControllerBase
    {
        private readonly INoteService noteService;

        public NoteControler(INoteService noteService)
        {
            this.noteService = noteService;
        }

    }
}

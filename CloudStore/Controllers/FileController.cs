using CloudStore.BL.Models;
using CloudStore.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace CloudStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly CloudStoreDbHelper _dbHelper;
        private readonly string _userDirectory;

        public FileController(IWebHostEnvironment webHostEnvironment, CloudStoreDbHelper dbHelper)
        {
            _webHostEnvironment = webHostEnvironment;
            _dbHelper = dbHelper;
            _userDirectory = _webHostEnvironment.ContentRootPath + "\\Files";
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var fileToDownload = await _dbHelper.GetFileByIdAsync(id);

            if (fileToDownload == null)
                return NotFound();

            string filePath = Path.Combine(_userDirectory,
                                            $"{fileToDownload.Path}");
            string file_type = $"application/{fileToDownload.Extension}";
            string file_name = $"{fileToDownload.Name}";

            return PhysicalFile(filePath, file_type, file_name);
        }

        [HttpGet("{id}")]
        public async Task<FileModel?> GetFileAsync(int id) =>
            await _dbHelper.GetFileByIdAsync(id);

        [HttpGet("AllFiles")]
        public async Task<IEnumerable<FileModel>?> GetAsync() =>
            await _dbHelper.GetAllFilesAsync();

        [HttpPost]
        public async Task Post([FromBody] FileModel file) =>
            await _dbHelper.AddFileAsync(file);

        [HttpPut]
        public async Task Put([FromBody] FileModel file) =>
            await _dbHelper.UpdateFile(file);

        [HttpDelete("{id}")]
        public async Task Delete(int id) =>
            await _dbHelper.DeleteFileByIdAsync(id);

        //[Route("api/directory")]
        [HttpPost("new-directory/{directory}")]
        public IActionResult PostDirectory(string directory)
        {
            var newDirectory = Path.Combine(_userDirectory, directory);

            if (Directory.Exists(newDirectory))
                return Ok("Directory is already exist");

            try
            {
                Directory.CreateDirectory(newDirectory);
            }
            catch (IOException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("new Directory was created");
        }
        [HttpPost("upload-file")]
        public async Task<IActionResult> AddFileAsync(IFormFile uploadedFile, string? directory = "")
        {
            if (uploadedFile is null) 
                return BadRequest();
            if (!Directory.Exists(Path.Combine(_userDirectory + directory)))
                return NotFound("Directory is not exist");

            var path = _userDirectory + "\\" + directory + "\\"  + uploadedFile.FileName;
            
            using var fs = new FileStream(path,FileMode.Create);
            await uploadedFile.CopyToAsync(fs);

            var temp = uploadedFile.FileName.Split('.');

            var newFile = new FileModel
            {
                Name = uploadedFile.FileName,
                Path = directory + "\\" + uploadedFile.FileName,
                Extension = temp[^1]
            };
            await _dbHelper.AddFileAsync(newFile);

            return Ok("New file is added");
        }
    }
}